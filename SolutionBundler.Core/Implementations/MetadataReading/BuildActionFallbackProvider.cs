using SolutionBundler.Core.Models;
using System.IO;

namespace SolutionBundler.Core.Implementations.MetadataReading;

/// <summary>
/// Provides fallback BuildAction determination based on file extensions.
/// </summary>
internal static class BuildActionFallbackProvider
{
    /// <summary>
    /// Determines the BuildAction for a file based on its extension.
    /// </summary>
    /// <param name="filePath">The file path to analyze.</param>
    /// <returns>The inferred BuildAction, or BuildAction.Unknown if no rule matches.</returns>
    public static BuildAction GetBuildActionByExtension(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();

        return extension switch
        {
            ".cs" => BuildAction.Compile,
            ".xaml" => BuildAction.Page,
            ".resx" => BuildAction.Resource,
            ".json" => BuildAction.Content,
            ".config" => BuildAction.Content,
            ".props" => BuildAction.Content,
            ".targets" => BuildAction.Content,
            _ => BuildAction.Unknown
        };
    }
}
