using System.IO;

namespace SolutionBundler.Core.Implementations.MetadataReading;

/// <summary>
/// Extension methods for path normalization.
/// </summary>
internal static class PathNormalizer
{
    /// <summary>
    /// Normalizes a path to use forward slashes and be relative to a root path.
    /// </summary>
    /// <param name="absolutePath">The absolute path to normalize.</param>
    /// <param name="rootPath">The root path for relative calculation.</param>
    /// <returns>A normalized relative path with forward slashes.</returns>
    public static string NormalizeRelativePath(string absolutePath, string rootPath)
    {
        var relativePath = Path.GetRelativePath(rootPath, absolutePath);
        return relativePath.Replace('\\', '/');
    }
}
