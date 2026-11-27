// ------------------------------------------------------------------------------
// © SDK GmbH 
// ------------------------------------------------------------------------------

using System;
using System.Text;


/// <summary>
/// Pure URI normalizer implementing RFC 3986 Sections 6.2.2 and 6.2.3
/// (including Section 5.2.4 dot-segment removal).
/// </summary>
public static class UriNormalizer
{
    private const string HexDigits = "0123456789ABCDEF";

    public static string Normalize(string uri)
    {
        ArgumentNullException.ThrowIfNull(uri);

        int end = uri.Length;

        // 1. Extract fragment (#...) and query (?...) per RFC 3986 Appendix B
        int hashIdx = uri.IndexOf('#');
        bool hasFragment = hashIdx >= 0;
        string fragment = hasFragment ? uri.Substring(hashIdx + 1) : string.Empty;
        if (hasFragment)
        {
            end = hashIdx;
        }

        int queryIdx = uri.IndexOf('?', 0, end);
        bool hasQuery = queryIdx >= 0;
        string query = hasQuery ? uri.Substring(queryIdx + 1, end - queryIdx - 1) : string.Empty;
        if (hasQuery)
        {
            end = queryIdx;
        }

        // 2. Extract scheme (must appear before any '/')
        int pos = 0;
        bool hasScheme = false;
        string scheme = string.Empty;
        for (int i = 0; i < end; i++)
        {
            char c = uri[i];
            if (c == '/')
            {
                break;
            }
            if (c == ':')
            {
                if (i > 0)
                {
                    hasScheme = true;
                    scheme = NormalizeAsciiLower(uri.Substring(0, i)); // 6.2.2.1
                    pos = i + 1;
                }
                break;
            }
        }

        // 3. Extract authority (//...)
        bool hasAuthority = pos + 1 < end && uri[pos] == '/' && uri[pos + 1] == '/';
        bool hasUserinfo = false;
        string userinfo = string.Empty;
        string host = string.Empty;
        bool hasPort = false;
        string port = string.Empty;

        if (hasAuthority)
        {
            pos += 2;
            int slashIdx = uri.IndexOf('/', pos, end - pos);
            int authEnd = slashIdx >= 0 ? slashIdx : end;
            string authority = uri.Substring(pos, authEnd - pos);
            pos = authEnd;

            int atIdx = authority.LastIndexOf('@');
            int hostStart = 0;
            if (atIdx >= 0)
            {
                hasUserinfo = true;
                userinfo = NormalizePercentEncoding(authority.Substring(0, atIdx), false);
                hostStart = atIdx + 1;
            }

            int colonIdx = Utility.FindPortColon(authority, hostStart);
            if (colonIdx >= 0)
            {
                // Percent-decode unreserved chars BEFORE lowercasing host (e.g. %45 -> E -> e)
                host = NormalizePercentEncoding(authority.Substring(hostStart, colonIdx - hostStart), true);
                port = authority.Substring(colonIdx + 1);
                hasPort = !IsEmptyOrDefaultPort(scheme, port); // 6.2.3
            }
            else
            {
                host = NormalizePercentEncoding(authority.Substring(hostStart), true);
            }

            // 6.2.3: empty authority default for "file" when userinfo and port are absent
            if (hasScheme && scheme == "file" && !hasUserinfo && !hasPort && host == "localhost")
            {
                host = string.Empty;
            }
        }

        // 4. Normalize path: percent-encoding (6.2.2.2) BEFORE dot-segments (6.2.2.3 / 5.2.4)
        string path = uri.Substring(pos, end - pos);
        if (hasScheme && scheme == "mailto" && !hasAuthority)
        {
            path = NormalizeMailtoPath(path); // 6.2.3
        }
        else
        {
            path = NormalizePercentEncoding(path, false);
            path = RemoveDotSegments(path, hasAuthority);
        }

        // 6.2.3: empty path with authority normalizes to "/"
        if (hasAuthority && path.Length == 0)
        {
            path = "/";
        }

        // 5. Normalize query & fragment (keep empty '?' and '#' delimiters per 6.2.3)
        if (hasQuery)
        {
            query = NormalizePercentEncoding(query, false);
        }
        if (hasFragment)
        {
            fragment = NormalizePercentEncoding(fragment, false);
        }

        // 6. Reassemble
        var output = new StringBuilder(uri.Length);
        if (hasScheme)
        {
            output.Append(scheme).Append(':');
        }
        if (hasAuthority)
        {
            output.Append("//");
            if (hasUserinfo)
            {
                output.Append(userinfo).Append('@');
            }
            output.Append(host);
            if (hasPort)
            {
                output.Append(':').Append(port);
            }
        }
        output.Append(path);
        if (hasQuery)
        {
            output.Append('?').Append(query);
        }
        if (hasFragment)
        {
            output.Append('#').Append(fragment);
        }

        return output.ToString();
    }

