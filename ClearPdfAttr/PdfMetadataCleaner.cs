// ------------------------------------------------------------------------------
// © SDK GmbH 
// ------------------------------------------------------------------------------

using System.Buffers;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.IO;

namespace ClearPDFAttrib;

internal static class PdfMetadataCleaner
{
    private const int InMemoryThresholdBytes = 10 * 1024 * 1024; // 10 MB

    private static readonly XNamespace DcNs = "http://purl.org/dc/elements/1.1/";
    private static readonly XNamespace XmpNs = "http://ns.adobe.com/xap/1.0/";
    private static readonly XNamespace PdfNs = "http://ns.adobe.com/pdf/1.3/";

    private static readonly HashSet<XName> TargetXmpNames =
    [
        DcNs + "title",
        DcNs + "creator",       // PDF /Author maps to dc:creator
        XmpNs + "CreatorTool",  // PDF /Creator maps to xmp:CreatorTool (or xap:CreatorTool)
        PdfNs + "Title",        // Fallback for non-standard generators
        PdfNs + "Author",
        PdfNs + "Creator"
    ];

    private static readonly SearchValues<string> XmpCandidateTerms =
        SearchValues.Create(["title", "creator", "author"], StringComparison.OrdinalIgnoreCase);

    private static readonly XmlReaderSettings SafeXmlReaderSettings = new()
    {
        DtdProcessing = DtdProcessing.Prohibit,
        XmlResolver = null
    };

    private static readonly Encoding Utf8NoBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

    public static bool ProcessFile(string filePath, bool dryRun)
    {
        byte[]? rentedBuffer = null;
        string? tempPath = null;

        try
        {
            using (Stream inputStream = OpenPdfStream(filePath, out rentedBuffer))
            using (PdfDocument document = PdfReader.Open(inputStream, PdfDocumentOpenMode.Modify))
            {
                bool infoChanged = ClearDocumentInfo(document);
                bool xmpChanged = ClearXmpMetadata(document);

                if (!infoChanged && !xmpChanged)
                    return false;

                if (!dryRun)
                    tempPath = SaveToTempFile(document, filePath);
            }

            // inputStream and document are now closed -> safe to replace original file
            if (tempPath is not null)
            {
                File.Move(tempPath, filePath, overwrite: true);
                tempPath = null;
            }

            return true;
        }
        finally
        {
            DeleteQuietly(tempPath);

            if (rentedBuffer is not null)
                ArrayPool<byte>.Shared.Return(rentedBuffer);
        }
    }

    private static Stream OpenPdfStream(string filePath, out byte[]? rentedBuffer)
    {
        rentedBuffer = null;

        var fileStream = new FileStream(
            filePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 4096,
            FileOptions.SequentialScan);

        try
        {
            long length = fileStream.Length;
            if (length == 0)
                throw new InvalidDataException("Empty (0-byte) PDF file.");

            if (length > InMemoryThresholdBytes)
                return fileStream;

            int size = (int)length;
            byte[] buffer = ArrayPool<byte>.Shared.Rent(size);
            try
            {
                fileStream.ReadExactly(buffer, 0, size);
                fileStream.Dispose();

                rentedBuffer = buffer;
                return new MemoryStream(buffer, 0, size, writable: false);
            }
            catch
            {
                ArrayPool<byte>.Shared.Return(buffer);
                throw;
            }
        }
        catch
        {
            fileStream.Dispose();
            throw;
        }
    }

    private static bool ClearDocumentInfo(PdfDocument document)
    {
        var elements = document.Info.Elements;
        bool changed = elements.Remove("/Title");
        changed |= elements.Remove("/Author");
        changed |= elements.Remove("/Creator");
        return changed;
    }

    private static bool ClearXmpMetadata(PdfDocument document)
    {
        PdfDictionary? metadataDict = document.Internals.Catalog.Elements["/Metadata"] switch
        {
            PdfReference { Value: PdfDictionary { Stream: not null } d } => d,
            PdfDictionary { Stream: not null } d => d,
            _ => null
        };

        if (metadataDict?.Stream is null)
            return false;

        byte[] rawBytes = metadataDict.Stream.UnfilteredValue;
        if (rawBytes is not { Length: > 0 })
            return false;

        string xml = DecodeXmpBytes(rawBytes);
        if (string.IsNullOrWhiteSpace(xml) || !xml.AsSpan().ContainsAny(XmpCandidateTerms))
            return false;

        XDocument doc;
        using (var stringReader = new StringReader(xml))
        using (var xmlReader = XmlReader.Create(stringReader, SafeXmlReaderSettings))
        {
            doc = XDocument.Load(xmlReader, LoadOptions.None);
        }

        List<XObject>? toRemove = null;
        foreach (XElement element in doc.Descendants())
        {
            if (TargetXmpNames.Contains(element.Name))
            {
                (toRemove ??= []).Add(element);
                continue;
            }

            foreach (XAttribute attr in element.Attributes())
            {
                if (TargetXmpNames.Contains(attr.Name))
                    (toRemove ??= []).Add(attr);
            }
        }

        if (toRemove is null)
            return false;

        foreach (XObject obj in toRemove)
        {
            switch (obj)
            {
                case XElement el: el.Remove(); break;
                case XAttribute attr: attr.Remove(); break;
            }
        }

        if (doc.Declaration is not null)
            doc.Declaration.Encoding = "utf-8";

        var writerSettings = new XmlWriterSettings
        {
            Encoding = Utf8NoBom,
            OmitXmlDeclaration = doc.Declaration is null,
            Indent = true
        };

        using var outMs = new MemoryStream();
        using (var xmlWriter = XmlWriter.Create(outMs, writerSettings))
        {
            doc.Save(xmlWriter);
        }

        metadataDict.Stream.Value = outMs.ToArray();
        metadataDict.Elements.Remove("/Filter");
        metadataDict.Elements.Remove("/DecodeParms");
        return true;
    }

    private static string DecodeXmpBytes(byte[] rawBytes)
    {
        using var ms = new MemoryStream(rawBytes);
        using var reader = new StreamReader(ms, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        return reader.ReadToEnd().Trim('\0', ' ', '\t', '\r', '\n');
    }

    private static string SaveToTempFile(PdfDocument document, string targetPath)
    {
        string directory = Path.GetDirectoryName(targetPath)!;
        string fileName = Path.GetFileName(targetPath);
        string tempPath = Path.Combine(directory, $".{fileName}.{Guid.NewGuid():N}.tmp");

        using var outStream = new FileStream(
            tempPath,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None,
            bufferSize: 64 * 1024);

        document.Save(outStream, closeStream: false);
        outStream.Flush();
        return tempPath;
    }

    private static void DeleteQuietly(string? path)
    {
        if (path is null)
            return;

        try
        {
            if (File.Exists(path))
                File.Delete(path);
        }
        catch
        {
            // Ignore cleanup failure
        }
    }
}
