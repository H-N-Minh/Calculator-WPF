// ------------------------------------------------------------------------------
// © SDK GmbH 
// ------------------------------------------------------------------------------

using System.Text;
using System.Threading.Channels;
using ClearPDFAttrib;

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

if (!CliOptions.TryParse(args, out CliOptions? options))
{
    CliOptions.PrintUsage();
    return 1;
}

if (!Directory.Exists(options.RootDirectory))
{
    Console.Error.WriteLine($"Error: Directory not found: {options.RootDirectory}");
    return 1;
}

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    Console.WriteLine("\nCancellation requested, finishing in-flight files...");
    cts.Cancel();
};

await using var tracker = new ProcessingTracker(options.ErrorLogPath);

var channel = Channel.CreateBounded<string>(new BoundedChannelOptions(2048)
{
    SingleWriter = true,
    SingleReader = false,
    FullMode = BoundedChannelFullMode.Wait
});

Console.WriteLine($"Root:    {options.RootDirectory}");
Console.WriteLine($"Threads: {options.Threads}");
Console.WriteLine($"Mode:    {(options.DryRun ? "DRY-RUN (no changes)" : "LIVE (modifying files)")}");
Console.WriteLine();

Task producer = Task.Run(() => EnumeratePdfsAsync(options.RootDirectory, channel.Writer, tracker, cts.Token));
Task[] workers = [.. Enumerable.Range(0, options.Threads)
    .Select(_ => Task.Run(() => ProcessQueueAsync(channel.Reader, options.DryRun, tracker, cts.Token)))];

await Task.WhenAll([producer, .. workers]);
await tracker.PrintSummaryAsync();

return tracker.Errors > 0 ? 2 : 0;

static async Task EnumeratePdfsAsync(
    string rootDir,
    ChannelWriter<string> writer,
    ProcessingTracker tracker,
    CancellationToken ct)
{
    var enumOptions = new EnumerationOptions
    {
        RecurseSubdirectories = true,
        IgnoreInaccessible = true,
        MatchCasing = MatchCasing.CaseInsensitive,
        AttributesToSkip = FileAttributes.ReparsePoint
    };

    try
    {
        foreach (string filePath in Directory.EnumerateFiles(rootDir, "*.pdf", enumOptions))
        {
            if (ct.IsCancellationRequested)
                break;

            // Guard against Windows 8.3 short-name matches (e.g. file.pdf_bak -> FILE~1.PDF)
            if (!filePath.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                continue;

            await writer.WriteAsync(filePath, ct).ConfigureAwait(false);
        }
    }
    catch (OperationCanceledException)
    {
        // Graceful Ctrl+C shutdown
    }
    catch (Exception ex)
    {
        tracker.RecordError($"[ENUMERATION] {rootDir}", ex, countAsScanned: false);
    }
    finally
    {
        writer.TryComplete();
    }
}

static async Task ProcessQueueAsync(
    ChannelReader<string> reader,
    bool dryRun,
    ProcessingTracker tracker,
    CancellationToken ct)
{
    try
    {
        await foreach (string filePath in reader.ReadAllAsync(ct).ConfigureAwait(false))
        {
            try
            {
                if (PdfMetadataCleaner.ProcessFile(filePath, dryRun))
                    tracker.RecordModified();
                else
                    tracker.RecordUnchanged();
            }
            catch (Exception ex)
            {
                tracker.RecordError(filePath, ex);
            }
        }
    }
    catch (OperationCanceledException)
    {
        // Exits cleanly after completing the current file
    }
}
