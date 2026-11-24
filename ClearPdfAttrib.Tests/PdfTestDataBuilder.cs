// ------------------------------------------------------------------------------
// © SDK GmbH 
// ------------------------------------------------------------------------------

using System.Text;
using System.Xml.Linq;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using iText.Kernel.XMP;
using iText.Layout;
using iText.Layout.Element;

namespace ClearPdfAttrib.Tests;

internal static class PdfTestDataBuilder
{
    public const string IntegrityText = "This is a PDF integrity test.";
    public const string PreservedSubject = "Preserved Subject";
    public const string PreservedProducer = "CustomProducer";

    public static void CreateSamplePdf(
        string path,
        bool includeInfo,
        bool includeXmp,
        bool includeCleanXmp = false,
        string contentText = IntegrityText)
    {
        string? dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir))
        {
            Directory.CreateDirectory(dir);
        }

        byte[] bytes = CreateSamplePdfBytes(includeInfo, includeXmp, includeCleanXmp, contentText);
        File.WriteAllBytes(path, bytes);
    }

    public static byte[] CreateSamplePdfBytes(
        bool includeInfo,
        bool includeXmp,
        bool includeCleanXmp = false,
        string contentText = IntegrityText)
    {
        using var ms = new MemoryStream();
        using (var writer = new PdfWriter(ms))
        using (var pdf = new PdfDocument(writer))
        using (var doc = new Document(pdf))
        {
            PdfDocumentInfo info = pdf.GetDocumentInfo();
            info.SetSubject(PreservedSubject);
            info.SetMoreInfo("Producer", PreservedProducer);

            if (includeInfo)
            {
                info.SetTitle("Sample Info Title");
                info.SetAuthor("Alice Example");
                info.SetCreator("Sample Word Processor");
            }

            if (includeXmp || includeCleanXmp)
            {
                XMPMeta xmp = pdf.GetXmpMetadata(true)!;

                if (includeXmp)
                {
                    xmp.SetProperty(XMPConst.NS_DC, "title", "Sample XMP Title");
                    xmp.SetProperty(XMPConst.NS_DC, "creator", "Bob XMP Author");
                    xmp.SetProperty(XMPConst.NS_XMP, "CreatorTool", "Sample XMP Tool");
                }

                xmp.SetProperty(XMPConst.NS_DC, "description", PreservedSubject);
                xmp.SetProperty(XMPConst.NS_PDF, "Producer", PreservedProducer);
                pdf.SetXmpMetadata(xmp);
            }

            doc.Add(new Paragraph(contentText));
        }

        return ms.ToArray();
    }

    public static void CreateCorruptedPdf(string path)
    {
        string? dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir))
        {
            Directory.CreateDirectory(dir);
        }

        File.WriteAllText(path, "%PDF-1.7\nCORRUPTED_NON_PDF_CONTENT_DO_NOT_MODIFY\n", Encoding.ASCII);
    }

    /// <summary>
    /// Verifies PDF readability, content integrity, and inspects raw Info dictionary
    /// and raw /Catalog /Metadata XMP stream independently.
    /// </summary>
    public static void AssertCleanedAndIntact(
        string path,
        bool expectXmpStream,
        string expectedText = IntegrityText,
        int expectedPageCount = 1)
    {
        var fileInfo = new FileInfo(path);
        Assert.True(fileInfo.Exists, $"Expected PDF file to exist: {path}");
        Assert.True(fileInfo.Length > 0, $"Expected non-empty PDF file: {path}");

        using var reader = new PdfReader(path);
        using var pdf = new PdfDocument(reader);

        // 1. Structural & content integrity
        Assert.Equal(expectedPageCount, pdf.GetNumberOfPages());
        string pageText = PdfTextExtractor.GetTextFromPage(pdf.GetPage(1), new SimpleTextExtractionStrategy());
        Assert.Contains(expectedText, pageText);

        // 2. Raw Info dictionary check (no high-level synchronization)
        PdfDictionary? info = pdf.GetTrailer().GetAsDictionary(PdfName.Info);
        Assert.NotNull(info);
        Assert.False(info.ContainsKey(PdfName.Title), $"Info /Title still present in {path}");
        Assert.False(info.ContainsKey(PdfName.Author), $"Info /Author still present in {path}");
        Assert.False(info.ContainsKey(PdfName.Creator), $"Info /Creator still present in {path}");

        Assert.True(info.ContainsKey(PdfName.Producer), $"Info /Producer missing in {path}");
        Assert.False(string.IsNullOrWhiteSpace(info.GetAsString(PdfName.Producer)?.ToUnicodeString()));
        Assert.Equal(PreservedSubject, info.GetAsString(PdfName.Subject)?.ToUnicodeString());

        // 3. Raw XMP stream check (/Catalog /Metadata bytes + raw XML inspection)
        PdfStream? xmpStream = pdf.GetCatalog().GetPdfObject().GetAsStream(PdfName.Metadata);
        if (expectXmpStream)
        {
            Assert.NotNull(xmpStream);
        }

        if (xmpStream is not null)
        {
            byte[] rawXmpBytes = xmpStream.GetBytes();
            XMPMeta xmp = XMPMetaFactory.ParseFromBuffer(rawXmpBytes);

            Assert.False(xmp.DoesPropertyExist(XMPConst.NS_DC, "title"), $"XMP dc:title still present in {path}");
            Assert.False(xmp.DoesPropertyExist(XMPConst.NS_DC, "creator"), $"XMP dc:creator still present in {path}");
            Assert.False(xmp.DoesPropertyExist(XMPConst.NS_XMP, "CreatorTool"), $"XMP xmp:CreatorTool still present in {path}");

            if (expectXmpStream)
            {
                Assert.True(xmp.DoesPropertyExist(XMPConst.NS_DC, "description"), $"XMP dc:description lost in {path}");
                Assert.True(xmp.DoesPropertyExist(XMPConst.NS_PDF, "Producer"), $"XMP pdf:Producer lost in {path}");
            }

            AssertRawXmpXmlCleaned(rawXmpBytes, path);
        }
    }

    private static void AssertRawXmpXmlCleaned(byte[] rawXmpBytes, string path)
    {
        string xmlText = Encoding.UTF8.GetString(rawXmpBytes).Trim('\0', '\uFEFF', ' ', '\t', '\r', '\n');
        var doc = XDocument.Parse(xmlText);

        XNamespace dc = XMPConst.NS_DC;
        XNamespace xmpNs = XMPConst.NS_XMP;

        XName[] forbidden = [dc + "title", dc + "creator", xmpNs + "CreatorTool"];

        foreach (XName name in forbidden)
        {
            bool hasElement = doc.Descendants(name).Any();
            bool hasAttribute = doc.Descendants().Attributes(name).Any();
            Assert.False(hasElement || hasAttribute, $"Raw XMP XML still contains '{name}' in {path}");
        }
    }
}
