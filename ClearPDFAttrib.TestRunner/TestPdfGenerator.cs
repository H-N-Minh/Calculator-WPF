// ------------------------------------------------------------------------------
// © SDK GmbH 
// ------------------------------------------------------------------------------

using System.IO.Compression;
using System.Text;
using PdfSharp.Pdf;

namespace ClearPDFAttrib.TestRunner;

internal static class TestPdfGenerator
{
    internal const string PreservedSubject = "Test Subject - Keep Me";
    internal const string PreservedProducer = "SDK Test Producer";
    internal static readonly DateTime UntouchedTimestampUtc = new(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private static readonly Encoding Utf8NoBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

    public static void Generate(string rootDir, long count, int threads)
    {
        Directory.CreateDirectory(rootDir);

        // Pre-create bucket folders (1,000 files per folder, mirroring real structure)
        string baseDateDir = Path.Combine(rootDir, "Dokumente_2023", "06", "30");
        long bucketCount = (count + 999) / 1000;
        for (long b = 0; b < bucketCount; b++)
            Directory.CreateDirectory(Path.Combine(baseDateDir, b.ToString("D5")));

        // Create non-PDF decoys to verify they are untouched
        File.WriteAllText(Path.Combine(rootDir, "ignore_me.txt"), "keep-txt", Utf8NoBom);
        File.WriteAllText(Path.Combine(baseDateDir, "decoy.pdf_bak"), "keep-bak", Utf8NoBom);

        long completed = 0;
        var options = new ParallelOptions { MaxDegreeOfParallelism = threads };

        Parallel.For(0L, count, options, i =>
        {
            long bucket = i / 1000;
            string filePath = Path.Combine(baseDateDir, bucket.ToString("D5"), $"{i:D13}.pdf");

            CreateTestPdf(filePath, i);

            long done = Interlocked.Increment(ref completed);
            if (count >= 10_000 && done % 50_000 == 0)
                Console.WriteLine($"      Generated {done:N0} / {count:N0}...");
        });
    }

    private static void CreateTestPdf(string filePath, long index)
    {
        int scenario = (int)(index % 4);
        bool includeInfoTarget = scenario is 0 or 2;
        bool includeXmpTarget = scenario is 1 or 2;
        bool compressXmp = (index % 2) == 1;

        using var doc = new PdfDocument();
        doc.AddPage();

        // Unrelated Info property that must NOT be cleared
        doc.Info.Subject = PreservedSubject;

        if (includeInfoTarget)
        {
            doc.Info.Title = $"Test Title {index}";
            doc.Info.Author = $"Test Author {index}";
            doc.Info.Creator = $"Test Creator {index}";
        }

        // Attach XMP for scenarios 1, 2 (with target attrs) and 3 (clean XMP without target attrs)
        if (scenario is 1 or 2 or 3)
        {
            AttachXmpMetadata(doc, index, includeXmpTarget, compressXmp);
        }

        doc.Save(filePath);

        // Mark scenario 3 with an old timestamp to verify ClearPDFAttrib does not rewrite clean files
        if (scenario == 3)
            File.SetLastWriteTimeUtc(filePath, UntouchedTimestampUtc);
    }

    private static void AttachXmpMetadata(PdfDocument doc, long index, bool includeTargets, bool useAttributeAndFlateForm)
    {
        string xml = (includeTargets, useAttributeAndFlateForm) switch
        {
            (true, false) => $"""
                <?xpacket begin="﻿" id="W5M0MpCehiHzreSzNTczkc9d"?>
                <x:xmpmeta xmlns:x="adobe:ns:meta/">
                  <rdf:RDF xmlns:rdf="http://www.w3.org/1999/02/22-rdf-syntax-ns#">
                    <rdf:Description rdf:about=""
                        xmlns:dc="http://purl.org/dc/elements/1.1/"
                        xmlns:xmp="http://ns.adobe.com/xap/1.0/"
                        xmlns:pdf="http://ns.adobe.com/pdf/1.3/">
                      <dc:format>application/pdf</dc:format>
                      <dc:title><rdf:Alt><rdf:li xml:lang="x-default">XMP Title {index}</rdf:li></rdf:Alt></dc:title>
                      <dc:creator><rdf:Seq><rdf:li>XMP Author {index}</rdf:li></rdf:Seq></dc:creator>
                      <xmp:CreatorTool>XMP CreatorTool {index}</xmp:CreatorTool>
                      <pdf:Producer>{PreservedProducer}</pdf:Producer>
                    </rdf:Description>
                  </rdf:RDF>
                </x:xmpmeta>
                <?xpacket end="w"?>
                """,

            (true, true) => $"""
                <?xpacket begin="﻿" id="W5M0MpCehiHzreSzNTczkc9d"?>
                <x:xmpmeta xmlns:x="adobe:ns:meta/">
                  <rdf:RDF xmlns:rdf="http://www.w3.org/1999/02/22-rdf-syntax-ns#">
                    <rdf:Description rdf:about=""
                        xmlns:dc="http://purl.org/dc/elements/1.1/"
                        xmlns:xap="http://ns.adobe.com/xap/1.0/"
                        xmlns:pdf="http://ns.adobe.com/pdf/1.3/"
                        dc:format="application/pdf"
                        dc:title="Attr Title {index}"
                        dc:creator="Attr Author {index}"
                        xap:CreatorTool="Legacy Xap Creator {index}"
                        pdf:Producer="{PreservedProducer}" />
                  </rdf:RDF>
                </x:xmpmeta>
                <?xpacket end="w"?>
                """,

            (false, _) => $"""
                <?xpacket begin="﻿" id="W5M0MpCehiHzreSzNTczkc9d"?>
                <x:xmpmeta xmlns:x="adobe:ns:meta/">
                  <rdf:RDF xmlns:rdf="http://www.w3.org/1999/02/22-rdf-syntax-ns#">
                    <rdf:Description rdf:about=""
                        xmlns:dc="http://purl.org/dc/elements/1.1/"
                        xmlns:pdf="http://ns.adobe.com/pdf/1.3/">
                      <dc:format>application/pdf</dc:format>
                      <pdf:Producer>{PreservedProducer}</pdf:Producer>
                    </rdf:Description>
                  </rdf:RDF>
                </x:xmpmeta>
                <?xpacket end="w"?>
                """
        };

        byte[] rawBytes = Utf8NoBom.GetBytes(xml);
        byte[] streamBytes = rawBytes;

        if (useAttributeAndFlateForm)
        {
            using var ms = new MemoryStream();
            using (var zlib = new ZLibStream(ms, CompressionLevel.Fastest, leaveOpen: true))
                zlib.Write(rawBytes);
            streamBytes = ms.ToArray();
        }

        var metadataDict = new PdfDictionary(doc);
        metadataDict.Elements["/Type"] = new PdfName("/Metadata");
        metadataDict.Elements["/Subtype"] = new PdfName("/XML");
        metadataDict.CreateStream(streamBytes);

        if (useAttributeAndFlateForm)
            metadataDict.Elements["/Filter"] = new PdfName("/FlateDecode");

        doc.Internals.AddObject(metadataDict);
        doc.Internals.Catalog.Elements["/Metadata"] = metadataDict.Reference;
    }
}
