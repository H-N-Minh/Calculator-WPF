// ------------------------------------------------------------------------------
// © SDK GmbH 
// ------------------------------------------------------------------------------

using System.Text.Json;
using System.Text.Json.Serialization;
using BdewUriNormalization;

public static class AiTestRunner
{
    public static int Start(string[] args)
    {
        string[] jsonPaths = args.Length > 0
            ? args
            : ["C:\\SDKDevSign\\Training\\WPF Programns\\UriNormalizer\\AI Generated Tests\\Tests.json",
                "C:\\SDKDevSign\\Training\\WPF Programns\\UriNormalizer\\AI Generated Tests\\gpt.json",
                "C:\\SDKDevSign\\Training\\WPF Programns\\UriNormalizer\\AI Generated Tests\\Gem.json",
                "C:\\SDKDevSign\\Training\\WPF Programns\\UriNormalizer\\AI Generated Tests\\claud.json"];

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        int grandTotal = 0;
        int grandPassed = 0;
        int grandFailed = 0;

        foreach (string path in jsonPaths)
        {
            if (!File.Exists(path))
            {
                Console.WriteLine($"[ERROR] File not found: {Path.GetFullPath(path)}");
                grandFailed++;
                continue;
            }

            Console.WriteLine("============================================================");
            Console.WriteLine($"Running corpus: {Path.GetFileName(path)}");

            UnifiedTestCorpus? corpus = JsonSerializer.Deserialize<UnifiedTestCorpus>(File.ReadAllText(path), jsonOptions);
            if (corpus is null)
            {
                Console.WriteLine($"[ERROR] Failed to deserialize {path}");
                grandFailed++;
                continue;
            }

            if (!string.IsNullOrWhiteSpace(corpus.Title))
                Console.WriteLine($"Title:   {corpus.Title}");

            int total = 0, passed = 0, failed = 0;
            var validNormalizedInputs = new List<(string Id, string Input, string Normalized)>();

            // ------------------------------------------------------------------
            // 1. Positive test sections
            // ------------------------------------------------------------------
            foreach (var section in corpus.Sections)
            {
                foreach (var test in section.Tests)
                {
                    total++;
                    try
                    {
                        string actual = UriNormalizer.Normalize(test.Input);
                        if (actual == test.Expected)
                        {
                            passed++;
                            validNormalizedInputs.Add((test.Id, test.Input, actual));
                        }
                        else if (!string.IsNullOrEmpty(test.Alternative) && actual == test.Alternative)
                        {
                            // Matches documented alternative policy; still flag or adjust if you adopt the alternative
                            failed++;
                            Console.WriteLine($"[FAIL-ALT] {test.Id} ({section.Section}) matched documented alternative");
                            Console.WriteLine($"  Input:       {test.Input}");
                            Console.WriteLine($"  Expected:    {test.Expected}");
                            Console.WriteLine($"  Alternative: {test.Alternative}");
                            Console.WriteLine($"  Actual:      {actual}");
                        }
                        else
                        {
                            failed++;
                            Console.WriteLine($"[FAIL] {test.Id} ({section.Section})");
                            Console.WriteLine($"  Input:    {test.Input}");
                            Console.WriteLine($"  Expected: {test.Expected}");
                            Console.WriteLine($"  Actual:   {actual}");
                            if (!string.IsNullOrEmpty(test.Requires))
                                Console.WriteLine($"  Requires: {test.Requires}");
                        }
                    }
                    catch (Exception ex)
                    {
                        failed++;
                        Console.WriteLine($"[FAIL] {test.Id} ({section.Section}) threw {ex.GetType().Name}: {ex.Message}");
                        Console.WriteLine($"  Input: {test.Input}");
                    }
                }
            }

            // ------------------------------------------------------------------
            // 2. Invalid inputs
            // ------------------------------------------------------------------
            foreach (var test in corpus.InvalidInputs)
            {
                string expectedOutcome = string.IsNullOrWhiteSpace(test.ExpectedOutcome) ? "reject" : test.ExpectedOutcome;
                total++;

                bool rejected = false;
                string? actualOutput = null;
                Exception? unexpectedEx = null;

                try
                {
                    actualOutput = UriNormalizer.Normalize(test.Input);
                }
                catch (Exception ex) when (ex is FormatException or ArgumentException)
                {
                    rejected = true;
                }
                catch (Exception ex)
                {
                    unexpectedEx = ex;
                }

                if (unexpectedEx is not null)
                {
                    failed++;
                    Console.WriteLine($"[FAIL] {test.Id} threw unexpected {unexpectedEx.GetType().Name}: {unexpectedEx.Message}");
                    Console.WriteLine($"  Input: {test.Input}");
                }
                else if (expectedOutcome == "reject")
                {
                    if (rejected)
                    {
                        passed++;
                    }
                    else
                    {
                        failed++;
                        Console.WriteLine($"[FAIL] {test.Id} (expected reject, but succeeded)");
                        Console.WriteLine($"  Input:  {test.Input}");
                        Console.WriteLine($"  Output: {actualOutput}");
                        if (!string.IsNullOrEmpty(test.Reason))
                            Console.WriteLine($"  Reason: {test.Reason}");
                    }
                }
                else if (expectedOutcome == "reject_or_unsupported")
                {
                    passed++;
                    if (!rejected)
                    {
                        Console.WriteLine($"[INFO] {test.Id} ({expectedOutcome}): accepted -> {actualOutput}");
                    }
                }
                else
                {
                    failed++;
                    Console.WriteLine($"[FAIL] {test.Id}: unknown expected_outcome '{expectedOutcome}'");
                }
            }

            // ------------------------------------------------------------------
            // 3. Equivalence groups
            // ------------------------------------------------------------------
            foreach (var group in corpus.EquivalenceGroups)
            {
                total++;
                if (group.Inputs.Count == 0)
                {
                    failed++;
                    Console.WriteLine($"[FAIL] {group.Id}: equivalence group has no inputs");
                    continue;
                }

                var results = new List<(string Input, string? Output, string? Error)>();
                foreach (string uri in group.Inputs)
                {
                    try
                    {
                        results.Add((uri, UriNormalizer.Normalize(uri), null));
                    }
                    catch (Exception ex)
                    {
                        results.Add((uri, null, $"{ex.GetType().Name}: {ex.Message}"));
                    }
                }

                bool anyError = results.Any(r => r.Error is not null);
                var distinctOutputs = results.Where(r => r.Output is not null).Select(r => r.Output!).Distinct().ToList();
                bool matchesExpected = string.IsNullOrEmpty(group.Expected) ||
                                       (distinctOutputs.Count == 1 && distinctOutputs[0] == group.Expected);

                if (!anyError && distinctOutputs.Count == 1 && matchesExpected)
                {
                    passed++;
                }
                else
                {
                    failed++;
                    Console.WriteLine($"[FAIL] {group.Id} (equivalence_group)");
                    if (!string.IsNullOrEmpty(group.Expected))
                        Console.WriteLine($"  Expected common: {group.Expected}");
                    foreach (var r in results)
                    {
                        Console.WriteLine($"  Input:  {r.Input}");
                        Console.WriteLine($"  Result: {r.Output ?? $"ERROR ({r.Error})"}");
                    }
                }
            }

            // ------------------------------------------------------------------
            // 4. Distinct / non-equivalent pairs
            // ------------------------------------------------------------------
            foreach (var pair in corpus.DistinctPairs)
            {
                total++;
                try
                {
                    string normA = UriNormalizer.Normalize(pair.A);
                    string normB = UriNormalizer.Normalize(pair.B);

                    if (normA != normB)
                    {
                        passed++;
                    }
                    else
                    {
                        failed++;
                        Console.WriteLine($"[FAIL] {pair.Id} (distinct_pairs: inputs collapsed to same URI)");
                        Console.WriteLine($"  A:      {pair.A}");
                        Console.WriteLine($"  B:      {pair.B}");
                        Console.WriteLine($"  Output: {normA}");
                        if (!string.IsNullOrEmpty(pair.Reason))
                            Console.WriteLine($"  Reason: {pair.Reason}");
                    }
                }
                catch (Exception ex)
                {
                    failed++;
                    Console.WriteLine($"[FAIL] {pair.Id} (distinct_pairs) threw {ex.GetType().Name}: {ex.Message}");
                }
            }

            // ------------------------------------------------------------------
            // 5. Policy-dependent tests (check against strict HTTP or any documented policy)
            // ------------------------------------------------------------------
            foreach (var pol in corpus.PolicyTests)
            {
                total++;
                string outcome;
                try
                {
                    outcome = UriNormalizer.Normalize(pol.Input);
                }
                catch (Exception ex) when (ex is FormatException or ArgumentException)
                {
                    outcome = "reject";
                }
                catch (Exception ex)
                {
                    failed++;
                    Console.WriteLine($"[FAIL] {pol.Id} (policy_test) threw unexpected {ex.GetType().Name}: {ex.Message}");
                    continue;
                }

                // Prefer strict_http_* expectation if available; otherwise accept any documented profile expectation
                var strictEntry = pol.ProfileExpectations
                    .FirstOrDefault(kv => kv.Key.Contains("strict_http", StringComparison.OrdinalIgnoreCase));

                bool matched = strictEntry.Key is not null
                    ? outcome == strictEntry.Value
                    : pol.ProfileExpectations.Values.Contains(outcome);

                if (matched)
                {
                    passed++;
                }
                else
                {
                    failed++;
                    Console.WriteLine($"[FAIL] {pol.Id} (policy_test): {pol.PolicyQuestion}");
                    Console.WriteLine($"  Input:  {pol.Input}");
                    Console.WriteLine($"  Actual: {outcome}");
                    foreach (var kv in pol.ProfileExpectations)
                        Console.WriteLine($"  [{kv.Key}]: {kv.Value}");
                }
            }

            // ------------------------------------------------------------------
            // 6. Automated invariant properties over all valid normalized URIs
            // ------------------------------------------------------------------
            if (validNormalizedInputs.Count > 0)
            {
                total += 5;
                passed += CheckProperty("PROP-IDEMPOTENCE", validNormalizedInputs,
                    item => UriNormalizer.Normalize(item.Normalized) == item.Normalized,
                    ref failed);

                passed += CheckProperty("PROP-DETERMINISM", validNormalizedInputs,
                    item => UriNormalizer.Normalize(item.Input) == item.Normalized,
                    ref failed);

                passed += CheckProperty("PROP-ASCII-ONLY", validNormalizedInputs,
                    item => item.Normalized.All(c => c is >= (char)0x20 and <= (char)0x7E),
                    ref failed);

                passed += CheckProperty("PROP-UPPERCASE-HEX", validNormalizedInputs,
                    item => HasOnlyUppercasePercentTriplets(item.Normalized),
                    ref failed);

                passed += CheckProperty("PROP-NO-DOT-SEGMENTS", validNormalizedInputs,
                    item => HasNoDotSegmentsInPath(item.Normalized),
                    ref failed);
            }

            Console.WriteLine($"File Summary ({Path.GetFileName(path)}): {passed}/{total} passed, {failed} failed.");
            grandTotal += total;
            grandPassed += passed;
            grandFailed += failed;
        }

        Console.WriteLine();
        Console.WriteLine("============================================================");
        Console.WriteLine($"GRAND SUMMARY: {grandPassed}/{grandTotal} passed, {grandFailed} failed.");
        return grandFailed == 0 ? 0 : 1;
    }


