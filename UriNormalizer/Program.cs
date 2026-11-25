// ------------------------------------------------------------------------------
// © SDK GmbH 
// ------------------------------------------------------------------------------

using BdewUriNormalization;

// Example: normalizing a URI before passing it to your hashing step
string rawUri = "HTTPS://API.Example.COM:443/v1/./meters/../contracts/%7b123%7d?from=2026-01-01T00%3a00%3a00Z&filter=a+b%20c%2fd&tilde=%7e";
string normalizedUri = UriNormalizer.Normalize(rawUri);

Console.WriteLine($"Raw:        {rawUri}");
Console.WriteLine($"Normalized: {normalizedUri}");
Console.WriteLine();

// Next step (handled by your existing hashing code):
// byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(normalizedUri));

// Verification test vectors covering RFC 3986 Sections 6.2.2, 6.2.3, and 5.2.4
var testCases = new (string Input, string Expected)[]
{
    // RFC 3986 Section 6.2.2 example
    ("eXAMPLE://a/./b/../b/%63/%7bfoo%7d", "example://a/b/c/%7Bfoo%7D"),

    // RFC 3986 Section 6.2.3 examples
    ("http://example.com", "http://example.com/"),
    ("http://example.com/", "http://example.com/"),
    ("http://example.com:/", "http://example.com/"),
    ("http://example.com:80/", "http://example.com/"),
    ("HTTP://www.EXAMPLE.com/", "http://www.example.com/"),
    ("https://example.com:443", "https://example.com/"),
    ("https://example.com:8443/api", "https://example.com:8443/api"),
    ("http://example.com:443/", "http://example.com:443/"),

    // Empty query and fragment delimiters preserved (6.2.3)
    ("http://example.com/?", "http://example.com/?"),
    ("http://example.com#", "http://example.com/#"),

    // Percent-encoding & host case normalization
    ("http://EX%41MPLE.COM/%7euser/%2f/%42", "http://example.com/~user/%2F/B"),
    ("http://example.com/a/%2e/b/%2e%2e/c", "http://example.com/a/c"),

    // Dot-segment removal edge cases (5.2.4)
    ("http://example.com/a/b/c/./../../g", "http://example.com/a/g"),
    ("http://example.com/a/b/..", "http://example.com/a/"),
    ("http://example.com/.", "http://example.com/"),
    ("http://example.com/..", "http://example.com/"),
    ("http://example.com/a/%2F../b", "http://example.com/a/%2F../b"),

    // Userinfo case and delimiters preserved, IPv6 host lowercased
    ("http://User:Pass@[2001:DB8::1]:80/test", "http://User:Pass@[2001:db8::1]/test"),
    ("http://@example.com/", "http://@example.com/"),

    // Query parameter order, duplicates, '+' and trailing slash preserved
    ("https://api.test.de/v1/items/?b=2&a=1&b=3&q=a+b%20c", "https://api.test.de/v1/items/?b=2&a=1&b=3&q=a+b%20c"),
};

int passed = 0;
foreach (var (input, expected) in testCases)
{
    string actual = UriNormalizer.Normalize(input);
    bool ok = actual == expected;
    if (ok)
    {
        passed++;
    }
    else
    {
        Console.WriteLine($"FAIL: {input}");
        Console.WriteLine($"  Expected: {expected}");
        Console.WriteLine($"  Actual:   {actual}");
    }
}

Console.WriteLine($"Passed {passed}/{testCases.Length} test vectors.");
