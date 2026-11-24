// ------------------------------------------------------------------------------
// © SDK GmbH 
// ------------------------------------------------------------------------------

using System.Diagnostics;
using System.Text;
using Xunit.Abstractions;

namespace ClearPdfAttrib.Tests;

internal sealed class TestOutputSession : IDisposable
{
    private readonly ITestOutputHelper _output;
    private readonly Lock _writeLock = new();
    private readonly TextWriter _originalOut;
    private readonly TextWriter _originalError;
    private readonly PrefixedWriter _mainOutWriter;
    private readonly PrefixedWriter _mainErrWriter;
    private bool _disposed;

    public TestOutputSession(ITestOutputHelper output)
    {
        _output = output;
        _originalOut = Console.Out;
        _originalError = Console.Error;

        _mainOutWriter = new PrefixedWriter(this, "[MAIN] ");
        _mainErrWriter = new PrefixedWriter(this, "[MAIN ERR] ");

        Console.SetOut(_mainOutWriter);
        Console.SetError(_mainErrWriter);
    }

    public void Log(string message)
    {
        WriteLineInternal($"[TEST] {message}");
    }

    private void WriteLineInternal(string line)
    {
        lock (_writeLock)
        {
            if (!_disposed)
            {
                _output.WriteLine(line);
            }
        }
    }

    public void Dispose()
    {
        lock (_writeLock)
        {
            if (_disposed)
            {
                return;
            }

            _mainOutWriter.Flush();
            _mainErrWriter.Flush();
            Console.SetOut(_originalOut);
            Console.SetError(_originalError);
            _disposed = true;
        }
    }

    private sealed class PrefixedWriter(TestOutputSession session, string prefix) : TextWriter
    {
        private readonly StringBuilder _buffer = new();

        public override Encoding Encoding => Encoding.UTF8;

        public override void Write(char value)
        {
            lock (session._writeLock)
            {
                if (value == '\n')
                {
                    FlushBuffer_NoLock();
                }
                else if (value != '\r')
                {
                    _buffer.Append(value);
                }
            }
        }

        public override void Write(string? value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return;
            }

            lock (session._writeLock)
            {
                foreach (char ch in value)
                {
                    if (ch == '\n')
                    {
                        FlushBuffer_NoLock();
                    }
                    else if (ch != '\r')
                    {
                        _buffer.Append(ch);
                    }
                }
            }
        }

        public override void WriteLine(string? value)
        {
            lock (session._writeLock)
            {
                if (!string.IsNullOrEmpty(value))
                {
                    foreach (char ch in value)
                    {
                        if (ch == '\n')
                        {
                            FlushBuffer_NoLock();
                        }
                        else if (ch != '\r')
                        {
                            _buffer.Append(ch);
                        }
                    }
                }

                FlushBuffer_NoLock();
            }
        }

        public override void Flush()
        {
            lock (session._writeLock)
            {
                if (_buffer.Length > 0)
                {
                    FlushBuffer_NoLock();
                }
            }
        }

        private void FlushBuffer_NoLock()
        {
            if (!session._disposed)
            {
                session._output.WriteLine($"{prefix}{_buffer}");
            }

            _buffer.Clear();
        }
    }
}

internal sealed class SampleWorkspace : IDisposable
{
    private static readonly Lock CacheLock = new();
    private const string ManifestFileName = ".sample-count";

    private readonly string _runRoot;
    private readonly TestOutputSession _log;
    public string WorkingDirectory { get; }
    public string ErrorLogPath { get; }

    private SampleWorkspace(string runRoot, string workingDirectory, TestOutputSession log)
    {
        _runRoot = runRoot;
        _log = log;
        WorkingDirectory = workingDirectory;
        ErrorLogPath = Path.Combine(runRoot, "errors.log");
    }

