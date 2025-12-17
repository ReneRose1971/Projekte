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
    /// ‹berspringt automatisch Verzeichnisse, auf die kein Zugriff besteht (z.B. System-Ordner) oder die nicht existieren (z.B. defekte Symlinks).
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

        void ScanDirectory(string directory)
        {
            // ‹berspringe ausgeschlossene Verzeichnisse
            if (IsExcludedDir(directory))
                return;

            try
            {
                // Scanne Dateien in diesem Verzeichnis
                foreach (var pattern in settings.IncludePatterns)
                {
                    try
                    {
                        foreach (var file in Directory.EnumerateFiles(directory, pattern))
                        {
                            try
                            {
                                var full = Path.GetFullPath(file);
                                if (IsExcludedFile(full)) continue;

                                var rel = Path.GetRelativePath(root, full);
                                var fi = new FileInfo(full);
                                files.Add(new FileEntry
                                {
                                    FullPath = full,
                                    RelativePath = rel.Replace('\\', '/'),
                                    Size = fi.Length
                                });
                            }
                            catch (UnauthorizedAccessException)
                            {
                                // ‹berspringe Dateien, auf die kein Zugriff besteht
                            }
                            catch (IOException)
                            {
                                // ‹berspringe Dateien mit I/O-Problemen (z.B. locked files)
                            }
                        }
                    }
                    catch (UnauthorizedAccessException)
                    {
                        // ‹berspringe, wenn keine Berechtigung f¸r dieses Pattern
                    }
                    catch (DirectoryNotFoundException)
                    {
                        // ‹berspringe, wenn Verzeichnis nicht mehr existiert (z.B. gelˆscht w‰hrend Scan)
                    }
                    catch (IOException)
                    {
                        // ‹berspringe bei I/O-Fehlern
                    }
                }

                // Rekursiv in Unterverzeichnisse
                try
                {
                    foreach (var subDir in Directory.EnumerateDirectories(directory))
                    {
                        ScanDirectory(subDir);
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    // ‹berspringe Unterverzeichnisse, auf die kein Zugriff besteht
                }
                catch (DirectoryNotFoundException)
                {
                    // ‹berspringe Unterverzeichnisse, die nicht existieren (z.B. defekte Symlinks)
                }
                catch (IOException)
                {
                    // ‹berspringe bei I/O-Fehlern
                }
            }
            catch (UnauthorizedAccessException)
            {
                // ‹berspringe das gesamte Verzeichnis, wenn kein Zugriff besteht
            }
            catch (DirectoryNotFoundException)
            {
                // ‹berspringe das gesamte Verzeichnis, wenn es nicht existiert
            }
            catch (IOException)
            {
                // ‹berspringe bei I/O-Fehlern
            }
        }

        // Starte rekursive Traversierung
        ScanDirectory(root);

        // Distinct by relative path
        return files
            .GroupBy(f => f.RelativePath, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .OrderBy(f => f.RelativePath, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}
