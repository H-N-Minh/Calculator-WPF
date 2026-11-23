// ------------------------------------------------------------------------------
// © SDK GmbH 
// ------------------------------------------------------------------------------
using System.Diagnostics.CodeAnalysis;

namespace ClearPDFAttrib;

internal sealed record CliOptions(
    string RootDirectory,
    int Threads,
    bool DryRun,
    string ErrorLogPath)
{
    public static bool TryParse(string[] args, [NotNullWhen(true)] out CliOptions? options)
    {
        options = null;
        string? rootDir = null;
        int threads = Math.Clamp(Environment.ProcessorCount, 4, 16);
        bool dryRun = false;
        string? logPath = null;

        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "-h" or "--help" or "/?":
                    return false;

                case "-n" or "--dry-run":
                    dryRun = true;
                    break;

                case "-t" or "--threads":
                    if (i + 1 >= args.Length || !int.TryParse(args[++i], out threads) || threads <= 0)
                        return false;
                    break;

                case var arg when arg.StartsWith('-') || rootDir is not null:
                    return false;

                case var arg:
                    rootDir = arg;
                    break;
            }
        }

        if (string.IsNullOrWhiteSpace(rootDir))
            return false;

        logPath ??= Path.Combine(
            Environment.CurrentDirectory,
            $"ClearPDFAttrib_errors_{DateTime.Now:yyyyMMdd_HHmmss}.log");

        options = new CliOptions(rootDir, threads, dryRun, logPath);
        return true;
    }

    public static void PrintUsage()
    {
        Console.WriteLine("""
            Usage:
              ClearPDFAttrib <Directory> [Options]

            Options:
              -n, --dry-run         Scan and report only, do not modify files
              -t, --threads <N>     Worker thread count (default: 4..16 based on CPU)
              -l, --log <Path>      Error log file path

            Examples:
              ClearPDFAttrib C:\temp --dry-run
              ClearPDFAttrib \\ME-SDK-DB-01\SDK.DocumentStore\Sherpa\Strom --threads 12
            """);
    }
}
