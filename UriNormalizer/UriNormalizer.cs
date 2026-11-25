// ------------------------------------------------------------------------------
// © SDK GmbH 
// ------------------------------------------------------------------------------

using System.Globalization;
using System.Text;

namespace BdewUriNormalization;

/// <summary>
/// Normalizes a URI strictly according to RFC 3986 Sections 6.2.2 and 6.2.3.
/// </summary>
public static class UriNormalizer
{
    public static string Normalize(string input)
    {
        // 1. Parse raw components without System.Uri side effects
        ParsedUri uri = ParseUri(input);

        // 2. Lowercase scheme (RFC 3986 Section 6.2.2.1)
        uri.Scheme = uri.Scheme.ToLowerInvariant();

        // Validate HTTP/HTTPS authority requirement
        if ((uri.Scheme == "http" || uri.Scheme == "https") &&
            (!uri.HasAuthority || string.IsNullOrEmpty(uri.Host)))
        {
            throw new FormatException($"URI with scheme '{uri.Scheme}' must contain a non-empty host.");
        }

        // 3 & 4. Normalize percent-encoding across all components:
        //        - Uppercase hex digits (%xx -> %XX) (6.2.2.1)
        //        - Decode unreserved characters (6.2.2.2)
        //        Note: Run this on host BEFORE lowercasing host so encoded
        //        uppercase ASCII letters like %41 ('A') also become lowercase ('a').
        uri.UserInfo = uri.UserInfo is null ? null : NormalizePercentEncoding(uri.UserInfo);
        uri.Host = uri.Host is null ? null : NormalizeHostCase(NormalizePercentEncoding(uri.Host));
        uri.Path = NormalizePercentEncoding(uri.Path);
        uri.Query = uri.Query is null ? null : NormalizePercentEncoding(uri.Query);
        uri.Fragment = uri.Fragment is null ? null : NormalizePercentEncoding(uri.Fragment);

        // 5. Remove dot segments from path using RFC 3986 Section 5.2.4 (6.2.2.3)
        uri.Path = RemoveDotSegments(uri.Path);

        // 6. Scheme-based normalization: default/empty ports & empty path (6.2.3)
        NormalizeSchemeSpecificParts(ref uri);

        // 7. Reconstruct normalized URI preserving delimiters (?, #, @, trailing '/')
        return SerializeUri(uri);
    }

    private static ParsedUri ParseUri(string input)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(input);

        for (int i = 0; i < input.Length; i++)
        {
            if (char.IsControl(input[i]) || char.IsWhiteSpace(input[i]))
            {
                throw new FormatException("URI must not contain whitespace or control characters.");
            }
        }

        // Split #fragment
        int fragmentIndex = input.IndexOf('#');
        bool hasFragment = fragmentIndex >= 0;
        string? fragment = hasFragment ? input[(fragmentIndex + 1)..] : null;
        if (fragment is not null && fragment.Contains('#'))
        {
            throw new FormatException("URI fragment must not contain '#'.");
        }

        string beforeFragment = hasFragment ? input[..fragmentIndex] : input;

        // Split ?query
        int queryIndex = beforeFragment.IndexOf('?');
        bool hasQuery = queryIndex >= 0;
        string? query = hasQuery ? beforeFragment[(queryIndex + 1)..] : null;
        string beforeQuery = hasQuery ? beforeFragment[..queryIndex] : beforeFragment;

        // Split scheme:hier-part
        int colonIndex = beforeQuery.IndexOf(':');
        if (colonIndex <= 0)
        {
            throw new FormatException("URI must begin with a valid scheme followed by ':'.");
        }

        string scheme = beforeQuery[..colonIndex];
        if (!IsValidScheme(scheme))
        {
            throw new FormatException($"Invalid URI scheme '{scheme}'.");
        }

        string hierPart = beforeQuery[(colonIndex + 1)..];

        // Split //authority and path
        bool hasAuthority = hierPart.StartsWith("//", StringComparison.Ordinal);
        string? userInfo = null;
        string? host = null;
        bool hasPort = false;
        string? port = null;
        string path;

        if (hasAuthority)
        {
            string afterDoubleSlash = hierPart[2..];
            int pathStart = afterDoubleSlash.IndexOf('/');
            string authority = pathStart >= 0 ? afterDoubleSlash[..pathStart] : afterDoubleSlash;
            path = pathStart >= 0 ? afterDoubleSlash[pathStart..] : string.Empty;

            (userInfo, host, hasPort, port) = ParseAuthority(authority);
        }
        else
        {
            path = hierPart;
        }

