// ------------------------------------------------------------------------------
// © SDK GmbH 
// ------------------------------------------------------------------------------
using System.Diagnostics;

namespace ClearPDFAttrib.TestRunner;

internal static class ClearPdfAttribRunner
{
    public static int Run(string targetDir, int threads, string? explicitExePath)
    {
        ProcessStartInfo psi = BuildStartInfo(targetDir, threads, explicitExePath);

        using var process = Process.Start(psi)
            ?? throw new InvalidOperationException("Failed to start ClearPDFAttrib process.");

        process.WaitForExit();
        return process.ExitCode;
    }

    private static ProcessStartInfo BuildStartInfo(string targetDir, int threads, string? explicitExePath)
    {
        var psi = new ProcessStartInfo { UseShellExecute = false };

        if (!string.IsNullOrWhiteSpace(explicitExePath))
        {
            if (explicitExePath.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
            {
                psi.FileName = "dotnet";
                psi.ArgumentList.Add(explicitExePath);
            }
            else
            {
                psi.FileName = explicitExePath;
            }

            AddTargetArgs(psi, targetDir, threads);
            return psi;
        }

        // Auto-discover sibling ClearPDFAttrib binary or .csproj
        string? repoRoot = FindClearPdfAttribRoot();
        if (repoRoot is null)
        {
            throw new FileNotFoundException(
                "Could not locate ClearPDFAttrib project/executable automatically. Pass --exe <path>.");
        }

        string? compiledDll = FindCompiledDll(repoRoot);
        if (compiledDll is not null)
        {
            psi.FileName = "dotnet";
            psi.ArgumentList.Add(compiledDll);
            AddTargetArgs(psi, targetDir, threads);
            return psi;
        }
        string? csproj = Directory.EnumerateFiles(repoRoot, "*.csproj").FirstOrDefault();
        if (csproj is null)
            throw new FileNotFoundException($"No .csproj found in {repoRoot}. Pass --exe <path>.");

        psi.FileName = "dotnet";
        psi.ArgumentList.Add("run");
        psi.ArgumentList.Add("--project");
        psi.ArgumentList.Add(csproj);
        psi.ArgumentList.Add("-c");
        psi.ArgumentList.Add("Release");
        psi.ArgumentList.Add("--");
        AddTargetArgs(psi, targetDir, threads);
        return psi;
    }

    private static void AddTargetArgs(ProcessStartInfo psi, string targetDir, int threads)
    {
        psi.ArgumentList.Add(targetDir);
        psi.ArgumentList.Add("--threads");
        psi.ArgumentList.Add(threads.ToString());
    }

    private static string? FindClearPdfAttribRoot()
    {
        string[] folderNames = ["ClearPDFAttrib", "ClearPdfAttr"];

        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        for (int depth = 0; depth < 8 && dir is not null; depth++, dir = dir.Parent)
        {
            foreach (string name in folderNames)
            {
                string candidate = Path.Combine(dir.FullName, name);
                if (Directory.Exists(candidate) &&
                    Directory.EnumerateFiles(candidate, "*.csproj").Any())
                {
                    return candidate;
                }
            }
        }
        return null;
    }

    private static string? FindCompiledDll(string projectDir)
    {
        string[] dllNames = ["ClearPDFAttrib.dll", "ClearPdfAttr.dll"];

        foreach (string config in new[] { "Release", "Debug" })
        {
            string configDir = Path.Combine(projectDir, "bin", config);
            if (!Directory.Exists(configDir))
                continue;

            foreach (string dllName in dllNames)
            {
                string? dll = Directory.EnumerateFiles(configDir, dllName, SearchOption.AllDirectories)
                    .FirstOrDefault();
                if (dll is not null)
                    return dll;
            }
        }
        return null;
    }
}