    // ----------------------------------------------------------------------
    // Helpers & Unified Schema Models
    // ----------------------------------------------------------------------

    static int CheckProperty(
        string propName,
        List<(string Id, string Input, string Normalized)> items,
        Func<(string Id, string Input, string Normalized), bool> predicate,
        ref int failedCount)
    {
        foreach (var item in items)
        {
            try
            {
                if (!predicate(item))
                {
                    failedCount++;
                    Console.WriteLine($"[FAIL] {propName} violated on {item.Id}");
                    Console.WriteLine($"  Input:      {item.Input}");
                    Console.WriteLine($"  Normalized: {item.Normalized}");
                    return 0;
                }
            }
            catch (Exception ex)
            {
                failedCount++;
                Console.WriteLine($"[FAIL] {propName} threw {ex.GetType().Name} on {item.Id}: {ex.Message}");
                return 0;
            }
        }
        return 1;
    }

    static bool HasOnlyUppercasePercentTriplets(string uri)
    {
        for (int i = 0; i < uri.Length; i++)
        {
            if (uri[i] != '%') continue;
            if (i + 2 >= uri.Length) return false;
            if (!IsUpperHex(uri[i + 1]) || !IsUpperHex(uri[i + 2])) return false;
            i += 2;
        }
        return true;
    }

