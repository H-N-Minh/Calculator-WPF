// ------------------------------------------------------------------------------
// © SDK GmbH 
// ------------------------------------------------------------------------------
namespace ClearPdfAttrib;

internal static class Program
{
    public static async Task<int> Main(string[] args)
    {
        if (!TryParseArgs(args, out ProcessingOptions? options, out string? parseError, out bool showHelp))
        {
            if (!string.IsNullOrEmpty(parseError))
            {
                Console.Error.WriteLine($"Error: {parseError}");
                Console.Error.WriteLine();
            }

            PrintUsage();
            return showHelp ? 0 : 1;
        }

        using var cts = new CancellationTokenSource();

        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
            Console.Error.WriteLine("Cancellation requested. Waiting for active workers to finish safely...");
        };

        try
        {
            BatchProcessingResult result = await PdfBatchProcessor.RunAsync(options!, cts.Token);
            return result.ExitCode;
        }
        catch (Exception ex) when (ex is DirectoryNotFoundException or UnauthorizedAccessException or IOException)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            return 1;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Fatal error: {ex}");
            return 1;
        }
    }

    private static bool TryParseArgs(
        string[] args,
        out ProcessingOptions? options,
        out string? error,
        out bool showHelp)
    {
        options = null;
        error = null;
        showHelp = false;

        if (args.Length == 0)
        {
            error = "Missing root directory argument.";
            return false;
        }

        string? rootDirectory = null;
        int parallel = 4;
        bool dryRun = false;
        bool skipSigned = false;
        bool ignoreInaccessibleDirs = false;
        int progressInterval = 10_000;
        int retries = 3;
        string errorLog = $"ClearPdfAttrib-errors-{DateTime.Now:yyyyMMdd-HHmmss}.log";

        for (int i = 0; i < args.Length; i++)
        {
            string arg = args[i];

            switch (arg)
            {
                case "-h":
                case "--help":
                case "/?":
                    showHelp = true;
                    return false;

                case "--dry-run":
                    dryRun = true;
                    break;

                case "--skip-signed":
                    skipSigned = true;
                    break;

                case "--ignore-inaccessible-dirs":
                    ignoreInaccessibleDirs = true;
                    break;

                case "--parallel":
                    if (!TryReadPositiveInt(args, ref i, "--parallel", 1, 64, out parallel, out error))
                    {
                        return false;
                    }
                    break;

                case "--progress-interval":
                    if (!TryReadPositiveInt(args, ref i, "--progress-interval", 1, 1_000_000, out progressInterval, out error))
                    {
                        return false;
                    }
                    break;

                case "--retries":
                    if (!TryReadPositiveInt(args, ref i, "--retries", 0, 10, out retries, out error))
                    {
                        return false;
                    }
                    break;

                case "--error-log":
                    if (i + 1 >= args.Length)
                    {
                        error = "Missing value for --error-log.";
                        return false;
                    }
                    errorLog = args[++i];
                    break;

                default:
                    if (arg.StartsWith("--", StringComparison.Ordinal))
                    {
                        error = $"Unknown option '{arg}'.";
                        return false;
                    }

                    if (rootDirectory is not null)
                    {
                        error = $"Unexpected extra argument '{arg}'.";
                        return false;
                    }

                    rootDirectory = arg;
                    break;
            }
        }

        if (string.IsNullOrWhiteSpace(rootDirectory))
        {
            error = "Missing root directory argument.";
            return false;
        }

        options = new ProcessingOptions(
            RootDirectory: rootDirectory,
            MaxParallelism: parallel,
            DryRun: dryRun,
            SkipSigned: skipSigned,
            IgnoreInaccessibleDirectories: ignoreInaccessibleDirs,
            ProgressInterval: progressInterval,
            MaxRetries: retries,
            ErrorLogPath: errorLog);

        return true;
    }

    private static bool TryReadPositiveInt(
        string[] args,
        ref int index,
        string optionName,
        int min,
        int max,
        out int value,
        out string? error)
    {
        value = 0;
        error = null;

        if (index + 1 >= args.Length)
        {
            error = $"Missing value for {optionName}.";
            return false;
        }

        if (!int.TryParse(args[++index], out value) || value < min || value > max)
        {
            error = $"Value for {optionName} must be an integer between {min} and {max}.";
            return false;
        }

        return true;
    }

    private static void PrintUsage()
    {
        Console.WriteLine("""
            Usage:
              ClearPdfAttrib <rootDirectory> [options]

            Options:
              --parallel <n>               Number of parallel worker tasks (default: 4, max: 64)
              --dry-run                    Scan and report without modifying any files
              --skip-signed                Do not modify digitally signed PDFs
              --error-log <path>           Path to error log file (default: ClearPdfAttrib-errors-<timestamp>.log)
              --progress-interval <n>      Print progress every N processed files (default: 10000)
              --retries <n>                Retries on transient SMB/IO errors (default: 3)
              --ignore-inaccessible-dirs   Skip folders with permission errors without logging directory failures
              -h, --help                   Show this help message

            Exit codes:
              0  Completed with zero file/directory failures
              1  Startup/argument error or cancelled by user
              2  Completed, but one or more files or directories failed
            """);
    }
}