    public static SampleWorkspace CreateCopyOfSubfolder(string subfolder, TestOutputSession log)
    {
        EnsureSampleCacheReady(log);

        string sourceDir = Path.Combine(TestSettings.SampleCacheDirectory, subfolder);
        string runRoot = Path.Combine(Path.GetTempPath(), "ClearPdfAttrib.TestRuns", Guid.NewGuid().ToString("N"));
        string workingDir = Path.Combine(runRoot, subfolder);

        long start = Stopwatch.GetTimestamp();
        int copiedFiles = CopyDirectory(sourceDir, workingDir);
        double elapsedSec = Stopwatch.GetElapsedTime(start).TotalSeconds;

        log.Log($"Copied {copiedFiles:N0} files from '{sourceDir}' to '{workingDir}' in {elapsedSec:F2}s.");
        return new SampleWorkspace(runRoot, workingDir, log);
    }

    private static void EnsureSampleCacheReady(TestOutputSession log)
    {
        lock (CacheLock)
        {
            string cacheRoot = TestSettings.SampleCacheDirectory;
            string manifestPath = Path.Combine(cacheRoot, ManifestFileName);
            string expectedCount = TestSettings.TotalPdfCount.ToString();

            if (Directory.Exists(cacheRoot) &&
                File.Exists(manifestPath) &&
                File.ReadAllText(manifestPath).Trim() == expectedCount)
            {
                log.Log($"Reusing existing sample cache at '{cacheRoot}' ({TestSettings.TotalPdfCount:N0} PDFs).");
                return;
            }

            log.Log($"Sample cache missing or PDF count changed. Creating {TestSettings.TotalPdfCount:N0} samples at '{cacheRoot}'...");
            long start = Stopwatch.GetTimestamp();

            string stagingRoot = $"{cacheRoot}.staging_{Guid.NewGuid():N}";
            try
            {
                BuildAllSamples(stagingRoot, log);
                File.WriteAllText(Path.Combine(stagingRoot, ManifestFileName), expectedCount);

                if (Directory.Exists(cacheRoot))
                {
                    Directory.Delete(cacheRoot, recursive: true);
                }

                Directory.Move(stagingRoot, cacheRoot);
            }
            catch
            {
                if (Directory.Exists(stagingRoot))
                {
                    Directory.Delete(stagingRoot, recursive: true);
                }
                throw;
            }

            log.Log($"Sample cache built in {Stopwatch.GetElapsedTime(start).TotalSeconds:F2}s.");
        }
    }

