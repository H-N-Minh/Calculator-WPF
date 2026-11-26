// ------------------------------------------------------------------------------
// © SDK GmbH 
// ------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using static System.Net.WebRequestMethods;

namespace Official_Examples_Tests;

internal class OfficialTestRunner
{
    private static Dictionary<String, String> _dataSet = new()
    {
        ["example://a/b/c/%7Bfoo%7D"] = "example://a/b/c/%7Bfoo%7D",
        ["http://example.com"] = "http://example.com/",
        ["HTTP://www.EXAMPLE.com/"] = "http://www.example.com/",
        ["http://example.com/%3a"] = "http://example.com/%3A",
        ["mailto:Joe@Example.COM"] = "mailto:Joe@example.com",
        ["http://example.com/data"] = "http://example.com/data",
        ["http://example.com/?"] = "http://example.com/?",
        ["http://example.com#"] = "http://example.com/#",
        ["http://example.com:80/"] = "http://example.com/",
        ["http://example.com:/"] = "http://example.com/",
        ["https://API.Example.com:443?a=1&b=%7e%2f"] = "https://api.example.com/?a=1&b=~%2F"
    };

    private static List<List<String>> _equivalents = [
        ["example://a/b/c/%7Bfoo%7D", "eXAMPLE://a/./b/../b/%63/%7bfoo%7d"],
        ["http://example.com", "http://example.com/", "http://example.com:/", "http://example.com:80/"],
        ["HTTP://www.EXAMPLE.com/", "http://www.example.com/"],
        ["http://example.com/%3a", "http://example.com/%3A"],
        ["mailto:Joe@Example.COM", "mailto:Joe@example.com"]
    ];

    private static List<List<String>> _nonEquivalents = [
        ["http://example.com/?", "http://example.com"],
        ["http://example.com", "http://example.com#"],
        ["http://example.com/data", "http://example.com/data/"]
        ];

    public static int Start(string[] args)
    {
        var uriNormalizer = new UriNormalizer();
        int total = 0;
        int passed = 0;

        foreach (var (input, expected) in _dataSet)
        {
            string actual = uriNormalizer.Normalize(input);
            Check(
                actual == expected,
                $"Normalize(\"{input}\") -> got \"{actual}\", expected \"{expected}\""
            );
        }

        for (int g = 0; g < _equivalents.Count; g++)
        {
            var group = _equivalents[g];
            string normA = uriNormalizer.Normalize(group[0]);
            for (int i = 1; i < group.Count; i++)
            {
                string normB = uriNormalizer.Normalize(group[i]);
                Check(
                    normA == normB,
                    $"Equivalents[{g}] ({group[0]} vs {group[i]}) -> \"{normA}\" != \"{normB}\""
                );
            }
        }

        for (int g = 0; g < _nonEquivalents.Count; g++)
        {
            var group = _nonEquivalents[g];
            for (int i = 0; i < group.Count; i++)
            {
                string normA = uriNormalizer.Normalize(group[i]);
                for (int j = i + 1; j < group.Count; j++)
                {
                    string normB = uriNormalizer.Normalize(group[j]);
                    Check(
                        normA != normB,
                        $"NonEquivalents[{group}] ({group[i]} vs {group[j]}) -> unexpectedly equal: \"{normA}\""
                    );
                }
            }
        }

        Console.WriteLine($"Completed: {passed}/{total} passed.");

        return passed == total ? 0 : 1;


        void Check(bool condition, string message)
        {
            total++;
            if (condition)
            {
                passed++;
            }
            else
            {
                Console.WriteLine($"FAIL: {message}");
            }
        }
    }
}