    static bool IsUpperHex(char c) =>
        (c >= '0' && c <= '9') || (c >= 'A' && c <= 'F');

    static bool HasNoDotSegmentsInPath(string uri)
    {
        int schemeEnd = uri.IndexOf("://", StringComparison.Ordinal);
        int afterAuthority = schemeEnd >= 0 ? uri.IndexOf('/', schemeEnd + 3) : uri.IndexOf('/');
        if (afterAuthority < 0) return true;

        int pathEnd = uri.IndexOfAny(['?', '#'], afterAuthority);
        string path = pathEnd >= 0
            ? uri[afterAuthority..pathEnd]
            : uri[afterAuthority..];

        foreach (string seg in path.Split('/'))
        {
            if (seg is "." or "..") return false;
        }
        return true;
    }
}

public sealed class UnifiedTestCorpus
{
    [JsonPropertyName("title")] public string? Title { get; set; }
    [JsonPropertyName("profile")] public string? Profile { get; set; }
    [JsonPropertyName("sections")] public List<TestSection> Sections { get; set; } = [];
    [JsonPropertyName("invalid_inputs")] public List<InvalidInputTest> InvalidInputs { get; set; } = [];
    [JsonPropertyName("equivalence_groups")] public List<EquivalenceGroup> EquivalenceGroups { get; set; } = [];
    [JsonPropertyName("distinct_pairs")] public List<DistinctPair> DistinctPairs { get; set; } = [];
    [JsonPropertyName("policy_tests")] public List<PolicyTest> PolicyTests { get; set; } = [];
}

