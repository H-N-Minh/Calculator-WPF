// ------------------------------------------------------------------------------
// © SDK GmbH 
// ------------------------------------------------------------------------------

using System.Security.Cryptography;
using Xunit.Abstractions;

namespace ClearPdfAttrib.Tests;

public sealed class BatchSafetyAndScaleTests(ITestOutputHelper output)
{
    [Fact]
    public async Task CorruptedPdf_DoesNotStopBatch_LeavesCorruptUntouched_AndLeavesNoTempFiles()
    {
        using var log = new TestOutputSession(output);
        using var work = SampleWorkspace.CreateCopyOfSubfolder("03_ErrorHandling", log);

        string corruptPath = Path.Combine(work.WorkingDirectory, "Corrupted.pdf");
        byte[] corruptBytesBefore = File.ReadAllBytes(corruptPath);

        log.Log("Starting ClearPdfAttrib on 03_ErrorHandling copy...");
        var options = new ProcessingOptions(
            RootDirectory: work.WorkingDirectory,
            MaxParallelism: 2,
            ProgressInterval: 1,
            ErrorLogPath: work.ErrorLogPath,
            Quiet: false);

        BatchProcessingResult result = await PdfBatchProcessor.RunAsync(options, CancellationToken.None);

        // Consistent counters + exit code 2
        Assert.Equal(3, result.Processed);
        Assert.Equal(2, result.Modified);
        Assert.Equal(0, result.SkippedClean);
        Assert.Equal(0, result.SkippedSigned);
        Assert.Equal(1, result.Failed);
        Assert.Equal(0, result.DirectoryErrors);
        Assert.Equal(2, result.ExitCode);
        Assert.Equal(
            result.Processed,
            result.Modified + result.SkippedClean + result.SkippedSigned + result.Failed);

        // Valid files processed; corrupted file untouched
        PdfTestDataBuilder.AssertCleanedAndIntact(Path.Combine(work.WorkingDirectory, "Valid1.pdf"), expectXmpStream: true);
        PdfTestDataBuilder.AssertCleanedAndIntact(Path.Combine(work.WorkingDirectory, "Valid2.pdf"), expectXmpStream: true);
        Assert.Equal(corruptBytesBefore, File.ReadAllBytes(corruptPath));

        // No temporary files left behind in target folder
        string[] remainingFiles = Directory
            .GetFiles(work.WorkingDirectory, "*", SearchOption.AllDirectories)
            .Select(Path.GetFileName)
            .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
            .ToArray()!;
        Assert.Equal(["Corrupted.pdf", "Valid1.pdf", "Valid2.pdf"], remainingFiles);

        // Error log recorded failure
        Assert.True(File.Exists(work.ErrorLogPath));
        Assert.Contains("Corrupted.pdf", File.ReadAllText(work.ErrorLogPath));
    }

    [Fact]
    public async Task AlreadyCleanAndRepeatedExecution_SkipsWithoutModifying()
    {
        using var log = new TestOutputSession(output);
        using var work = SampleWorkspace.CreateCopyOfSubfolder("01_Metadata", log);

        var options = new ProcessingOptions(
            RootDirectory: work.WorkingDirectory,
            MaxParallelism: 4,
            ProgressInterval: 2,
            ErrorLogPath: work.ErrorLogPath,
            Quiet: false);

        log.Log("Pass 1 (3 dirty + 1 clean)...");
        BatchProcessingResult first = await PdfBatchProcessor.RunAsync(options, CancellationToken.None);
        Assert.Equal(4, first.Processed);
        Assert.Equal(3, first.Modified);
        Assert.Equal(1, first.SkippedClean);
        Assert.Equal(0, first.ExitCode);

        Dictionary<string, byte[]> hashesAfterFirstRun = Directory
            .GetFiles(work.WorkingDirectory, "*.pdf")
            .ToDictionary(p => p, p => SHA256.HashData(File.ReadAllBytes(p)));

        log.Log("Pass 2 (all 4 already clean)...");
        BatchProcessingResult second = await PdfBatchProcessor.RunAsync(options, CancellationToken.None);
        Assert.Equal(4, second.Processed);
        Assert.Equal(0, second.Modified);
        Assert.Equal(4, second.SkippedClean);
        Assert.Equal(0, second.ExitCode);

        foreach ((string path, byte[] expectedHash) in hashesAfterFirstRun)
        {
            Assert.Equal(expectedHash, SHA256.HashData(File.ReadAllBytes(path)));
        }
    }

    [Theory]
    [InlineData(1)]
    [InlineData(4)]
    public async Task BulkDataset_ParallelProcessing_CleansAllPdfs(int workers)
    {
        using var log = new TestOutputSession(output);
        using var work = SampleWorkspace.CreateCopyOfSubfolder("04_Bulk", log);

        int expectedCount = TestSettings.BulkPdfCount;
        int progressInterval = TestSettings.GetProgressInterval(expectedCount);

        log.Log($"Running ClearPdfAttrib with {workers} worker(s) on {expectedCount:N0} PDFs (progress every {progressInterval:N0})...");
        var options = new ProcessingOptions(
            RootDirectory: work.WorkingDirectory,
            MaxParallelism: workers,
            ProgressInterval: progressInterval,
            ErrorLogPath: work.ErrorLogPath,
            Quiet: false);

        BatchProcessingResult result = await PdfBatchProcessor.RunAsync(options, CancellationToken.None);

        Assert.Equal(expectedCount, result.Processed);
        Assert.Equal(expectedCount, result.Modified);
        Assert.Equal(0, result.Failed);
        Assert.Equal(0, result.DirectoryErrors);
        Assert.Equal(0, result.ExitCode);

        string[] files = Directory.GetFiles(work.WorkingDirectory, "*.pdf", SearchOption.AllDirectories);
        Assert.Equal(expectedCount, files.Length);

        log.Log($"Verifying {files.Length:N0} processed PDFs...");
        int verified = 0;
        Parallel.ForEach(files, file =>
        {
            int index = int.Parse(Path.GetFileNameWithoutExtension(file)) - 1;
            bool expectXmp = (index % 3) != 0;
            PdfTestDataBuilder.AssertCleanedAndIntact(file, expectXmpStream: expectXmp);

            int done = Interlocked.Increment(ref verified);
            if (files.Length >= 500 && done % progressInterval == 0)
            {
                log.Log($"Verified {done:N0} / {files.Length:N0} PDFs...");
            }
        });

        log.Log($"Completed verification for {workers} worker(s).");
    }
}
