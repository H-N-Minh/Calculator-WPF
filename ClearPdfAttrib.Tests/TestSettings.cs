// ------------------------------------------------------------------------------
// © SDK GmbH 
// ------------------------------------------------------------------------------
[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace ClearPdfAttrib.Tests;

internal static class TestSettings
{
    /// <summary>
    /// Total PDFs created in the cached sample folder.
    /// 13 are fixed scenario fixtures (01_Metadata=4, 02_Traversal=6, 03_ErrorHandling=3);
    /// the rest are generated in 04_Bulk.
    /// Increase this (e.g. 5_000 or 20_000) to test scale and watch live progress.
    /// </summary>
    public const int TotalPdfCount = 6_000;

    public const int FixedFixturePdfCount = 13;

    public static int BulkPdfCount
    {
        get
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(TotalPdfCount, FixedFixturePdfCount + 1);
            return TotalPdfCount - FixedFixturePdfCount;
        }
    }

    public static string SampleCacheDirectory =>
        Path.Combine(Path.GetTempPath(), "ClearPdfAttrib.SampleCache");

    /// <summary>
    /// Prints roughly ~10 live progress updates regardless of batch size.
    /// </summary>
    public static int GetProgressInterval(int pdfCount) =>
        Math.Clamp(pdfCount / 10, 1, 2_000);
}