public sealed class TestSection
{
    [JsonPropertyName("section")] public string Section { get; set; } = "";
    [JsonPropertyName("description")] public string? Description { get; set; }
    [JsonPropertyName("tests")] public List<PositiveTestCase> Tests { get; set; } = [];
}

public sealed class PositiveTestCase
{
    [JsonPropertyName("id")] public string Id { get; set; } = "";
    [JsonPropertyName("input")] public string Input { get; set; } = "";
    [JsonPropertyName("expected")] public string Expected { get; set; } = "";
    [JsonPropertyName("purpose")] public string? Purpose { get; set; }
    [JsonPropertyName("rfc"), JsonConverter(typeof(FlexibleStringConverter))]
    public string? Rfc { get; set; }

    [JsonPropertyName("policy"), JsonConverter(typeof(FlexibleStringConverter))]
    public string? Policy { get; set; }
    [JsonPropertyName("alternative")] public string? Alternative { get; set; }
    [JsonPropertyName("requires"), JsonConverter(typeof(FlexibleStringConverter))]
    public string? Requires { get; set; }
}

public sealed class InvalidInputTest
{
    [JsonPropertyName("id")] public string Id { get; set; } = "";
    [JsonPropertyName("input")] public string Input { get; set; } = "";
    [JsonPropertyName("expected_outcome")] public string? ExpectedOutcome { get; set; }
    [JsonPropertyName("reason")] public string? Reason { get; set; }
}

public sealed class EquivalenceGroup
{
    [JsonPropertyName("id")] public string Id { get; set; } = "";
    [JsonPropertyName("expected")] public string? Expected { get; set; }
    [JsonPropertyName("inputs")] public List<string> Inputs { get; set; } = [];
    [JsonPropertyName("purpose")] public string? Purpose { get; set; }
}

public sealed class DistinctPair
{
    [JsonPropertyName("id")] public string Id { get; set; } = "";
    [JsonPropertyName("a")] public string A { get; set; } = "";
    [JsonPropertyName("b")] public string B { get; set; } = "";
    [JsonPropertyName("reason")] public string? Reason { get; set; }
}

public sealed class PolicyTest
{
    [JsonPropertyName("id")] public string Id { get; set; } = "";
    [JsonPropertyName("input")] public string Input { get; set; } = "";
    [JsonPropertyName("policy_question")] public string? PolicyQuestion { get; set; }
    [JsonPropertyName("purpose")] public string? Purpose { get; set; }
    [JsonPropertyName("profile_expectations")] public Dictionary<string, string?> ProfileExpectations { get; set; } = new();
}

public sealed class FlexibleStringConverter : JsonConverter<string?>
{
    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.String:
                return reader.GetString();
            case JsonTokenType.True:
                return "true";
            case JsonTokenType.False:
                return "false";
            case JsonTokenType.Null:
                return null;
            default:
                {
                    using var doc = JsonDocument.ParseValue(ref reader);
                    return doc.RootElement.GetRawText();
                }
        }
    }

    public override void Write(Utf8JsonWriter writer, string? value, JsonSerializerOptions options)
        => writer.WriteStringValue(value);
}
