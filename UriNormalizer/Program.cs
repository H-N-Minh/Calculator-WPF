// ------------------------------------------------------------------------------
// © SDK GmbH 
// ------------------------------------------------------------------------------

using System.Text.Json;
using BdewUriNormalization;

string jsonPath = args.Length > 0 ? args[0] : "Tests.json";

if (!File.Exists(jsonPath))
{
    Console.WriteLine($"File not found: {Path.GetFullPath(jsonPath)}");
    return 1;
}

using JsonDocument doc = JsonDocument.Parse(File.ReadAllText(jsonPath));
JsonElement root = doc.RootElement;

int total = 0;
int passed = 0;
int failed = 0;

// 1. Run all positive test sections (any root array except invalid_inputs / source_references)
foreach (JsonProperty prop in root.EnumerateObject())
{
    if (prop.Value.ValueKind != JsonValueKind.Array ||
        prop.Name is "invalid_inputs" or "source_references")
    {
        continue;
    }

    foreach (JsonElement sectionElement in prop.Value.EnumerateArray())
    {
        if (sectionElement.ValueKind != JsonValueKind.Object)
        {
            continue;
        }

        string sectionName = sectionElement.TryGetProperty("section", out var secProp)
            ? secProp.GetString() ?? "(unnamed)"
            : prop.Name;

        // Find the array of test cases inside this section object
        foreach (JsonProperty secField in sectionElement.EnumerateObject())
        {
            if (secField.Value.ValueKind != JsonValueKind.Array)
            {
                continue;
            }

            foreach (JsonElement test in secField.Value.EnumerateArray())
            {
                string id = test.GetProperty("id").GetString() ?? "";
                string input = test.GetProperty("input").GetString() ?? "";
                string expected = test.GetProperty("expected").GetString() ?? "";

                total++;
                try
                {
                    string actual = UriNormalizer.Normalize(input);
                    if (actual == expected)
                    {
                        passed++;
                    }
                    else
                    {
                        failed++;
                        Console.WriteLine($"[FAIL] {id} ({sectionName})");
                        Console.WriteLine($"  Input:    {input}");
                        Console.WriteLine($"  Expected: {expected}");
                        Console.WriteLine($"  Actual:   {actual}");
                    }
                }
                catch (Exception ex)
                {
                    failed++;
                    Console.WriteLine($"[FAIL] {id} ({sectionName}) threw {ex.GetType().Name}: {ex.Message}");
                    Console.WriteLine($"  Input: {input}");
                }
            }
        }
    }
}

// 2. Run invalid_inputs
if (root.TryGetProperty("invalid_inputs", out JsonElement invalidInputs) &&
    invalidInputs.ValueKind == JsonValueKind.Array)
{
    foreach (JsonElement test in invalidInputs.EnumerateArray())
    {
        string id = test.GetProperty("id").GetString() ?? "";
        string input = test.GetProperty("input").GetString() ?? "";
        string expectedOutcome = test.GetProperty("expected_outcome").GetString() ?? "reject";

        total++;
        bool rejected = false;
        string? actualOutput = null;

        try
        {
            actualOutput = UriNormalizer.Normalize(input);
        }
        catch (FormatException)
        {
            rejected = true;
        }
        catch (ArgumentException)
        {
            rejected = true;
        }

        if (expectedOutcome == "reject")
        {
            if (rejected)
            {
                passed++;
            }
            else
            {
                failed++;
                Console.WriteLine($"[FAIL] {id} (expected reject, but succeeded)");
                Console.WriteLine($"  Input:  {input}");
                Console.WriteLine($"  Output: {actualOutput}");
            }
        }
        else if (expectedOutcome == "reject_or_unsupported")
        {
            // Passes either way; log if it was accepted as a general RFC 3986 URI
            passed++;
            if (!rejected)
            {
                Console.WriteLine($"[INFO] {id} ({expectedOutcome}): accepted as general URI -> {actualOutput}");
            }
        }
    }
}

Console.WriteLine();
Console.WriteLine($"Summary: {passed}/{total} passed, {failed} failed.");
return failed == 0 ? 0 : 1;
