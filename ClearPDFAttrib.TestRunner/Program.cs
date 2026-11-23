// ------------------------------------------------------------------------------
// © SDK GmbH 
// ------------------------------------------------------------------------------

using System.Diagnostics;
using System.Text;
using ClearPDFAttrib.TestRunner;

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

if (!TestOptions.TryParse(args, out TestOptions? options))
{
    TestOptions.PrintUsage();
    return 1;
}

Console.WriteLine($"Test folder: {options.TargetDirectory}");
Console.WriteLine($"PDF count:   {options.Count:N0}");
Console.WriteLine($"Threads:     {options.Threads}");
Console.WriteLine();

var totalSw = Stopwatch.StartNew();

try
{
    // 1. Generate test PDFs + decoy files
    Console.WriteLine("[1/3] Generating test PDFs...");
    var stepSw = Stopwatch.StartNew();
    TestPdfGenerator.Generate(options.TargetDirectory, options.Count, options.Threads);
    Console.WriteLine($"      Done in {stepSw.Elapsed:hh\\:mm\\:ss\\.ff}\n");

    // 2. Run ClearPDFAttrib
    Console.WriteLine("[2/3] Running ClearPDFAttrib...");
    stepSw.Restart();
    int exitCode = ClearPdfAttribRunner.Run(options.TargetDirectory, options.Threads, options.ExecutablePath);
    Console.WriteLine($"      Exited with code {exitCode} in {stepSw.Elapsed:hh\\:mm\\:ss\\.ff}\n");

    if (exitCode != 0)
    {
        Console.Error.WriteLine($"FAIL: ClearPDFAttrib returned non-zero exit code ({exitCode}).");
        return 2;
    }

    // 3. Verify all PDFs and decoys
    Console.WriteLine("[3/3] Verifying all generated files...");
    stepSw.Restart();
    VerificationResult result = TestPdfVerifier.Verify(options.TargetDirectory, options.Count, options.Threads);
    Console.WriteLine($"      Verified {result.VerifiedPdfs:N0} PDFs in {stepSw.Elapsed:hh\\:mm\\:ss\\.ff}\n");

    if (result.FailureCount > 0)
    {
        Console.Error.WriteLine($"FAIL: {result.FailureCount:N0} verification failure(s) detected:");
        foreach (string sample in result.SampleFailures)
            Console.Error.WriteLine($"  - {sample}");
        return 3;
    }

    Console.WriteLine($"PASS: All {result.VerifiedPdfs:N0} PDFs verified clean in {totalSw.Elapsed:hh\\:mm\\:ss\\.ff}.");
    return 0;
}
finally
{
    if (!options.KeepFiles && options.CreatedTempDir && Directory.Exists(options.TargetDirectory))
    {
        Console.WriteLine("Cleaning up temporary test directory...");
        //Directory.Delete(options.TargetDirectory, recursive: true);
    }
}
