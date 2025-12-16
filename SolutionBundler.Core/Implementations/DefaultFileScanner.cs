using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SolutionBundler.Core.Abstractions;
using SolutionBundler.Core.Models;

namespace SolutionBundler.Core.Implementations;

/// <summary>
/// Standard-Implementierung des Datei-Scanners. Ermittelt Dateien im Projekt-Verzeichnis entsprechend den ScanSettings.
/// </summary>
public sealed class DefaultFileScanner : IFileScanner
{
    /// <summary>
    /// Scannt das Root-Verzeichnis rekursiv nach Dateien gem‰ﬂ der in <paramref name="settings"/> angegebenen Muster.
    /// </summary>
    /// <param name="rootPath">Absoluter Pfad des Projekt-Roots.</param>
    /// <param name="settings">Scan-Einstellungen mit Include/Exclude-Mustern.</param>
    /// <returns>Sortierte, eindeutige Liste der gefundenen Dateien als <see cref="FileEntry"/>-Objekte.</returns>
    public IReadOnlyList<FileEntry> Scan(string rootPath, ScanSettings settings)
    {
        var root = Path.GetFullPath(rootPath);
        var files = new List<FileEntry>();

        bool IsExcludedDir(string path) =>
            settings.ExcludeDirs.Any(d => path.Contains(Path.DirectorySeparatorChar + d + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase));

        bool IsExcludedFile(string file) =>
            settings.ExcludeGlobs.Any(glob => file.EndsWith(glob.Replace("*", ""), StringComparison.OrdinalIgnoreCase));

        foreach (var pattern in settings.IncludePatterns)
        {
            foreach (var file in Directory.EnumerateFiles(root, pattern, SearchOption.AllDirectories))
            {
                var full = Path.GetFullPath(file);
                if (IsExcludedDir(full) || IsExcludedFile(full)) continue;

                var rel = Path.GetRelativePath(root, full);
                var fi = new FileInfo(full);
                files.Add(new FileEntry
                {
                    FullPath = full,
                    RelativePath = rel.Replace('\\', '/'),
                    Size = fi.Length
                });
            }
        }

        // Distinct by relative path
        return files
            .GroupBy(f => f.RelativePath, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .OrderBy(f => f.RelativePath, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}
