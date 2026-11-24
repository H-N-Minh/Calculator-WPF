// ------------------------------------------------------------------------------
// © SDK GmbH 
// ------------------------------------------------------------------------------
using iText.Kernel.Exceptions;
using iText.Kernel.Pdf;
using iText.Kernel.XMP;

namespace ClearPdfAttrib;

public enum PdfInspectionStatus
{
    CleanAlready,
    NeedsCleaning,
    SkippedSigned
}

public static class PdfMetadataCleaner
{
    public static PdfInspectionStatus Inspect(string sourcePath, bool skipSigned)
    {
        using var reader = new PdfReader(sourcePath);
        using var pdf = new PdfDocument(reader);

        if (reader.IsEncrypted() && !reader.IsOpenedWithFullPermission())
        {
            throw new BadPasswordException("PDF is encrypted without full (owner) permissions.");
        }

        if (skipSigned && HasDigitalSignatures(pdf))
        {
            return PdfInspectionStatus.SkippedSigned;
        }

        return HasTargetMetadata(pdf)
            ? PdfInspectionStatus.NeedsCleaning
            : PdfInspectionStatus.CleanAlready;
    }

    public static void ClearMetadata(string sourcePath, string destinationPath)
    {
        using var reader = new PdfReader(sourcePath);
        using var writer = new PdfWriter(destinationPath);
        using var pdf = new PdfDocument(reader, writer);

        if (reader.IsEncrypted() && !reader.IsOpenedWithFullPermission())
        {
            throw new BadPasswordException("PDF is encrypted without full (owner) permissions.");
        }

        // Initializing DocumentInfo syncs existing XMP -> Info in iText 9.
        // Removing keys from both representations before SetXmpMetadata/Close prevents re-sync.
        PdfDocumentInfo info = pdf.GetDocumentInfo();
        info.SetMoreInfo("Title", null);
        info.SetMoreInfo("Author", null);
        info.SetMoreInfo("Creator", null);

        PdfDictionary? infoDict = pdf.GetTrailer().GetAsDictionary(PdfName.Info);
        if (infoDict is not null)
        {
            infoDict.Remove(PdfName.Title);
            infoDict.Remove(PdfName.Author);
            infoDict.Remove(PdfName.Creator);
        }

        XMPMeta? xmp = pdf.GetXmpMetadata(false);
        if (xmp is not null)
        {
            DeleteTargetXmpProperties(xmp);
            pdf.SetXmpMetadata(xmp);
        }
    }

    private static bool HasTargetMetadata(PdfDocument pdf)
    {
        // Inspect the raw /Info dictionary without triggering GetDocumentInfo() lazy sync.
        PdfDictionary? infoDict = pdf.GetTrailer().GetAsDictionary(PdfName.Info);
        if (infoDict is not null &&
            (HasNonNullEntry(infoDict, PdfName.Title) ||
             HasNonNullEntry(infoDict, PdfName.Author) ||
             HasNonNullEntry(infoDict, PdfName.Creator)))
        {
            return true;
        }

        XMPMeta? xmp = pdf.GetXmpMetadata(false);
        return xmp is not null && HasTargetXmpProperties(xmp);
    }

    private static bool HasNonNullEntry(PdfDictionary dict, PdfName key)
    {
        if (!dict.ContainsKey(key))
        {
            return false;
        }

        PdfObject? value = dict.Get(key);
        return value is not null and not PdfNull;
    }

    private static bool HasTargetXmpProperties(XMPMeta xmp)
    {
        return xmp.DoesPropertyExist(XMPConst.NS_DC, "title")
            || xmp.DoesPropertyExist(XMPConst.NS_DC, "creator")
            || xmp.DoesPropertyExist(XMPConst.NS_XMP, "CreatorTool")
            || xmp.DoesPropertyExist(XMPConst.NS_PDF, "Title")
            || xmp.DoesPropertyExist(XMPConst.NS_PDF, "Author")
            || xmp.DoesPropertyExist(XMPConst.NS_PDF, "Creator");
    }

    private static void DeleteTargetXmpProperties(XMPMeta xmp)
    {
        xmp.DeleteProperty(XMPConst.NS_DC, "title");
        xmp.DeleteProperty(XMPConst.NS_DC, "creator");
        xmp.DeleteProperty(XMPConst.NS_XMP, "CreatorTool");
        xmp.DeleteProperty(XMPConst.NS_PDF, "Title");
        xmp.DeleteProperty(XMPConst.NS_PDF, "Author");
        xmp.DeleteProperty(XMPConst.NS_PDF, "Creator");
    }

    private static bool HasDigitalSignatures(PdfDocument pdf)
    {
        PdfDictionary catalogDict = pdf.GetCatalog().GetPdfObject();

        PdfDictionary? perms = catalogDict.GetAsDictionary(PdfName.Perms);
        if (perms is not null && (perms.ContainsKey(PdfName.DocMDP) || perms.ContainsKey(PdfName.UR3)))
        {
            return true;
        }

        PdfDictionary? acroForm = catalogDict.GetAsDictionary(PdfName.AcroForm);
        PdfArray? fields = acroForm?.GetAsArray(PdfName.Fields);
        if (fields is null || fields.IsEmpty())
        {
            return false;
        }

        var visited = new HashSet<PdfDictionary>(ReferenceEqualityComparer.Instance);
        var stack = new Stack<(PdfDictionary Field, PdfName? InheritedFt)>();

        for (int i = 0; i < fields.Size(); i++)
        {
            PdfDictionary? field = fields.GetAsDictionary(i);
            if (field is not null && visited.Add(field))
            {
                stack.Push((field, null));
            }
        }

        while (stack.Count > 0)
        {
            (PdfDictionary field, PdfName? inheritedFt) = stack.Pop();
            PdfName? effectiveFt = field.GetAsName(PdfName.FT) ?? inheritedFt;

            if (PdfName.Sig.Equals(effectiveFt) && field.ContainsKey(PdfName.V))
            {
                return true;
            }

            PdfArray? kids = field.GetAsArray(PdfName.Kids);
            if (kids is not null)
            {
                for (int i = 0; i < kids.Size(); i++)
                {
                    PdfDictionary? kid = kids.GetAsDictionary(i);
                    if (kid is not null && visited.Add(kid))
                    {
                        stack.Push((kid, effectiveFt));
                    }
                }
            }
        }

        return false;
    }
}