    private static void BuildAllSamples(string root, TestOutputSession log)
    {
        // 01_Metadata (4 PDFs)
        string meta = Path.Combine(root, "01_Metadata");
        PdfTestDataBuilder.CreateSamplePdf(Path.Combine(meta, "InfoOnly.pdf"), includeInfo: true, includeXmp: false);
        PdfTestDataBuilder.CreateSamplePdf(Path.Combine(meta, "XmpOnly.pdf"), includeInfo: false, includeXmp: true);
        PdfTestDataBuilder.CreateSamplePdf(Path.Combine(meta, "Both.pdf"), includeInfo: true, includeXmp: true);
        PdfTestDataBuilder.CreateSamplePdf(Path.Combine(meta, "Clean.pdf"), includeInfo: false, includeXmp: false, includeCleanXmp: true);

        // 02_Traversal (6 PDFs + 4 non-PDFs)
        string trav = Path.Combine(root, "02_Traversal");
        PdfTestDataBuilder.CreateSamplePdf(Path.Combine(trav, "Document1.pdf"), includeInfo: true, includeXmp: true);
        File.WriteAllText(Path.Combine(trav, "Document2.txt"), "Plain text document - must stay unchanged.");
        File.WriteAllBytes(Path.Combine(trav, "Document3.docx"), [0x50, 0x4B, 0x03, 0x04, 0x14, 0x00, 0x64, 0x6F, 0x63, 0x78]);
        PdfTestDataBuilder.CreateSamplePdf(Path.Combine(trav, "UPPERCASE.PDF"), includeInfo: true, includeXmp: true);

        PdfTestDataBuilder.CreateSamplePdf(Path.Combine(trav, "FolderA", "Document4.pdf"), includeInfo: true, includeXmp: false);
        File.WriteAllText(Path.Combine(trav, "FolderA", "Document5.txt"), "Nested text file.");
        PdfTestDataBuilder.CreateSamplePdf(Path.Combine(trav, "FolderA", "FolderB", "Document6.pdf"), includeInfo: false, includeXmp: true);
        File.WriteAllBytes(Path.Combine(trav, "FolderA", "FolderB", "Document7.docx"), [0x50, 0x4B, 0x03, 0x04, 0x00, 0x01, 0x02]);

        PdfTestDataBuilder.CreateSamplePdf(Path.Combine(trav, "FolderC", "Document8.pdf"), includeInfo: true, includeXmp: true);
        PdfTestDataBuilder.CreateSamplePdf(Path.Combine(trav, "Folder With Spaces", "Space Document.pdf"), includeInfo: true, includeXmp: true);

        // 03_ErrorHandling (3 PDFs)
        string err = Path.Combine(root, "03_ErrorHandling");
        PdfTestDataBuilder.CreateSamplePdf(Path.Combine(err, "Valid1.pdf"), includeInfo: true, includeXmp: true);
        PdfTestDataBuilder.CreateCorruptedPdf(Path.Combine(err, "Corrupted.pdf"));
        PdfTestDataBuilder.CreateSamplePdf(Path.Combine(err, "Valid2.pdf"), includeInfo: true, includeXmp: true);

        // 04_Bulk (Remaining PDFs distributed across realistic Sherpa/Strom directory hierarchy)
        int bulkCount = TestSettings.BulkPdfCount;
        string bulkRoot = Path.Combine(root, "04_Bulk");
        byte[][] templates =
        [
            PdfTestDataBuilder.CreateSamplePdfBytes(includeInfo: true, includeXmp: false),
            PdfTestDataBuilder.CreateSamplePdfBytes(includeInfo: false, includeXmp: true),
            PdfTestDataBuilder.CreateSamplePdfBytes(includeInfo: true, includeXmp: true)
        ];

        string[] paths = new string[bulkCount];
        var dirs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        for (int i = 0; i < bulkCount; i++)
        {
            string dir = Path.Combine(
                bulkRoot,
                "Sherpa",
                "Strom",
                $"Dokumente_{2021 + (i % 4)}",
                $"{(i % 12) + 1:D2}",
                $"{(i % 28) + 1:D2}",
                $"{(i / 500) + 1:D5}");

            dirs.Add(dir);
            paths[i] = Path.Combine(dir, $"{i + 1:D13}.pdf");
        }

        foreach (string dir in dirs)
        {
            Directory.CreateDirectory(dir);
        }

        int created = 0;
        int reportStep = Math.Clamp(bulkCount / 5, 500, 5_000);

        Parallel.For(0, bulkCount, i =>
        {
            File.WriteAllBytes(paths[i], templates[i % 3]);
            int count = Interlocked.Increment(ref created);
            if (bulkCount >= 500 && count % reportStep == 0)
            {
                log.Log($"Sample generation progress: {count:N0} / {bulkCount:N0} bulk PDFs...");
            }
        });
    }

    private static int CopyDirectory(string sourceDir, string destDir)
    {
        Directory.CreateDirectory(destDir);

        foreach (string subDir in Directory.EnumerateDirectories(sourceDir, "*", SearchOption.AllDirectories))
        {
            string rel = Path.GetRelativePath(sourceDir, subDir);
            Directory.CreateDirectory(Path.Combine(destDir, rel));
        }

        string[] files = Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories);
        Parallel.ForEach(files, srcFile =>
        {
            string rel = Path.GetRelativePath(sourceDir, srcFile);
            File.Copy(srcFile, Path.Combine(destDir, rel), overwrite: true);
        });

        return files.Length;
    }

    public void Dispose()
    {
        if (File.Exists(ErrorLogPath))
        {
            string[] errorLines = File.ReadAllLines(ErrorLogPath)
                .Where(line => !string.IsNullOrWhiteSpace(line) && !line.StartsWith('#'))
                .ToArray();

            if (errorLines.Length > 0)
            {
                _log.Log($"--- errors.log ({errorLines.Length} entries, showing up to 20) ---");
                foreach (string line in errorLines.Take(20))
                {
                    _log.Log(line);
                }
            }
        }

        if (!Directory.Exists(_runRoot))
        {
            return;
        }

        for (int attempt = 0; attempt < 3; attempt++)
        {
            try
            {
                Directory.Delete(_runRoot, recursive: true);
                return;
            }
            catch (IOException) when (attempt < 2)
            {
                Thread.Sleep(50);
            }
        }
    }
}