    // 6.2.2.1 & 6.2.2.2: Decode unreserved %XX, uppercase reserved %XX. Optionally lowercase ASCII (including the decoded %XX)
    private static string NormalizePercentEncoding(string s, bool lowercaseAscii)
    {
        StringBuilder? sb = null;
        for (int i = 0; i < s.Length; i++)
        {
            char c = s[i];
            bool isPercentEncoding = c == '%' && i + 2 < s.Length;
            if (isPercentEncoding)
            {
                int hi = Utility.HexValue(s[i + 1]);
                int lo = Utility.HexValue(s[i + 2]);
                if (hi >= 0 && lo >= 0)
                {
                    int b = (hi << 4) | lo;
                    if (Utility.IsUnreserved(b))
                    {
                        DecodeLowercaseUnreserved(s, lowercaseAscii, ref sb, i, b);
                    }
                    else
                    {
                        UppercaseReserved(s, ref sb, i, hi, lo);
                    }

                    i += 2;
                    continue;
                }
            }

            bool shouldLowercaseAscii = lowercaseAscii && c >= 'A' && c <= 'Z';
            if (shouldLowercaseAscii)
            {
                if (sb == null)
                {
                    sb = new StringBuilder(s.Length);
                    sb.Append(s, 0, i);
                }
                sb.Append((char)(c + 32));
            }
            else if (sb != null)
            {
                sb.Append(c);
            }
        }

        return sb == null ? s : sb.ToString();

        static void UppercaseReserved(string s, ref StringBuilder? sb, int i, int hi, int lo)
        {
            char upperHi = HexDigits[hi];
            char upperLo = HexDigits[lo];
            if (sb != null || upperHi != s[i + 1] || upperLo != s[i + 2])
            {
                if (sb == null)
                {
                    sb = new StringBuilder(s.Length);
                    sb.Append(s, 0, i);
                }
                sb.Append('%').Append(upperHi).Append(upperLo);
            }
        }

        static void DecodeLowercaseUnreserved(string s, bool lowercaseAscii, ref StringBuilder? sb, int i, int b)
        {
            if (sb == null)
            {
                sb = new StringBuilder(s.Length);
                sb.Append(s, 0, i);
            }
            char decoded = (char)b;
            if (lowercaseAscii && decoded >= 'A' && decoded <= 'Z')
                decoded = (char)(decoded + 32);

            sb.Append(decoded);
        }
    }

    // 5.2.4: O(N) cursor-based remove_dot_segments (with RFC 3986 §3.3 '//' guard when !hasAuthority)
    private static string RemoveDotSegments(string path, bool hasAuthority)
    {
        if (path.IndexOf('.') < 0)
        {
            return path;
        }

        int len = path.Length;
        var output = new StringBuilder(len);
        int i = 0;

        while (i < len)
        {
            if (Utility.StartsWithAt(path, i, "../"))                          // 2A
            {
                i += 3;
            }
            else if (Utility.StartsWithAt(path, i, "./"))                      // 2A
            {
                i += 2;
            }
            else if (Utility.StartsWithAt(path, i, "/./")                      // 2B
                  && (hasAuthority || output.Length > 0 || !Utility.StartsWithAt(path, i, "/.//")))
            {
                i += 2;
            }
            else if (i + 2 == len && Utility.StartsWithAt(path, i, "/."))      // 2B
            {
                output.Append('/');
                break;
            }
            else if (Utility.StartsWithAt(path, i, "/../")                     // 2C
                  && (hasAuthority || Utility.LastSlashIndex(output) > 0 || !Utility.StartsWithAt(path, i, "/..//")))
            {
                int lastSlash = Utility.LastSlashIndex(output);
                output.Length = lastSlash >= 0 ? lastSlash : 0;
                i += 3;
            }
            else if (i + 3 == len && Utility.StartsWithAt(path, i, "/.."))     // 2C
            {
                int lastSlash = Utility.LastSlashIndex(output);
                output.Length = lastSlash >= 0 ? lastSlash : 0;
                output.Append('/');
                break;
            }
            else if ((i + 1 == len && path[i] == '.')                  // 2D
                  || (i + 2 == len && Utility.StartsWithAt(path, i, "..")))
            {
                break;
            }
            else                                                       // 2E
            {
                int start = i;
                if (path[i] == '/')
                {
                    i++;
                }
                int nextSlash = path.IndexOf('/', i);
                int segmentEnd = nextSlash < 0 ? len : nextSlash;
                output.Append(path, start, segmentEnd - start);
                i = segmentEnd;
            }
        }

        return output.ToString();
    }

