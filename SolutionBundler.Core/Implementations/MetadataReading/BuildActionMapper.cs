using SolutionBundler.Core.Models;

namespace SolutionBundler.Core.Implementations.MetadataReading;

/// <summary>
/// Maps MSBuild ItemGroup element names to BuildAction enum values.
/// </summary>
internal static class BuildActionMapper
{
    /// <summary>
    /// Maps an MSBuild element name to a corresponding BuildAction.
    /// </summary>
    /// <param name="elementName">The local name of the ItemGroup element (e.g., "Compile", "Page").</param>
    /// <returns>The corresponding BuildAction, or BuildAction.Unknown if not recognized.</returns>
    public static BuildAction MapElementToBuildAction(string elementName)
    {
        return elementName switch
        {
            "Compile" => BuildAction.Compile,
            "Page" => BuildAction.Page,
            "Resource" => BuildAction.Resource,
            "Content" => BuildAction.Content,
            "None" => BuildAction.None,
            _ => BuildAction.Unknown
        };
    }
}
