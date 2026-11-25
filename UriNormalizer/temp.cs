// ------------------------------------------------------------------------------
// © SDK GmbH 
// ------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;

namespace UriNormalizer;

using System.Text.Json;
using System.Text.Json.Nodes;

public static class TestCorpusMigrator
{
    public static void Main(string[] args)
    {
        TestCorpusMigrator.MigrateFile("Tests.json", "unified/Tests.json");
        TestCorpusMigrator.MigrateFile("Gpt_tests.json", "unified/gpt.json");
        TestCorpusMigrator.MigrateFile("Gem_tests.json", "unified/Gem.json");
        TestCorpusMigrator.MigrateFile("Clau_tests.json", "unified/claud.json");
    }
    private static readonly JsonSerializerOptions WriteOpts = new() { WriteIndented = true };

    public static void MigrateFile(string inputPath, string outputPath)
    {
        var root = JsonNode.Parse(File.ReadAllText(inputPath))?.AsObject()
            ?? throw new InvalidDataException($"Invalid JSON object in {inputPath}");

        var unified = new JsonObject();

        // 1. Copy common metadata if present
        CopyIfExists(root, unified, "title");
        CopyIfExists(root, unified, "profile");
        CopyIfExists(root, unified, "assumptions");
        CopyIfExists(root, unified, "recommended_order");
        CopyIfExists(root, unified, "source_references");
        CopyIfExists(root, unified, "scope");

        // 2. Unify sections (handles Tests.json arbitrary root arrays + standard "sections")
        var sectionsOut = new JsonArray();
        if (root.TryGetPropertyValue("sections", out var sectionsNode) && sectionsNode is JsonArray secArr)
        {
            foreach (var sec in secArr)
            {
                if (sec is JsonObject secObj)
                    sectionsOut.Add(NormalizeSectionObject(secObj, "unnamed"));
            }
        }
        else
        {
            // Tests.json style: any root array except metadata/known categories
            var reserved = new HashSet<string>
            {
                "assumptions", "recommended_order", "source_references", "invalid_inputs",
                "equivalence_groups", "distinct_pairs", "non_equivalent_pairs",
                "non_equivalence_pairs", "profile_dependent_tests", "policy_tests",
                "properties", "normalization_properties"
            };

            foreach (var kvp in root)
            {
                if (reserved.Contains(kvp.Key) || kvp.Value is not JsonArray arr) continue;
                foreach (var item in arr)
                {
                    if (item is JsonObject secObj)
                        sectionsOut.Add(NormalizeSectionObject(secObj, kvp.Key));
                }
            }
        }
        unified["sections"] = sectionsOut;

        // 3. invalid_inputs
        unified["invalid_inputs"] = root.TryGetPropertyValue("invalid_inputs", out var inv) && inv is JsonArray
            ? inv.DeepClone()
            : new JsonArray();

        // 4. equivalence_groups (handles members / equivalent_inputs / variants & expected / expected_common)
        var eqOut = new JsonArray();
        if (root.TryGetPropertyValue("equivalence_groups", out var eqNode) && eqNode is JsonArray eqArr)
        {
            foreach (var item in eqArr)
            {
                if (item is not JsonObject obj) continue;
                var newEq = new JsonObject
                {
                    ["id"] = GetString(obj, "id"),
                    ["expected"] = GetString(obj, "expected", "expected_common"),
                    ["inputs"] = CloneArrayFirstMatch(obj, "inputs", "equivalent_inputs", "members", "variants", "uris"),
                    ["purpose"] = GetString(obj, "purpose", "description")
                };
                eqOut.Add(newEq);
            }
        }
        unified["equivalence_groups"] = eqOut;

        // 5. distinct_pairs (handles distinct_pairs / non_equivalent_pairs / non_equivalence_pairs)
        var distOut = new JsonArray();
        var distSource = GetFirstArray(root, "distinct_pairs", "non_equivalent_pairs", "non_equivalence_pairs");
        if (distSource is not null)
        {
            foreach (var item in distSource)
            {
                if (item is not JsonObject obj) continue;
                string? a = GetString(obj, "a", "uri_a", "left");
                string? b = GetString(obj, "b", "uri_b", "right");

                // Fallback if stored as a 2-element array e.g. "uris": ["...", "..."]
                if ((a is null || b is null) &&
                    GetFirstArray(obj, "uris", "pair", "inputs") is { Count: >= 2 } pairArr)
                {
                    a ??= pairArr[0]?.GetValue<string>();
                    b ??= pairArr[1]?.GetValue<string>();
                }

                distOut.Add(new JsonObject
                {
                    ["id"] = GetString(obj, "id"),
                    ["a"] = a,
                    ["b"] = b,
                    ["reason"] = GetString(obj, "reason", "purpose", "description")
                });
            }
        }
        unified["distinct_pairs"] = distOut;

        // 6. policy_tests (handles Gem.json profile_dependent_tests)
        var policyOut = new JsonArray();
        var policySource = GetFirstArray(root, "policy_tests", "profile_dependent_tests");
        if (policySource is not null)
        {
            foreach (var item in policySource)
            {
                if (item is not JsonObject obj) continue;
                var expectations = new JsonObject();

                if (obj.TryGetPropertyValue("profile_expectations", out var existingExp) && existingExp is JsonObject expObj)
                {
                    expectations = (JsonObject)expObj.DeepClone();
                }
                else
                {
                    foreach (var kvp in obj)
                    {
                        if (kvp.Key.EndsWith("_expected", StringComparison.OrdinalIgnoreCase))
                            expectations[kvp.Key] = kvp.Value?.DeepClone();
                    }
                }

                policyOut.Add(new JsonObject
                {
                    ["id"] = GetString(obj, "id"),
                    ["input"] = GetString(obj, "input"),
                    ["policy_question"] = GetString(obj, "policy_question"),
                    ["purpose"] = GetString(obj, "purpose"),
                    ["profile_expectations"] = expectations
                });
            }
        }
        unified["policy_tests"] = policyOut;

        // 7. properties (handles properties / normalization_properties)
        var propSource = GetFirstArray(root, "properties", "normalization_properties");
        unified["properties"] = propSource is not null ? propSource.DeepClone() : new JsonArray();

        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(outputPath))!);
        File.WriteAllText(outputPath, unified.ToJsonString(WriteOpts));
    }

    private static JsonObject NormalizeSectionObject(JsonObject secObj, string fallbackName)
    {
        JsonArray? tests = null;
        if (secObj.TryGetPropertyValue("tests", out var t) && t is JsonArray tArr)
        {
            tests = (JsonArray)tArr.DeepClone();
        }
        else
        {
            // Find the first array property inside the section object (Tests.json pattern)
            foreach (var kvp in secObj)
            {
                if (kvp.Value is JsonArray arr)
                {
                    tests = (JsonArray)arr.DeepClone();
                    break;
                }
            }
        }

        return new JsonObject
        {
            ["section"] = GetString(secObj, "section") ?? fallbackName,
            ["description"] = GetString(secObj, "description"),
            ["tests"] = tests ?? new JsonArray()
        };
    }

    private static void CopyIfExists(JsonObject src, JsonObject dst, string key)
    {
        if (src.TryGetPropertyValue(key, out var val) && val is not null)
            dst[key] = val.DeepClone();
    }

    private static string? GetString(JsonObject obj, params string[] keys)
    {
        foreach (var key in keys)
        {
            if (obj.TryGetPropertyValue(key, out var val) && val is JsonValue jv && jv.TryGetValue<string>(out var s))
                return s;
        }
        return null;
    }

    private static JsonArray? GetFirstArray(JsonObject obj, params string[] keys)
    {
        foreach (var key in keys)
        {
            if (obj.TryGetPropertyValue(key, out var val) && val is JsonArray arr)
                return arr;
        }
        return null;
    }

    private static JsonArray CloneArrayFirstMatch(JsonObject obj, params string[] keys)
        => GetFirstArray(obj, keys)?.DeepClone().AsArray() ?? new JsonArray();
}
