// ------------------------------------------------------------------------------
// © SDK GmbH 
// ------------------------------------------------------------------------------

namespace ClearPdfAttrib;

public sealed record BatchProcessingResult(
    long Processed,
    long Modified,
    long SkippedClean,
    long SkippedSigned,
    long Failed,
    long DirectoryErrors,
    long Retries,
    bool Cancelled,
    TimeSpan Elapsed,
    string? ErrorLogPath)
{
    public int ExitCode => Cancelled ? 1 : (Failed > 0 || DirectoryErrors > 0 ? 2 : 0);
}

public static class PdfBatchProcessor
{
    /// <summary>
    /// Simple entry point for automated tests and callers using default options.
    /// </summary>
    public static Task<BatchProcessingResult> RunAsync(
        string rootDirectory,
        int maxDegreeOfParallelism = 4,
        CancellationToken cancellationToken = default)
    {
        var options = new ProcessingOptions(
            RootDirectory: rootDirectory,
            MaxParallelism: maxDegreeOfParallelism,
            ErrorLogPath: null,
            Quiet: true);

        return RunAsync(options, cancellationToken);
    }

    /// <summary>
    /// Full entry point used by the CLI and configurable integration tests.
    /// </summary>
    public static async Task<BatchProcessingResult> RunAsync(
        ProcessingOptions options,
        CancellationToken cancellationToken = default)
    {
        OptionsErrorHandling(options);

        PrintProgramStartInfo(options);

        using var stats = new ProcessingStatistics(
            options.ErrorLogPath,
            options.Quiet ? 0 : options.ProgressInterval,
            options.DryRun);

        var processor = new FileProcessor(options, stats);
        bool cancelled = false;

        try
        {
            await processor.RunAsync(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            cancelled = true;
        }

        if (!options.Quiet)
        {
            stats.PrintSummary(cancelled);
        }

        return new BatchProcessingResult(
            Processed: stats.Processed,
            Modified: stats.Modified,
            SkippedClean: stats.SkippedClean,
            SkippedSigned: stats.SkippedSigned,
            Failed: stats.Failed,
            DirectoryErrors: stats.DirectoryErrors,
            Retries: stats.Retries,
            Cancelled: cancelled,
            Elapsed: stats.Elapsed,
            ErrorLogPath: stats.ErrorLogPath);
    }

    private static void PrintProgramStartInfo(ProcessingOptions options)
    {
        if (!options.Quiet)
        {
            Console.WriteLine("--------- PROGRAM START ---------");
            Console.WriteLine($"Root directory     : {options.RootDirectory}");
            Console.WriteLine($"Mode               : {(options.DryRun ? "DRY-RUN (no files will be modified)" : "LIVE (in-place atomic replace)")}");
            Console.WriteLine($"Parallel workers   : {options.MaxParallelism}");
            Console.WriteLine($"Skip signed PDFs   : {options.SkipSigned}");
            Console.WriteLine($"Progress interval  : Progress report at every {options.ProgressInterval:N0} files");
            Console.WriteLine($"Max retries  : {options.MaxRetries}");
            if (!string.IsNullOrEmpty(options.ErrorLogPath))
            {
                Console.WriteLine($"Error log          : {Path.GetFullPath(options.ErrorLogPath)}");
            }
            Console.WriteLine();
        }
    }

    /// <summary>
    /// Quick error handling if the Options has invalid arg or the root directory doesnt exist or we dont have access rights to it.
    /// </summary>
    private static void OptionsErrorHandling(ProcessingOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.RootDirectory);
        ArgumentOutOfRangeException.ThrowIfLessThan(options.MaxParallelism, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(options.MaxParallelism, 64);

        if (!Directory.Exists(options.RootDirectory))
        {
            throw new DirectoryNotFoundException(
                $"Directory does not exist or is unreachable: '{options.RootDirectory}'");
        }

        // Fail fast if the root directory itself is inaccessible.
        using (IEnumerator<string> probe = Directory.EnumerateFileSystemEntries(options.RootDirectory).GetEnumerator())
        {
            probe.MoveNext();
        }
    }

}
