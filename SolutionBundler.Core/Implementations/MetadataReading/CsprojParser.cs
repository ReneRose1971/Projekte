using System.Xml.Linq;
using SolutionBundler.Core.Models;

namespace SolutionBundler.Core.Implementations.MetadataReading;

/// <summary>
/// Parses .csproj files and extracts file-to-BuildAction mappings.
/// </summary>
internal sealed class CsprojParser
{
    /// <summary>
    /// Parses a .csproj file and returns a dictionary mapping relative file paths to BuildActions.
    /// </summary>
    /// <param name="csprojPath">Full path to the .csproj file.</param>
    /// <param name="rootPath">Root path of the solution for relative path calculation.</param>
    /// <returns>Dictionary mapping normalized relative paths to BuildActions.</returns>
    public IDictionary<string, BuildAction> ParseCsproj(string csprojPath, string rootPath)
    {
        var map = new Dictionary<string, BuildAction>(StringComparer.OrdinalIgnoreCase);

        try
        {
            var doc = XDocument.Load(csprojPath);
            var projectDirectory = Path.GetDirectoryName(csprojPath)!;

            foreach (var itemGroup in doc.Descendants("ItemGroup"))
            {
                foreach (var item in itemGroup.Elements())
                {
                    var includeValue = item.Attribute("Include")?.Value;
                    
                    if (string.IsNullOrWhiteSpace(includeValue))
                        continue;

                    var buildAction = BuildActionMapper.MapElementToBuildAction(item.Name.LocalName);
                    var absolutePath = Path.GetFullPath(Path.Combine(projectDirectory, includeValue));
                    var relativePath = PathNormalizer.NormalizeRelativePath(absolutePath, rootPath);

                    // First entry wins if there are duplicates
                    if (!map.ContainsKey(relativePath))
                    {
                        map[relativePath] = buildAction;
                    }
                }
            }
        }
        catch
        {
            // Robust: continue on parsing errors
        }

        return map;
    }

    /// <summary>
    /// Parses multiple .csproj files and merges their BuildAction mappings.
    /// </summary>
    /// <param name="csprojPaths">Collection of full paths to .csproj files.</param>
    /// <param name="rootPath">Root path of the solution for relative path calculation.</param>
    /// <returns>Merged dictionary mapping normalized relative paths to BuildActions.</returns>
    public IDictionary<string, BuildAction> ParseMultipleCsprojs(IEnumerable<string> csprojPaths, string rootPath)
    {
        var mergedMap = new Dictionary<string, BuildAction>(StringComparer.OrdinalIgnoreCase);

        foreach (var csprojPath in csprojPaths)
        {
            var projectMap = ParseCsproj(csprojPath, rootPath);

            foreach (var (path, action) in projectMap)
            {
                // First entry wins across all projects
                if (!mergedMap.ContainsKey(path))
                {
                    mergedMap[path] = action;
                }
            }
        }

        return mergedMap;
    }
}
