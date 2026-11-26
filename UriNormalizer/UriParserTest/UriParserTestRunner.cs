// ------------------------------------------------------------------------------
// © SDK GmbH 
// ------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using static System.Net.WebRequestMethods;

namespace UriParserTest;

record UriTestCase(
    string SourceTest,
    string Mask,
    bool IsPartialMaskTest,
    bool IsRelativeUri,
    string Input,
    string ExpectedUriparser622,
    string ExpectedRfc3986_622_623,
    string? Note
);

public class TestStats
{
    public int Passed = 0;
    public int Failed = 0;
}

public static class UriParserTestRunner
{
    public static int Start(string[] args, out int passed, out int total)
    {
        string jsonPath = "C:\\SDKDevSign\\Training\\WPF Programns\\UriNormalizer\\UriParserTest\\uriparser_tests.json";

        var stats = new TestStats();

        RunUriParserTest(jsonPath, stats);

        // 2. Append critical RFC 3986 6.2.2 + 6.2.3 + BDEW tests missing from uriparser
        RunExtraTests(stats);

        passed = stats.Passed;
        total = stats.Passed + stats.Failed;

        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine($"\nSummary: {passed}/{total} tests passed");
        Console.ResetColor();

        return stats.Failed > 0 ? 1 : 0;


        static void RunUriParserTest(string jsonPath, TestStats stats)
        {
            List<UriTestCase> tests = JsonSerializer.Deserialize<List<UriTestCase>>(System.IO.File.ReadAllText(jsonPath))
                        ?? throw new InvalidOperationException("Failed to deserialize test cases.");

            Console.WriteLine("=== Running uriparser extracted tests ===");
            foreach (var tc in tests)
                RunTest(tc.SourceTest, tc.Input, tc.ExpectedRfc3986_622_623, tc.Note, stats);
        }

        static void RunExtraTests(TestStats stats)
        {
            Console.WriteLine("\n=== Running extra test for 6.2.3 & BDEW gap ===");
            var extraTests = new (string Name, string Input, string Expected)[]
            {
            ("6.2.3 HTTP default port",        "http://example.com:80/a",                "http://example.com/a"),
            ("6.2.3 HTTPS default port",       "https://example.com:443/a",              "https://example.com/a"),
            ("6.2.3 HTTPS non-default port",   "https://example.com:8443/a",             "https://example.com:8443/a"),
            ("6.2.3 Empty path + query",       "https://example.com?a=1",                "https://example.com/?a=1"),
            ("6.2.3 Empty query delimiter",    "https://example.com/?",                  "https://example.com/?"),
            ("6.2.3 Empty fragment delimiter", "https://example.com/#",                  "https://example.com/#"),
            ("6.2.2.2 All unreserved",         "https://example.com/%41%7A%30%39%2d%2e%5f%7e", "https://example.com/Az09-._~"),
            ("6.2.2.2 Reserved stays encoded", "https://example.com/%2f%3a%3f%23%25%2b%20%40%5b%60%7b", "https://example.com/%2F%3A%3F%23%25%2B%20%40%5B%60%7B"),
            ("6.2.2.3 Dot inside query",       "https://example.com/a?p=/../b/./c",      "https://example.com/a?p=/../b/./c"),
            ("BDEW Timestamp & query order",   "HTTPS://API.Netz.DE:443/v1/../v2/measures?to=2025-10-01T12%3a00%3a00Z&from=2025-10-01T00:00:00%2b02:00",
                                               "https://api.netz.de/v2/measures?to=2025-10-01T12%3A00%3A00Z&from=2025-10-01T00:00:00%2B02:00"),
            };

            foreach (var (name, input, expected) in extraTests)
            {
                RunTest(name, input, expected, null, stats);
            }
        }
    }

    static void RunTest(string name, string input, string expected, string? note, TestStats stats)
    {
        string actual;
        string secondPass;
        try
        {
            actual = UriNormalizer.Normalize(input);
            secondPass = UriNormalizer.Normalize(actual);
        }
        catch (Exception ex)
        {
            stats.Failed++;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[FAIL] {name}: threw {ex.GetType().Name}: {ex.Message}");
            Console.ResetColor();
            Console.WriteLine($"       Input:    {input}");
            return;
        }

        if (actual == expected && secondPass == actual)
        {
            stats.Passed++;
        }
        else
        {
            stats.Failed++;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[FAIL] {name}");
            Console.ResetColor();
            Console.WriteLine($"       Input:    {input}");
            Console.WriteLine($"       Expected: {expected}");
            Console.WriteLine($"       Actual:   {actual}");
            if (secondPass != actual)
                Console.WriteLine($"       2nd Pass: {secondPass} (NOT IDEMPOTENT!)");
            if (!string.IsNullOrEmpty(note))
                Console.WriteLine($"       Note:     {note}");
        }
    }
}

