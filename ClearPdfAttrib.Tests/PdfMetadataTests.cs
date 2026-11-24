// ------------------------------------------------------------------------------
// © SDK GmbH 
// ------------------------------------------------------------------------------

using Xunit.Abstractions;

namespace ClearPdfAttrib.Tests;

public sealed class PdfMetadataTests(ITestOutputHelper output)
{
    [Fact]
    public async Task InfoOnly_RemovesTitleAuthorCreator_AndPreservesUnrelatedMetadata()
    {
        using var log = new TestOutputSession(output);
        using var work = SampleWorkspace.CreateCopyOfSubfolder("01_Metadata", log);

        BatchProcessingResult result = await RunCleanerAsync(work, log);

        Assert.Equal(4, result.Processed);
        Assert.Equal(3, result.Modified);
        Assert.Equal(1, result.SkippedClean);
        Assert.Equal(0, result.Failed);
        Assert.Equal(0, result.ExitCode);

        PdfTestDataBuilder.AssertCleanedAndIntact(
            Path.Combine(work.WorkingDirectory, "InfoOnly.pdf"),
            expectXmpStream: false);
    }

    [Fact]
    public async Task XmpOnly_RemovesXmpMetadata_AndPreservesUnrelatedMetadata()
    {
        using var log = new TestOutputSession(output);
        using var work = SampleWorkspace.CreateCopyOfSubfolder("01_Metadata", log);

        BatchProcessingResult result = await RunCleanerAsync(work, log);
        Assert.Equal(0, result.ExitCode);

        PdfTestDataBuilder.AssertCleanedAndIntact(
            Path.Combine(work.WorkingDirectory, "XmpOnly.pdf"),
            expectXmpStream: true);
    }

    [Fact]
    public async Task Both_RemovesInfoAndXmpMetadata_Independently()
    {
        using var log = new TestOutputSession(output);
        using var work = SampleWorkspace.CreateCopyOfSubfolder("01_Metadata", log);

        BatchProcessingResult result = await RunCleanerAsync(work, log);
        Assert.Equal(0, result.ExitCode);

        PdfTestDataBuilder.AssertCleanedAndIntact(
            Path.Combine(work.WorkingDirectory, "Both.pdf"),
            expectXmpStream: true);
    }

    [Fact]
    public async Task Integrity_PreservesPagesTextContentAndFileSize()
    {
        using var log = new TestOutputSession(output);
        using var work = SampleWorkspace.CreateCopyOfSubfolder("01_Metadata", log);

        BatchProcessingResult result = await RunCleanerAsync(work, log);
        Assert.Equal(0, result.ExitCode);

        foreach (string file in Directory.EnumerateFiles(work.WorkingDirectory, "*.pdf"))
        {
            bool expectXmp = !Path.GetFileName(file).Equals("InfoOnly.pdf", StringComparison.OrdinalIgnoreCase);
            PdfTestDataBuilder.AssertCleanedAndIntact(
                file,
                expectXmpStream: expectXmp,
                expectedText: PdfTestDataBuilder.IntegrityText,
                expectedPageCount: 1);
        }
    }

    private static async Task<BatchProcessingResult> RunCleanerAsync(SampleWorkspace work, TestOutputSession log)
    {
        log.Log("Starting ClearPdfAttrib on 01_Metadata copy...");
        var options = new ProcessingOptions(
            RootDirectory: work.WorkingDirectory,
            MaxParallelism: 4,
            ProgressInterval: 2,
            ErrorLogPath: work.ErrorLogPath,
            Quiet: false);

        return await PdfBatchProcessor.RunAsync(options, CancellationToken.None);
    }
}
