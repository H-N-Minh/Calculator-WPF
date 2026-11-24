// ------------------------------------------------------------------------------
// © SDK GmbH 
// ------------------------------------------------------------------------------

using System.Runtime.CompilerServices;
using iText.Kernel.Exceptions;
using iText.Kernel.XMP;

namespace ClearPdfAttrib;

public sealed record ProcessingOptions(
    string RootDirectory,
    int MaxParallelism = 4,
    bool DryRun = false,
    bool SkipSigned = false,
    bool IgnoreInaccessibleDirectories = false,
    int ProgressInterval = 10_000,
    int MaxRetries = 3,
    string? ErrorLogPath = null,
    bool Quiet = false);

public sealed class FileProcessor(ProcessingOptions options, ProcessingStatistics stats)
{
    private static readonly EnumerationOptions SingleDirectoryOptions = new()
    {
        RecurseSubdirectories = false,
        IgnoreInaccessible = false,
        AttributesToSkip = FileAttributes.ReparsePoint,
        MatchCasing = MatchCasing.CaseInsensitive
    };

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        IAsyncEnumerable<string> pdfFiles = EnumeratePdfFilesAsync(options.RootDirectory, cancellationToken);

        var parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = options.MaxParallelism,
            CancellationToken = cancellationToken
        };

        await Parallel.ForEachAsync(
            pdfFiles,
            parallelOptions,
            async (file, ct) => await ProcessFileAsync(file, ct));
    }

    private async IAsyncEnumerable<string> EnumeratePdfFilesAsync(
        string rootDirectory,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var pendingDirectories = new Stack<string>();
        pendingDirectories.Push(rootDirectory);

        while (pendingDirectories.Count > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();
            string currentDir = pendingDirectories.Pop();

            // Enumerate subdirectories for this directory.
            IEnumerator<string>? dirEnumerator = await TryOpenEnumeratorWithRetryAsync(
                () => Directory.EnumerateDirectories(currentDir, "*", SingleDirectoryOptions).GetEnumerator(),
                currentDir,
                cancellationToken);

            if (dirEnumerator is not null)
            {
                using (dirEnumerator)
                {
                    while (true)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        (bool moved, Exception? enumEx) = TryMoveNext(dirEnumerator);

                        if (enumEx is not null)
                        {
                            HandleDirectoryException(currentDir, enumEx);
                            break;
                        }

                        if (!moved)
                        {
                            break;
                        }

                        pendingDirectories.Push(dirEnumerator.Current);
                    }
                }
            }

            // Lazily stream *.pdf files in this directory without buffering all paths globally.
            IEnumerator<string>? fileEnumerator = await TryOpenEnumeratorWithRetryAsync(
                () => Directory.EnumerateFiles(currentDir, "*.pdf", SingleDirectoryOptions).GetEnumerator(),
                currentDir,
                cancellationToken);

            if (fileEnumerator is not null)
            {
                using (fileEnumerator)
                {
                    while (true)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        (bool moved, Exception? enumEx) = TryMoveNext(fileEnumerator);

                        if (enumEx is not null)
                        {
                            HandleDirectoryException(currentDir, enumEx);
                            break;
                        }

                        if (!moved)
                        {
                            break;
                        }

                        string filePath = fileEnumerator.Current;
                        if (filePath.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                        {
                            yield return filePath;
                        }
                    }
                }
            }
        }
    }

    private async Task<IEnumerator<string>?> TryOpenEnumeratorWithRetryAsync(
        Func<IEnumerator<string>> factory,
        string directoryPath,
        CancellationToken cancellationToken)
    {
        for (int attempt = 0; ; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                return factory();
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex) when (attempt < options.MaxRetries && IsTransientIoException(ex))
            {
                stats.RecordRetry();
                await Task.Delay(ComputeBackoff(attempt), cancellationToken);
            }
            catch (Exception ex)
            {
                HandleDirectoryException(directoryPath, ex);
                return null;
            }
        }
    }

    private void HandleDirectoryException(string directoryPath, Exception ex)
    {
        if (options.IgnoreInaccessibleDirectories && ex is UnauthorizedAccessException)
        {
            return;
        }

        stats.RecordDirectoryError(directoryPath, ex);
    }

    private static (bool Moved, Exception? Error) TryMoveNext(IEnumerator<string> enumerator)
    {
        try
        {
            return (enumerator.MoveNext(), null);
        }
        catch (Exception ex)
        {
            return (false, ex);
        }
    }

    private async ValueTask ProcessFileAsync(string sourcePath, CancellationToken cancellationToken)
    {
        for (int attempt = 0; ; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                PdfInspectionStatus status = PdfMetadataCleaner.Inspect(sourcePath, options.SkipSigned);

                switch (status)
                {
                    case PdfInspectionStatus.CleanAlready:
                        stats.RecordSuccess(PdfCleanOutcome.SkippedClean);
                        return;

                    case PdfInspectionStatus.SkippedSigned:
                        stats.RecordSuccess(PdfCleanOutcome.SkippedSigned);
                        return;

                    case PdfInspectionStatus.NeedsCleaning:
                        if (options.DryRun)
                        {
                            stats.RecordSuccess(PdfCleanOutcome.Modified);
                            return;
                        }

                        RewriteAndReplaceFile(sourcePath);
                        stats.RecordSuccess(PdfCleanOutcome.Modified);
                        return;
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex) when (attempt < options.MaxRetries && IsTransientIoException(ex))
            {
                stats.RecordRetry();
                await Task.Delay(ComputeBackoff(attempt), cancellationToken);
            }
            catch (Exception ex)
            {
                stats.RecordFileFailure(sourcePath, ex);
                return;
            }
        }
    }

    private static void RewriteAndReplaceFile(string sourcePath)
    {
        if ((File.GetAttributes(sourcePath) & FileAttributes.ReadOnly) != 0)
        {
            throw new UnauthorizedAccessException($"File '{sourcePath}' has the ReadOnly attribute set.");
        }

        string? directory = Path.GetDirectoryName(sourcePath);
        if (string.IsNullOrEmpty(directory))
        {
            throw new InvalidOperationException($"Cannot determine directory for '{sourcePath}'.");
        }

        // Keep temp filename short and in the same directory for atomic replacement on the same filesystem.
        string tempFile = Path.Combine(directory, $".clearpdf-{Guid.NewGuid():N}.tmp");

        try
        {
            PdfMetadataCleaner.ClearMetadata(sourcePath, tempFile);

            var tempInfo = new FileInfo(tempFile);
            if (!tempInfo.Exists || tempInfo.Length == 0)
            {
                throw new InvalidDataException($"Temporary output PDF '{tempFile}' is missing or empty.");
            }

            ReplaceWithBriefSharingRetry(tempFile, sourcePath);
        }
        finally
        {
            TryDeleteTempFile(tempFile);
        }
    }

    private static void ReplaceWithBriefSharingRetry(string tempFile, string targetFile)
    {
        const int maxLocalAttempts = 3;

        for (int attempt = 0; ; attempt++)
        {
            try
            {
                File.Replace(
                    sourceFileName: tempFile,
                    destinationFileName: targetFile,
                    destinationBackupFileName: null,
                    ignoreMetadataErrors: true);
                return;
            }
            catch (IOException ex) when (attempt < maxLocalAttempts && IsSharingViolation(ex))
            {
                // Brief pause for endpoint AV scanners or SMB oplocks releasing the newly closed temp file.
                Thread.Sleep(150 * (attempt + 1));
            }
        }
    }

    private static void TryDeleteTempFile(string tempFile)
    {
        try
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
        catch
        {
            // Best-effort cleanup so finally never masks the primary exception.
        }
    }

    private static bool IsSharingViolation(IOException ex)
    {
        const int errorSharingViolation = unchecked((int)0x80070020);
        const int errorLockViolation = unchecked((int)0x80070021);
        return ex.HResult == errorSharingViolation || ex.HResult == errorLockViolation;
    }

    private static TimeSpan ComputeBackoff(int attempt) =>
        TimeSpan.FromMilliseconds(250 * (1 << attempt) + Random.Shared.Next(0, 150));

    private static bool IsTransientIoException(Exception ex)
    {
        for (Exception? current = ex; current is not null; current = current.InnerException)
        {
            if (current is BadPasswordException or XMPException)
            {
                return false;
            }

            if (current is FileNotFoundException
                or DirectoryNotFoundException
                or PathTooLongException
                or EndOfStreamException)
            {
                return false;
            }

            if (current is System.IO.IOException)
            {
                return true;
            }
        }

        if (ex is AggregateException agg)
        {
            foreach (Exception inner in agg.InnerExceptions)
            {
                if (IsTransientIoException(inner))
                {
                    return true;
                }
            }
        }

        return false;
    }
}
