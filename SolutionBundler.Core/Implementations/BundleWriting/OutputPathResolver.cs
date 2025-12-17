using SolutionBundler.Core.Models;
using System;
using System.IO;
using System.Linq;

namespace SolutionBundler.Core.Implementations.BundleWriting;

/// <summary>
/// Resolves output paths and file names for bundle generation.
/// </summary>
internal static class OutputPathResolver
{
    /// <summary>
    /// Ungültige Zeichen für Ordnernamen (plattformunabhängig).
    /// </summary>
    private static readonly char[] InvalidDirChars = Path.GetInvalidFileNameChars();

    /// <summary>
    /// Resolves the complete output path for the bundle file.
    /// </summary>
    /// <param name="settings">Scan settings containing the output file name.</param>
    /// <param name="projectName">Name of the project (fallback for file name).</param>
    /// <param name="group">Optional group name for subdirectory organization. If set, creates a subfolder.</param>
    /// <returns>Full path to the output file.</returns>
    public static string ResolveOutputPath(ScanSettings settings, string projectName, string? group = null)
    {
        var fileName = DetermineFileName(settings.OutputFileName, projectName);
        var outputDirectory = GetOutputDirectory(group);
        
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

    private static string GetOutputDirectory(string? group)
    {
        var baseDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "SolutionBundler",
            "Bundles");

        if (string.IsNullOrWhiteSpace(group))
        {
            return baseDir;
        }

        var safeGroupName = SanitizeDirectoryName(group);
        return Path.Combine(baseDir, safeGroupName);
    }

    /// <summary>
    /// Ersetzt ungültige Zeichen in Ordnernamen durch Unterstriche.
    /// </summary>
    /// <param name="directoryName">Ursprünglicher Ordnername.</param>
    /// <returns>Bereinigter Ordnername ohne ungültige Zeichen.</returns>
    private static string SanitizeDirectoryName(string directoryName)
    {
        var sanitized = new string(directoryName
            .Select(c => InvalidDirChars.Contains(c) ? '_' : c)
            .ToArray());

        sanitized = sanitized.Trim();
        
        var reserved = new[] { "CON", "PRN", "AUX", "NUL", "COM1", "COM2", "COM3", "COM4", "COM5", 
                               "COM6", "COM7", "COM8", "COM9", "LPT1", "LPT2", "LPT3", "LPT4", 
                               "LPT5", "LPT6", "LPT7", "LPT8", "LPT9" };
        
        if (reserved.Contains(sanitized, StringComparer.OrdinalIgnoreCase))
        {
            sanitized = "_" + sanitized;
        }

        return string.IsNullOrWhiteSpace(sanitized) ? "Default" : sanitized;
    }
}
