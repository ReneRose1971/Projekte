using SolutionBundler.Core.Abstractions;
using SolutionBundler.Core.Implementations.MetadataReading;
using SolutionBundler.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SolutionBundler.Core.Implementations;

/// <summary>
/// Liest aus .csproj-Dateien die in den ItemGroups angegebenen BuildActions und ordnet diese den gefundenen Dateien zu.
/// </summary>
public sealed class MsBuildProjectMetadataReader : IProjectMetadataReader
{
    private readonly CsprojParser _csprojParser;

    /// <summary>
    /// Initializes a new instance of the <see cref="MsBuildProjectMetadataReader"/> class.
    /// </summary>
    public MsBuildProjectMetadataReader()
    {
        _csprojParser = new CsprojParser();
    }

    /// <summary>
    /// Ergänzt die übergebene Liste von FileEntry-Objekten um die erkannten BuildActions anhand der .csproj-Inhalte.
    /// </summary>
    /// <param name="entries">Liste der Dateieinträge, die erweitert werden sollen.</param>
    /// <param name="rootPath">Root-Pfad der Solution, zur Berechnung relativer Dateipfade.</param>
    public void EnrichBuildActions(IList<FileEntry> entries, string rootPath)
    {
        var buildActionMap = BuildBuildActionMap(entries, rootPath);
        ApplyBuildActionsToEntries(entries, buildActionMap);
    }

    private IDictionary<string, BuildAction> BuildBuildActionMap(IList<FileEntry> entries, string rootPath)
    {
        var csprojPaths = entries
            .Where(f => f.RelativePath.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase))
            .Select(f => f.FullPath);

        return _csprojParser.ParseMultipleCsprojs(csprojPaths, rootPath);
    }

    private static void ApplyBuildActionsToEntries(IList<FileEntry> entries, IDictionary<string, BuildAction> buildActionMap)
    {
        foreach (var entry in entries)
        {
            if (buildActionMap.TryGetValue(entry.RelativePath, out var buildAction))
            {
                entry.Action = buildAction;
            }
            else
            {
                entry.Action = BuildActionFallbackProvider.GetBuildActionByExtension(entry.RelativePath);
            }
        }
    }
}