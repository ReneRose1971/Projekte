using SolutionBundler.Core.Models;

namespace SolutionBundler.Core.Implementations.BundleWriting;

/// <summary>
/// Resolves output paths and file names for bundle generation.
/// </summary>
internal static class OutputPathResolver
{
    /// <summary>
    /// Resolves the complete output path for the bundle file.
    /// </summary>
    /// <param name="settings">Scan settings containing the output file name.</param>
    /// <param name="projectName">Name of the project (fallback for file name).</param>
    /// <returns>Full path to the output file.</returns>
    public static string ResolveOutputPath(ScanSettings settings, string projectName)
    {
        var fileName = DetermineFileName(settings.OutputFileName, projectName);
        var outputDirectory = GetOutputDirectory();
        
        Directory.CreateDirectory(outputDirectory);
        
        return Path.Combine(outputDirectory, fileName);
    }

    private static string DetermineFileName(string settingsFileName, string projectName)
    {
        var fileName = string.IsNullOrWhiteSpace(settingsFileName)
            ? $"{projectName}.md"
            : settingsFileName;

        if (!fileName.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
        {
            fileName += ".md";
        }

        return fileName;
    }

    private static string GetOutputDirectory()
    {
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "SolutionBundler",
            "Bundles");
    }
}
