// ------------------------------------------------------------------------------
// © SDK GmbH 
// ------------------------------------------------------------------------------

using System.Security.Cryptography;
using Xunit.Abstractions;

namespace ClearPdfAttrib.Tests;

public sealed class DirectoryTraversalTests(ITestOutputHelper output)
{
    [Fact]
    public async Task RunAsync_ProcessesNestedPdfsSpacesAndUppercase_AndLeavesNonPdfsByteIdentical()
    {
        using var log = new TestOutputSession(output);
        using var work = SampleWorkspace.CreateCopyOfSubfolder("02_Traversal", log);

        string[] nonPdfFiles =
        [
            Path.Combine(work.WorkingDirectory, "Document2.txt"),
            Path.Combine(work.WorkingDirectory, "Document3.docx"),
            Path.Combine(work.WorkingDirectory, "FolderA", "Document5.txt"),
            Path.Combine(work.WorkingDirectory, "FolderA", "FolderB", "Document7.docx")
        ];

        Dictionary<string, byte[]> hashesBefore = nonPdfFiles.ToDictionary(
            path => path,
            path => SHA256.HashData(File.ReadAllBytes(path)));

        log.Log("Starting ClearPdfAttrib on 02_Traversal copy...");
        var options = new ProcessingOptions(
            RootDirectory: work.WorkingDirectory,
            MaxParallelism: 4,
            ProgressInterval: 2,
            ErrorLogPath: work.ErrorLogPath,
            Quiet: false);

        BatchProcessingResult result = await PdfBatchProcessor.RunAsync(options, CancellationToken.None);

        Assert.Equal(6, result.Processed);
        Assert.Equal(6, result.Modified);
        Assert.Equal(0, result.Failed);
        Assert.Equal(0, result.ExitCode);

        // Verify root, nested, space-containing, and uppercase .PDF files
        PdfTestDataBuilder.AssertCleanedAndIntact(Path.Combine(work.WorkingDirectory, "Document1.pdf"), expectXmpStream: true);
        PdfTestDataBuilder.AssertCleanedAndIntact(Path.Combine(work.WorkingDirectory, "UPPERCASE.PDF"), expectXmpStream: true);
        PdfTestDataBuilder.AssertCleanedAndIntact(Path.Combine(work.WorkingDirectory, "FolderA", "Document4.pdf"), expectXmpStream: false);
        PdfTestDataBuilder.AssertCleanedAndIntact(Path.Combine(work.WorkingDirectory, "FolderA", "FolderB", "Document6.pdf"), expectXmpStream: true);
        PdfTestDataBuilder.AssertCleanedAndIntact(Path.Combine(work.WorkingDirectory, "FolderC", "Document8.pdf"), expectXmpStream: true);
        PdfTestDataBuilder.AssertCleanedAndIntact(Path.Combine(work.WorkingDirectory, "Folder With Spaces", "Space Document.pdf"), expectXmpStream: true);

        // Verify non-PDF files remained byte-for-byte unchanged
        foreach (string nonPdf in nonPdfFiles)
        {
            Assert.True(File.Exists(nonPdf), $"Non-PDF file missing: {nonPdf}");
            byte[] hashAfter = SHA256.HashData(File.ReadAllBytes(nonPdf));
            Assert.Equal(hashesBefore[nonPdf], hashAfter);
        }
    }
}