    /// <summary> Handles the special mailto: rule. In mailto:Joe@Example.COM, the part before @ (Joe) is case-sensitive,
    /// while the domain after @ (Example.COM) is case-insensitive.
    /// </summary>
    private static string NormalizeMailtoPath(string path)
    {
        int atIdx = path.LastIndexOf('@');
        if (atIdx < 0)
            return NormalizePercentEncoding(path, false);

        return NormalizePercentEncoding(path.Substring(0, atIdx), false)
             + '@'
             + NormalizePercentEncoding(path.Substring(atIdx + 1), true);
    }

    /// <summary> Decides whether a port should be removed during normalization. Following Section 6.2.3 (Scheme-Based Normalization) </summary>
    private static bool IsEmptyOrDefaultPort(string scheme, string port)
    {
        if (port.Length == 0)
        {
            return true;
        }

        string defaultPort = Utility.DefaultPort(scheme);
        if (defaultPort.Length == 0)
        {
            return false;
        }

        int start = 0;
        int len = port.Length;
        while (start < len - 1 && port[start] == '0') // e.g. :080 -> 80
        {
            start++;
        }

        return (len - start) == defaultPort.Length
            && string.CompareOrdinal(port, start, defaultPort, 0, defaultPort.Length) == 0;
    }

    /// <summary> Converts only ASCII uppercase letters ('A'–'Z') to lowercase ('a'–'z') </summary>
    public static string NormalizeAsciiLower(string s)
    {
        int len = s.Length;
        StringBuilder? sb = null;
        for (int i = 0; i < len; i++)
        {
            char c = s[i];
            if (c >= 'A' && c <= 'Z')
            {
                if (sb == null)
                {
                    sb = new StringBuilder(len);
                    sb.Append(s, 0, i);
                }
                sb.Append((char)(c + 32));
            }
            else if (sb != null)
            {
                sb.Append(c);
            }
        }
        return sb == null ? s : sb.ToString();
    }
}


/// <summary> Helper for UriNormalizer. None of these methods modify any strings. </summary>
public static class Utility
{
    /// <summary> Converts a single hexadecimal character ('0'–'9', 'A'–'F', 'a'–'f') into its numeric value (0–15).
    /// Returns -1 if c is not a valid hex digit.</summary> 
    public static int HexValue(char c)
    {
        if (c >= '0' && c <= '9') return c - '0';
        if (c >= 'A' && c <= 'F') return c - 'A' + 10;
        if (c >= 'a' && c <= 'f') return c - 'a' + 10;
        return -1;
    }

    /// <summary> Checks whether a byte value is an unreserved character in RFC 3986: A-Z, a-z, 0-9, -, ., _, or ~ </summary>
    public static bool IsUnreserved(int b)
    {
        return (b >= 'A' && b <= 'Z')
            || (b >= 'a' && b <= 'z')
            || (b >= '0' && b <= '9')
            || b == '-' || b == '.' || b == '_' || b == '~';
    }

    /// <summary> Checks if s contains prefix starting at position index, using raw character-by-character comparison (CompareOrdinal) </summary>
    public static bool StartsWithAt(string s, int index, string prefix)
    {
        return index + prefix.Length <= s.Length
            && string.CompareOrdinal(s, index, prefix, 0, prefix.Length) == 0;
    }

    /// <summary> Walks backwards from the end of a StringBuilder to find the index of the last '/' (or -1 if there isn't one) </summary>
    public static int LastSlashIndex(StringBuilder sb)
    {
        for (int i = sb.Length - 1; i >= 0; i--)
        {
            if (sb[i] == '/')
            {
                return i;
            }
        }
        return -1;
    }

    /// <summary> Maps known schemes to their standard port strings: http/ws → "80", https/wss → "443", ftp → "21". Everything else returns "" </summary>
    public static string DefaultPort(string scheme)
    {
        switch (scheme)
        {
            case "http":
            case "ws":
                return "80";
            case "https":
            case "wss":
                return "443";
            case "ftp":
                return "21";
            default:
                return string.Empty;
        }
    }

    /// <summary> Finds the : that separates the host from the port </summary>
    public static int FindPortColon(string authority, int hostStart)
    {
        int len = authority.Length;
        if (hostStart < len && authority[hostStart] == '[') // IPv6 / IP-literal
        {
            int closeBracket = authority.IndexOf(']', hostStart);
            if (closeBracket < 0)
            {
                return -1;
            }
            int next = closeBracket + 1;
            return next < len && authority[next] == ':' ? next : -1;
        }
        return authority.IndexOf(':', hostStart);
    }
}
