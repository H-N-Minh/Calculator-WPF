// ------------------------------------------------------------------------------
// © SDK GmbH 
// ------------------------------------------------------------------------------

using System.Collections.Concurrent;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.IO;

namespace ClearPDFAttrib.TestRunner;

internal sealed record VerificationResult(long VerifiedPdfs, long FailureCount, IReadOnlyList<string> SampleFailures);

internal static class TestPdfVerifier
{
    private const int MaxStoredFailureSamples = 25;

    private static readonly XNamespace DcNs = "http://purl.org/dc/elements/1.1/";
    private static readonly XNamespace XmpNs = "http://ns.adobe.com/xap/1.0/";
    private static readonly XNamespace PdfNs = "http://ns.adobe.com/pdf/1.3/";

    private static readonly HashSet<XName> ForbiddenXmpNames =
    [
        DcNs + "title",
        DcNs + "creator",
        XmpNs + "CreatorTool",
        PdfNs + "Title",
        PdfNs + "Author",
        PdfNs + "Creator"
    ];

    private static readonly XmlReaderSettings SafeXmlSettings = new()
    {
        DtdProcessing = DtdProcessing.Prohibit,
        XmlResolver = null
    };

    public static VerificationResult Verify(string rootDir, long expectedCount, int threads)
    {
        long verifiedCount = 0;
        long failureCount = 0;
        var failureSamples = new ConcurrentQueue<string>();

        void RecordFailure(string message)
        {
            long current = Interlocked.Increment(ref failureCount);
            if (current <= MaxStoredFailureSamples)
                failureSamples.Enqueue(message);
        }

        // Verify decoy files remain intact
        VerifyDecoyFile(Path.Combine(rootDir, "ignore_me.txt"), "keep-txt", RecordFailure);
        VerifyDecoyFile(Path.Combine(rootDir, "Dokumente_2023", "06", "30", "decoy.pdf_bak"), "keep-bak", RecordFailure);

        // Stream PDFs in parallel without holding all paths in memory
        var enumOptions = new EnumerationOptions { RecurseSubdirectories = true };
        var partitioner = Partitioner.Create(
            Directory.EnumerateFiles(rootDir, "*.pdf", enumOptions),
            EnumerablePartitionerOptions.NoBuffering);

        Parallel.ForEach(partitioner, new ParallelOptions { MaxDegreeOfParallelism = threads }, filePath =>
        {
            try
            {
                VerifySinglePdf(filePath, RecordFailure);
            }
            catch (Exception ex)
            {
                RecordFailure($"{filePath}: Exception during verification: {ex.Message}");
            }

            long done = Interlocked.Increment(ref verifiedCount);
            if (expectedCount >= 10_000 && done % 50_000 == 0)
                Console.WriteLine($"      Verified {done:N0} / {expectedCount:N0}...");
        });

        if (verifiedCount != expectedCount)
            RecordFailure($"Expected {expectedCount:N0} PDFs, but found {verifiedCount:N0}.");

        return new VerificationResult(verifiedCount, failureCount, [.. failureSamples]);
    }

    private static void VerifySinglePdf(string filePath, Action<string> recordFailure)
    {
        string fileNameNoExt = Path.GetFileNameWithoutExtension(filePath);
        if (long.TryParse(fileNameNoExt, out long index) && (index % 4) == 3)
        {
            DateTime actualTimestamp = File.GetLastWriteTimeUtc(filePath);
            if (actualTimestamp != TestPdfGenerator.UntouchedTimestampUtc)
                recordFailure($"{filePath}: Clean file (scenario 3) was unexpectedly modified.");
        }

        using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.SequentialScan);
        using PdfDocument doc = PdfReader.Open(fs, PdfDocumentOpenMode.Import);

        // 1. Verify Document Information Dictionary
        var infoElements = doc.Info.Elements;
        if (infoElements.ContainsKey("/Title") || !string.IsNullOrEmpty(doc.Info.Title))
            recordFailure($"{filePath}: Info /Title still present ('{doc.Info.Title}').");

        if (infoElements.ContainsKey("/Author") || !string.IsNullOrEmpty(doc.Info.Author))
            recordFailure($"{filePath}: Info /Author still present ('{doc.Info.Author}').");

        if (infoElements.ContainsKey("/Creator") || !string.IsNullOrEmpty(doc.Info.Creator))
            recordFailure($"{filePath}: Info /Creator still present ('{doc.Info.Creator}').");

        if (doc.Info.Subject != TestPdfGenerator.PreservedSubject)
            recordFailure($"{filePath}: Preserved Info /Subject was altered ('{doc.Info.Subject}').");

        // 2. Verify XMP Metadata (if present)
        PdfDictionary? metadataDict = doc.Internals.Catalog.Elements["/Metadata"] switch
        {
            PdfReference { Value: PdfDictionary { Stream: not null } d } => d,
            PdfDictionary { Stream: not null } d => d,
            _ => null
        };

        if (metadataDict?.Stream is null)
            return;

        byte[] rawBytes = metadataDict.Stream.UnfilteredValue;
        string xml;
        using (var ms = new MemoryStream(rawBytes))
        using (var reader = new StreamReader(ms, Encoding.UTF8, detectEncodingFromByteOrderMarks: true))
        {
            xml = reader.ReadToEnd().Trim('\0', ' ', '\t', '\r', '\n');
        }

        XDocument xmp;
        using (var sr = new StringReader(xml))
        using (var xr = XmlReader.Create(sr, SafeXmlSettings))
        {
            xmp = XDocument.Load(xr, LoadOptions.None);
        }

        foreach (XElement el in xmp.Descendants())
        {
            if (ForbiddenXmpNames.Contains(el.Name))
                recordFailure($"{filePath}: XMP element <{el.Name}> still present.");

            foreach (XAttribute attr in el.Attributes())
            {
                if (ForbiddenXmpNames.Contains(attr.Name))
                    recordFailure($"{filePath}: XMP attribute {attr.Name} still present.");
            }
        }

        if (!HasXmpProperty(xmp, DcNs + "format") || !HasXmpProperty(xmp, PdfNs + "Producer"))
            recordFailure($"{filePath}: Preserved XMP properties (dc:format / pdf:Producer) were lost.");
    }

    private static bool HasXmpProperty(XDocument doc, XName name) =>
        doc.Descendants().Any(el => el.Name == name || el.Attributes().Any(a => a.Name == name));

    private static void VerifyDecoyFile(string path, string expectedContent, Action<string> recordFailure)
    {
        if (!File.Exists(path))
        {
            recordFailure($"Decoy file missing: {path}");
            return;
        }

        string actual = File.ReadAllText(path, Encoding.UTF8);
        if (actual != expectedContent)
            recordFailure($"Decoy file modified: {path}");
    }
}
