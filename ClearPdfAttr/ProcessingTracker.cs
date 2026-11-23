// ------------------------------------------------------------------------------
// © SDK GmbH 
// ------------------------------------------------------------------------------

using System.Diagnostics;
using System.Text;

namespace ClearPDFAttrib;

internal sealed class ProcessingTracker : IAsyncDisposable
{
    private readonly Stopwatch _stopwatch = Stopwatch.StartNew();
    private readonly string _errorLogPath;
    private readonly Lock _logLock = new();
    private readonly CancellationTokenSource _timerCts = new();
    private readonly Task _progressTask;

    private StreamWriter? _errorWriter;
    private long _scanned;
    private long _modified;
    private long _unchanged;
    private long _errors;

    public ProcessingTracker(string errorLogPath)
    {
        _errorLogPath = errorLogPath;
        _progressTask = ReportLoopAsync(_timerCts.Token);
    }

    public long Errors => Volatile.Read(ref _errors);
    public bool HasLoggedErrors { get; private set; }

    public void RecordModified()
    {
        Interlocked.Increment(ref _modified);
        Interlocked.Increment(ref _scanned);
    }

    public void RecordUnchanged()
    {
        Interlocked.Increment(ref _unchanged);
        Interlocked.Increment(ref _scanned);
    }

    public void RecordError(string context, Exception ex, bool countAsScanned = true)
    {
        Interlocked.Increment(ref _errors);
        if (countAsScanned)
            Interlocked.Increment(ref _scanned);

        lock (_logLock)
        {
            _errorWriter ??= CreateWriter(_errorLogPath);
            _errorWriter.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {context} | {ex.GetType().Name}: {ex.Message}");
            HasLoggedErrors = true;
        }
    }

    public async Task PrintSummaryAsync()
    {
        await StopProgressLoopAsync().ConfigureAwait(false);
        _stopwatch.Stop();

        Console.WriteLine();
        Console.WriteLine("=== Summary ===");
        PrintProgress();

        if (HasLoggedErrors)
            Console.WriteLine($"Error log: {_errorLogPath}");
    }

    public async ValueTask DisposeAsync()
    {
        await StopProgressLoopAsync().ConfigureAwait(false);
        _timerCts.Dispose();

        lock (_logLock)
        {
            _errorWriter?.Dispose();
            _errorWriter = null;
        }
    }

    private async Task StopProgressLoopAsync()
    {
        if (!_timerCts.IsCancellationRequested)
            await _timerCts.CancelAsync().ConfigureAwait(false);

        await _progressTask.ConfigureAwait(false);
    }

    private async Task ReportLoopAsync(CancellationToken ct)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(5));
        try
        {
            while (await timer.WaitForNextTickAsync(ct).ConfigureAwait(false))
                PrintProgress();
        }
        catch (OperationCanceledException)
        {
            // Normal shutdown
        }
    }

    private void PrintProgress()
    {
        long scanned = Volatile.Read(ref _scanned);
        long modified = Volatile.Read(ref _modified);
        long unchanged = Volatile.Read(ref _unchanged);
        long errors = Volatile.Read(ref _errors);
        TimeSpan elapsed = _stopwatch.Elapsed;
        double rate = elapsed.TotalSeconds > 0 ? scanned / elapsed.TotalSeconds : 0;

        Console.WriteLine(
            $"[{elapsed:hh\\:mm\\:ss}] Scanned: {scanned:N0} | Cleaned: {modified:N0} | Unchanged: {unchanged:N0} | Errors: {errors:N0} | {rate:N0} PDFs/s");
    }

    private static StreamWriter CreateWriter(string path)
    {
        string? dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);

        return new StreamWriter(path, append: true, Encoding.UTF8) { AutoFlush = true };
    }
}
