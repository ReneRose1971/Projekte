namespace SolutionBundler.Core.Implementations.BundleWriting;

/// <summary>
/// Generates URL-safe anchor links for Markdown headers.
/// </summary>
internal static class MarkdownAnchorGenerator
{
    /// <summary>
    /// Converts a file path to a valid Markdown anchor.
    /// </summary>
    /// <param name="relativePath">The relative file path.</param>
    /// <returns>A lowercase, URL-safe anchor string.</returns>
    public static string Generate(string relativePath)
    {
        return relativePath
            .Replace('\\', '/')
            .ToLowerInvariant()
            .Replace(' ', '-')
            .Replace('/', '-')
            .Replace('.', '-')
            .Replace(':', '-');
    }
}