        return new ParsedUri
        {
            Scheme = scheme,
            HasAuthority = hasAuthority,
            UserInfo = userInfo,
            Host = host,
            HasPort = hasPort,
            Port = port,
            Path = path,
            HasQuery = hasQuery,
            Query = query,
            HasFragment = hasFragment,
            Fragment = fragment
        };
    }

    private static (string? UserInfo, string Host, bool HasPort, string? Port) ParseAuthority(string authority)
    {
        if (authority.Length == 0)
        {
            return (null, string.Empty, false, null);
        }

        string? userInfo = null;
        string hostPort = authority;

        int atIndex = authority.IndexOf('@');
        if (atIndex >= 0)
        {
            if (authority.IndexOf('@', atIndex + 1) >= 0)
            {
                throw new FormatException("Invalid URI authority: multiple '@' characters.");
            }

            userInfo = authority[..atIndex];
            hostPort = authority[(atIndex + 1)..];
        }

        string host;
        bool hasPort = false;
        string? port = null;

        // Handle IPv6 / IP-literal enclosed in [...]
        if (hostPort.StartsWith('['))
        {
            int closeBracket = hostPort.IndexOf(']');
            if (closeBracket < 0)
            {
                throw new FormatException("Invalid URI host: missing closing ']'.");
            }

            host = hostPort[..(closeBracket + 1)];

            if (closeBracket < hostPort.Length - 1)
            {
                if (hostPort[closeBracket + 1] != ':')
                {
                    throw new FormatException("Invalid URI authority after IP literal.");
                }

                hasPort = true;
                port = hostPort[(closeBracket + 2)..];
            }
        }
        else
        {
            int colonIndex = hostPort.IndexOf(':');
            if (colonIndex >= 0)
            {
                if (hostPort.IndexOf(':', colonIndex + 1) >= 0)
                {
                    throw new FormatException("Invalid URI authority: multiple ':' characters.");
                }

                host = hostPort[..colonIndex];
                hasPort = true;
                port = hostPort[(colonIndex + 1)..];
            }
            else
            {
                host = hostPort;
            }

            if (host.Contains('[') || host.Contains(']'))
            {
                throw new FormatException("Invalid '[' or ']' in URI host.");
            }
        }

        if (hasPort && !IsDigitsOnly(port!))
        {
            throw new FormatException($"Invalid URI port '{port}'.");
        }

        return (userInfo, host, hasPort, port);
    }

    private static string NormalizePercentEncoding(string input)
    {
        if (!input.Contains('%'))
        {
            return input;
        }

        var result = new StringBuilder(input.Length);

        for (int i = 0; i < input.Length; i++)
        {
            if (input[i] != '%')
            {
                result.Append(input[i]);
                continue;
            }

            if (i + 2 >= input.Length ||
                !IsHexDigit(input[i + 1]) ||
                !IsHexDigit(input[i + 2]))
            {
                throw new FormatException($"Invalid percent-encoding in '{input}' at index {i}.");
            }

            int octet = (HexValue(input[i + 1]) << 4) | HexValue(input[i + 2]);
            char decoded = (char)octet;

            if (IsUnreserved(decoded))
            {
                result.Append(decoded);
            }
            else
            {
                result.Append('%');
                result.Append(char.ToUpperInvariant(input[i + 1]));
                result.Append(char.ToUpperInvariant(input[i + 2]));
            }

            i += 2;
        }

        return result.ToString();
    }

    private static string NormalizeHostCase(string host)
    {
        // Lowercases host characters while keeping %XX hex digits uppercase.
        var result = new StringBuilder(host.Length);

        for (int i = 0; i < host.Length; i++)
        {
            if (host[i] == '%' && i + 2 < host.Length)
            {
                result.Append('%');
                result.Append(char.ToUpperInvariant(host[i + 1]));
                result.Append(char.ToUpperInvariant(host[i + 2]));
                i += 2;
            }
            else
            {
                result.Append(char.ToLowerInvariant(host[i]));
            }
        }

        return result.ToString();
    }

    // Exact implementation of RFC 3986 Section 5.2.4 (remove_dot_segments)
    private static string RemoveDotSegments(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return path;
        }

        string input = path;
        var output = new StringBuilder(path.Length);

        while (input.Length > 0)
        {
            // 2A: If input begins with "../" or "./", remove that prefix.
            if (input.StartsWith("../", StringComparison.Ordinal))
            {
                input = input[3..];
            }
            else if (input.StartsWith("./", StringComparison.Ordinal))
            {
                input = input[2..];
            }
            // 2B: If input begins with "/./" or is "/.", replace prefix with "/".
            else if (input.StartsWith("/./", StringComparison.Ordinal))
            {
                input = input[2..];
            }
            else if (input == "/.")
            {
                input = "/";
            }
            // 2C: If input begins with "/../" or is "/..", replace prefix with "/"
            //     and remove last segment and its preceding "/" from output.
            else if (input.StartsWith("/../", StringComparison.Ordinal))
            {
                input = input[3..];
                RemoveLastSegment(output);
            }
            else if (input == "/..")
            {
                input = "/";
                RemoveLastSegment(output);
            }
            // 2D: If input consists only of "." or "..", remove it.
            else if (input == "." || input == "..")
            {
                input = string.Empty;
            }
            // 2E: Move the first path segment (including initial '/' if any) to output.
            else
            {
                int searchStart = input.StartsWith('/') ? 1 : 0;
                int nextSlash = input.IndexOf('/', searchStart);

                if (nextSlash < 0)
                {
                    output.Append(input);
                    input = string.Empty;
                }
                else
                {
                    output.Append(input, 0, nextSlash);
                    input = input[nextSlash..];
                }
            }
        }

        return output.ToString();
    }

    private static void RemoveLastSegment(StringBuilder output)
    {
        for (int i = output.Length - 1; i >= 0; i--)
        {
            if (output[i] == '/')
            {
                output.Length = i;
                return;
            }
        }

        output.Length = 0;
    }

    private static void NormalizeSchemeSpecificParts(ref ParsedUri uri)
    {
        // Remove empty port (e.g. "http://example.com:/") or default port for http/https
        if (uri.HasPort && ShouldRemovePort(uri.Scheme, uri.Port!))
        {
            uri.HasPort = false;
            uri.Port = null;
        }

        // Normalize empty path to "/" when an authority is present
        if (uri.HasAuthority && uri.Path.Length == 0)
        {
            uri.Path = "/";
        }
    }

    private static bool ShouldRemovePort(string scheme, string port)
    {
        if (port.Length == 0)
        {
            return true;
        }

        if (int.TryParse(port, NumberStyles.None, CultureInfo.InvariantCulture, out int portNumber))
        {
            return (scheme == "http" && portNumber == 80) ||
                   (scheme == "https" && portNumber == 443);
        }

        return false;
    }

    private static string SerializeUri(ParsedUri uri)
    {
        var sb = new StringBuilder();
        sb.Append(uri.Scheme);
        sb.Append(':');

        if (uri.HasAuthority)
        {
            sb.Append("//");

            if (uri.UserInfo is not null)
            {
                sb.Append(uri.UserInfo);
                sb.Append('@');
            }

            sb.Append(uri.Host);

            if (uri.HasPort)
            {
                sb.Append(':');
                sb.Append(uri.Port);
            }
        }

        sb.Append(uri.Path);

        if (uri.HasQuery)
        {
            sb.Append('?');
            sb.Append(uri.Query);
        }

        if (uri.HasFragment)
        {
            sb.Append('#');
            sb.Append(uri.Fragment);
        }

        return sb.ToString();
    }

    private static bool IsValidScheme(string scheme)
    {
        if (scheme.Length == 0 || !IsAsciiLetter(scheme[0]))
        {
            return false;
        }

        for (int i = 1; i < scheme.Length; i++)
        {
            char c = scheme[i];
            if (!IsAsciiLetter(c) && !IsAsciiDigit(c) && c != '+' && c != '-' && c != '.')
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsUnreserved(char c) =>
        IsAsciiLetter(c) ||
        IsAsciiDigit(c) ||
        c == '-' || c == '.' || c == '_' || c == '~';

    private static bool IsAsciiLetter(char c) =>
        (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z');

    private static bool IsAsciiDigit(char c) =>
        c >= '0' && c <= '9';

    private static bool IsDigitsOnly(string value)
    {
        foreach (char c in value)
        {
            if (!IsAsciiDigit(c))
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsHexDigit(char c) =>
        IsAsciiDigit(c) || (c >= 'A' && c <= 'F') || (c >= 'a' && c <= 'f');

    private static int HexValue(char c) => c switch
    {
        >= '0' and <= '9' => c - '0',
        >= 'A' and <= 'F' => c - 'A' + 10,
        >= 'a' and <= 'f' => c - 'a' + 10,
        _ => throw new FormatException($"Invalid hex digit '{c}'.")
    };

    private struct ParsedUri
    {
        public string Scheme;
        public bool HasAuthority;
        public string? UserInfo;
        public string? Host;
        public bool HasPort;
        public string? Port;
        public string Path;
        public bool HasQuery;
        public string? Query;
        public bool HasFragment;
        public string? Fragment;
    }
}
