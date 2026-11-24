// ------------------------------------------------------------------------------
// © SDK GmbH 
// ------------------------------------------------------------------------------

using System.Diagnostics;
using System.Text;

namespace ClearPdfAttrib;

public enum PdfCleanOutcome
{
    Modified,
    SkippedClean,
    SkippedSigned
}

public sealed class ProcessingStatistics : IDisposable
{
    private readonly Lock _logLock = new();
    private readonly Stopwatch _stopwatch = Stopwatch.StartNew();
    private readonly int _progressInterval;
    private readonly bool _dryRun;
    private readonly StreamWriter? _errorWriter;

    private long _processed;
    private long _modified;
    private long _skippedClean;
    private long _skippedSigned;
    private long _failed;
    private long _directoryErrors;
    private long _retries;

    public TimeSpan Elapsed => _stopwatch.Elapsed;
    public string? ErrorLogPath { get; }

    public long Processed => Interlocked.Read(ref _processed);
    public long Modified => Interlocked.Read(ref _modified);
    public long SkippedClean => Interlocked.Read(ref _skippedClean);
    public long SkippedSigned => Interlocked.Read(ref _skippedSigned);
    public long Failed => Interlocked.Read(ref _failed);
    public long DirectoryErrors => Interlocked.Read(ref _directoryErrors);
    public long Retries => Interlocked.Read(ref _retries);

    public ProcessingStatistics(string? errorLogPath, int progressInterval, bool dryRun)
    {
        _progressInterval = progressInterval;
        _dryRun = dryRun;
        ErrorLogPath = errorLogPath;

        if (!string.IsNullOrWhiteSpace(errorLogPath))
        {
            string? logDir = Path.GetDirectoryName(Path.GetFullPath(errorLogPath));
            if (!string.IsNullOrEmpty(logDir))
            {
                Directory.CreateDirectory(logDir);
            }

            _errorWriter = new StreamWriter(
                errorLogPath,
                append: false,
                encoding: new UTF8Encoding(encoderShouldEmitUTF8Identifier: false))
            {
                AutoFlush = true
            };

            _errorWriter.WriteLine($"# ClearPdfAttrib Error Log - Started {DateTimeOffset.UtcNow:O}");
            _errorWriter.WriteLine("# TimestampUTC\tKind\tPath\tError");
        }
    }

    public void RecordSuccess(PdfCleanOutcome outcome)
    {
        switch (outcome)
        {
            case PdfCleanOutcome.Modified:
                Interlocked.Increment(ref _modified);
                break;
            case PdfCleanOutcome.SkippedClean:
                Interlocked.Increment(ref _skippedClean);
                break;
            case PdfCleanOutcome.SkippedSigned:
                Interlocked.Increment(ref _skippedSigned);
                break;
        }

        OnItemProcessed();
    }

    public void RecordFileFailure(string filePath, Exception ex)
    {
        Interlocked.Increment(ref _failed);
        WriteErrorLine("FILE", filePath, FormatException(ex));
        OnItemProcessed();
    }

    public void RecordDirectoryError(string directoryPath, Exception ex)
    {
        Interlocked.Increment(ref _directoryErrors);
        string details = FormatException(ex);
        WriteErrorLine("DIR", directoryPath, details);

        lock (_logLock)
        {
            Console.Error.WriteLine($"[DIR ERROR] {directoryPath} -> {details}");
        }
    }

    public void RecordRetry()
    {
        Interlocked.Increment(ref _retries);
    }

    private void OnItemProcessed()
    {
        long count = Interlocked.Increment(ref _processed);
        if (_progressInterval > 0 && count % _progressInterval == 0)
        {
            PrintProgress(count);
        }
    }

    private void PrintProgress(long processed)
    {
        TimeSpan elapsed = _stopwatch.Elapsed;
        double rate = elapsed.TotalSeconds > 0 ? processed / elapsed.TotalSeconds : 0;
        string modLabel = _dryRun ? "would-modify" : "modified";

        lock (_logLock)
        {
            Console.WriteLine(
                $"[{elapsed:hh\\:mm\\:ss}] Processed: {processed:N0} ({rate:N1} files/s) | " +
                $"{modLabel}: {Modified:N0} | skipped-clean: {SkippedClean:N0} | " +
                $"skipped-signed: {SkippedSigned:N0} | failed: {Failed:N0}");
        }
    }

    public void PrintSummary(bool cancelled)
    {
        _stopwatch.Stop();
        TimeSpan elapsed = _stopwatch.Elapsed;
        long total = Processed;
        double rate = elapsed.TotalSeconds > 0 ? total / elapsed.TotalSeconds : 0;
        string modLabel = _dryRun ? "Would modify" : "Modified";

        lock (_logLock)
        {
            Console.WriteLine();
            Console.WriteLine(cancelled ? "=== Summary (CANCELLED) ===" : "=== Summary ===");
            Console.WriteLine($"Elapsed time       : {elapsed:hh\\:mm\\:ss\\.fff}");
            Console.WriteLine($"Throughput         : {rate:N1} files/s");
            Console.WriteLine($"Processed PDFs     : {total:N0}");
            Console.WriteLine($"{modLabel,-18} : {Modified:N0}");
            Console.WriteLine($"Skipped (clean)    : {SkippedClean:N0}");
            Console.WriteLine($"Skipped (signed)   : {SkippedSigned:N0}");
            Console.WriteLine($"Failed files       : {Failed:N0}");
            Console.WriteLine($"Directory errors   : {DirectoryErrors:N0}");
            Console.WriteLine($"Transient retries  : {Retries:N0}");

            if (!string.IsNullOrEmpty(ErrorLogPath))
            {
                Console.WriteLine($"Error log          : {Path.GetFullPath(ErrorLogPath)}");
            }
        }
    }

    private void WriteErrorLine(string kind, string path, string message)
    {
        if (_errorWriter is null)
        {
            return;
        }

        string line = $"{DateTimeOffset.UtcNow:O}\t{kind}\t{Sanitize(path)}\t{message}";
        lock (_logLock)
        {
            _errorWriter.WriteLine(line);
        }
    }

    private static string FormatException(Exception ex)
    {
        var sb = new StringBuilder();
        sb.Append(ex.GetType().Name).Append(": ").Append(Sanitize(ex.Message));

        if (ex.InnerException is not null)
        {
            sb.Append(" -> ")
              .Append(ex.InnerException.GetType().Name)
              .Append(": ")
              .Append(Sanitize(ex.InnerException.Message));
        }

        return sb.ToString();
    }

    private static string Sanitize(string text) =>
        text.Replace('\r', ' ').Replace('\n', ' ').Replace('\t', ' ');

    public void Dispose()
    {
        lock (_logLock)
        {
            _errorWriter?.Dispose();
        }
    }
}
