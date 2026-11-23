// ------------------------------------------------------------------------------
// © SDK GmbH 
// ------------------------------------------------------------------------------

using System.Diagnostics.CodeAnalysis;

namespace ClearPDFAttrib.TestRunner;

internal sealed record TestOptions(
    string TargetDirectory,
    long Count,
    int Threads,
    string? ExecutablePath,
    bool KeepFiles,
    bool CreatedTempDir)
{
    public static bool TryParse(string[] args, [NotNullWhen(true)] out TestOptions? options)
    {
        options = null;
        string? dir = null;
        long count = 200;
        int threads = Math.Clamp(Environment.ProcessorCount, 4, 16);
        string? exePath = null;
        bool keep = false;

        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "-h" or "--help" or "/?":
                    return false;

                case "-c" or "--count":
                    if (i + 1 >= args.Length || !long.TryParse(args[++i], out count) || count <= 0)
                        return false;
                    break;

                case "-d" or "--dir":
                    if (i + 1 >= args.Length)
                        return false;
                    dir = args[++i];
                    break;

                case "-t" or "--threads":
                    if (i + 1 >= args.Length || !int.TryParse(args[++i], out threads) || threads <= 0)
                        return false;
                    break;

                case "-e" or "--exe":
                    if (i + 1 >= args.Length)
                        return false;
                    exePath = args[++i];
                    break;

                case "-k" or "--keep":
                    keep = true;
                    break;

                default:
                    return false;
            }
        }

        bool createdTemp = dir is null;
        dir ??= Path.Combine(Path.GetTempPath(), $"ClearPDFAttribTest_{DateTime.Now:yyyyMMdd_HHmmss}");

        options = new TestOptions(dir, count, threads, exePath, keep, createdTemp);
        return true;
    }

    public static void PrintUsage()
    {
        Console.WriteLine("""
            Usage:
              ClearPDFAttrib.TestRunner [Options]

            Options:
              -c, --count <N>       Number of PDFs to generate & test (default: 200)
              -d, --dir <Path>      Target test directory (default: auto temp folder)
              -t, --threads <N>     Parallel worker count (default: 4..16)
              -e, --exe <Path>      Path to ClearPDFAttrib.exe or .dll (auto-detected if omitted)
              -k, --keep            Keep generated test files after completion

            Examples:
              dotnet run -c Release
              dotnet run -c Release -- --count 2000000 --dir D:\PdfStressTest --threads 12
            """);
    }
}
