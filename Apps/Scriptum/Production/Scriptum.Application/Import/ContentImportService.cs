using System.Text.Json;
using System.Text.RegularExpressions;
using DataToolKit.Abstractions.DataStores;
using DataToolKit.Storage.DataStores;
using DataToolKit.Storage.Repositories;
using Scriptum.Application.Import.Dtos;
using Scriptum.Content.Data;

namespace Scriptum.Application.Import;

/// <summary>
/// Service für den Import von Content-Daten aus externen JSON-Dateien.
/// </summary>
public sealed class ContentImportService : IContentImportService
{
    private readonly IDataStoreProvider _dataStoreProvider;
    private readonly IRepositoryFactory _repositoryFactory;

    private PersistentDataStore<ModuleData>? _moduleStore;
    private PersistentDataStore<LessonData>? _lessonStore;
    private PersistentDataStore<LessonGuideData>? _guideStore;

    /// <summary>
    /// Erstellt eine neue Instanz des ContentImportService.
    /// </summary>
    /// <param name="dataStoreProvider">Provider für DataStores.</param>
    /// <param name="repositoryFactory">Factory für Repositories.</param>
    public ContentImportService(
        IDataStoreProvider dataStoreProvider,
        IRepositoryFactory repositoryFactory)
    {
        _dataStoreProvider = dataStoreProvider ?? throw new ArgumentNullException(nameof(dataStoreProvider));
        _repositoryFactory = repositoryFactory ?? throw new ArgumentNullException(nameof(repositoryFactory));
    }

    /// <inheritdoc />
    public async Task<ContentImportResult> ImportAsync(
        ContentImportRequest request,
        CancellationToken cancellationToken = default)
    {
        var warnings = new List<string>();

        try
        {
            EnsureDataStoresLoaded();

            if (!request.OverwriteExisting && HasExistingContent())
            {
                return new ContentImportResult
                {
                    Success = false,
                    ErrorMessage = "Es sind bereits Inhalte vorhanden. Aktivieren Sie 'Vorhandenen Content überschreiben', um fortzufahren."
                };
            }

            var moduleImports = await LoadModuleImportsAsync(request.ModulesImportJsonPath, cancellationToken);
            var lessonImports = await LoadLessonImportsAsync(request.LessonsImportJsonPath, cancellationToken);
            var guideImports = await LoadGuideImportsAsync(request.GuidesImportJsonPath, cancellationToken);

            var modules = MapModules(moduleImports);
            var (lessons, lessonWarnings) = MapLessons(lessonImports, modules);
            warnings.AddRange(lessonWarnings);

            var (guides, guideWarnings) = MapGuides(guideImports, lessons);
            warnings.AddRange(guideWarnings);

            var validationWarnings = ValidateContent(modules, lessons);
            warnings.AddRange(validationWarnings);

            if (request.OverwriteExisting)
            {
                ClearExistingContent();
            }

            WriteContent(modules, lessons, guides);

            var outputPath = GetOutputFolderPath();

            return new ContentImportResult
            {
                Success = true,
                ModulesImported = modules.Count,
                LessonsImported = lessons.Count,
                GuidesImported = guides.Count,
                Warnings = warnings,
                OutputFolderPath = outputPath
            };
        }
        catch (FileNotFoundException ex)
        {
            return new ContentImportResult
            {
                Success = false,
                ErrorMessage = $"Datei nicht gefunden: {ex.Message}"
            };
        }
        catch (JsonException ex)
        {
            return new ContentImportResult
            {
                Success = false,
                ErrorMessage = $"Ungültiges JSON-Format: {ex.Message}"
            };
        }
        catch (Exception ex)
        {
            return new ContentImportResult
            {
                Success = false,
                ErrorMessage = $"Import fehlgeschlagen: {ex.Message}"
            };
        }
    }

    private void EnsureDataStoresLoaded()
    {
        if (_moduleStore == null)
        {
            _moduleStore = _dataStoreProvider.GetPersistent<ModuleData>(
                _repositoryFactory,
                isSingleton: true,
                trackPropertyChanges: false,
                autoLoad: true);
        }

        if (_lessonStore == null)
        {
            _lessonStore = _dataStoreProvider.GetPersistent<LessonData>(
                _repositoryFactory,
                isSingleton: true,
                trackPropertyChanges: false,
                autoLoad: true);
        }

        if (_guideStore == null)
        {
            _guideStore = _dataStoreProvider.GetPersistent<LessonGuideData>(
                _repositoryFactory,
                isSingleton: true,
                trackPropertyChanges: false,
                autoLoad: true);
        }
    }

    private bool HasExistingContent()
    {
        return _moduleStore!.Items.Any() || _lessonStore!.Items.Any() || _guideStore!.Items.Any();
    }

    private static async Task<List<ModuleImportDto>> LoadModuleImportsAsync(
        string path,
        CancellationToken cancellationToken)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException($"Modul-Datei nicht gefunden: {path}");

        var json = await File.ReadAllTextAsync(path, cancellationToken);
        return JsonSerializer.Deserialize<List<ModuleImportDto>>(json) ?? new List<ModuleImportDto>();
    }

    private static async Task<List<LessonImportDto>> LoadLessonImportsAsync(
        string path,
        CancellationToken cancellationToken)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException($"Lektions-Datei nicht gefunden: {path}");

        var json = await File.ReadAllTextAsync(path, cancellationToken);
        return JsonSerializer.Deserialize<List<LessonImportDto>>(json) ?? new List<LessonImportDto>();
    }

    private static async Task<List<GuideImportDto>> LoadGuideImportsAsync(
        string path,
        CancellationToken cancellationToken)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException($"Guide-Datei nicht gefunden: {path}");

        var json = await File.ReadAllTextAsync(path, cancellationToken);
        return JsonSerializer.Deserialize<List<GuideImportDto>>(json) ?? new List<GuideImportDto>();
    }

    private static List<ModuleData> MapModules(List<ModuleImportDto> imports)
    {
        var modules = new List<ModuleData>();

        foreach (var import in imports)
        {
            var order = ExtractOrderFromModuleId(import.ModuleId);
            var module = new ModuleData(
                import.ModuleId,
                import.Titel,
                import.Beschreibung,
                order);

            modules.Add(module);
        }

        return modules;
    }

    private static int ExtractOrderFromModuleId(string moduleId)
    {
        var match = Regex.Match(moduleId, @"\d+");
        if (match.Success && int.TryParse(match.Value, out var order))
        {
            return order;
        }
        return 0;
    }

    private static (List<LessonData>, List<string>) MapLessons(
        List<LessonImportDto> imports,
        List<ModuleData> modules)
    {
        var lessons = new List<LessonData>();
        var warnings = new List<string>();
        var usedLessonIds = new HashSet<string>();

        foreach (var import in imports)
        {
            if (!modules.Any(m => m.ModuleId == import.ModuleId))
            {
                warnings.Add($"Lektion '{import.Titel}' referenziert unbekanntes Modul '{import.ModuleId}'");
                continue;
            }

            var lessonId = import.LessonId;
            if (string.IsNullOrWhiteSpace(lessonId))
            {
                lessonId = GenerateUniqueLessonId(import.ModuleId, import.Titel, usedLessonIds);
            }

            if (usedLessonIds.Contains(lessonId))
            {
                warnings.Add($"Doppelte LessonId '{lessonId}' gefunden. Lektion '{import.Titel}' wird übersprungen.");
                continue;
            }

            usedLessonIds.Add(lessonId);

            var normalizedText = NormalizeLineBreaks(import.Uebungstext);

            var lesson = new LessonData(
                lessonId,
                import.ModuleId,
                import.Titel,
                import.Beschreibung,
                import.Schwierigkeit,
                import.Tags,
                normalizedText);

            lessons.Add(lesson);
        }

        return (lessons, warnings);
    }

    private static string GenerateUniqueLessonId(string moduleId, string titel, HashSet<string> usedIds)
    {
        var baseId = $"{moduleId}_{SanitizeForId(titel)}";
        var lessonId = baseId;
        var counter = 1;

        while (usedIds.Contains(lessonId))
        {
            lessonId = $"{baseId}_{counter}";
            counter++;
        }

        return lessonId;
    }

    private static string SanitizeForId(string input)
    {
        var sanitized = Regex.Replace(input, @"[^a-zA-Z0-9]", "_");
        return sanitized.ToLower();
    }

    private static string NormalizeLineBreaks(string text)
    {
        return text
            .Replace("\r\n", "\n")
            .Replace("\r", "\n");
    }

    private static (List<LessonGuideData>, List<string>) MapGuides(
        List<GuideImportDto> imports,
        List<LessonData> lessons)
    {
        var guides = new List<LessonGuideData>();
        var warnings = new List<string>();

        var lessonsByTitle = lessons.ToDictionary(l => l.Titel, l => l.LessonId, StringComparer.OrdinalIgnoreCase);

        foreach (var import in imports)
        {
            string? lessonId = import.LessonId;

            if (string.IsNullOrWhiteSpace(lessonId) && !string.IsNullOrWhiteSpace(import.LessonTitel))
            {
                if (lessonsByTitle.TryGetValue(import.LessonTitel, out var mappedId))
                {
                    lessonId = mappedId;
                }
                else
                {
                    warnings.Add($"Guide für Lektion '{import.LessonTitel}' konnte nicht zugeordnet werden");
                    continue;
                }
            }

            if (string.IsNullOrWhiteSpace(lessonId))
            {
                warnings.Add("Guide ohne LessonId und ohne LessonTitel wird übersprungen");
                continue;
            }

            if (!lessons.Any(l => l.LessonId == lessonId))
            {
                warnings.Add($"Guide referenziert unbekannte LessonId '{lessonId}'");
                continue;
            }

            var guide = new LessonGuideData(lessonId, import.GuideTextMarkdown);
            guides.Add(guide);
        }

        return (guides, warnings);
    }

    private static List<string> ValidateContent(List<ModuleData> modules, List<LessonData> lessons)
    {
        var warnings = new List<string>();

        var moduleIds = modules.Select(m => m.ModuleId).ToHashSet();
        var lessonIds = lessons.Select(l => l.LessonId).ToHashSet();

        foreach (var lesson in lessons)
        {
            if (!moduleIds.Contains(lesson.ModuleId))
            {
                warnings.Add($"Lektion '{lesson.LessonId}' referenziert fehlendes Modul '{lesson.ModuleId}'");
            }
        }

        if (lessonIds.Count != lessons.Count)
        {
            warnings.Add("Es wurden doppelte LessonIds gefunden");
        }

        return warnings;
    }

    private void ClearExistingContent()
    {
        _moduleStore!.Clear();
        _lessonStore!.Clear();
        _guideStore!.Clear();
    }

    private void WriteContent(
        List<ModuleData> modules,
        List<LessonData> lessons,
        List<LessonGuideData> guides)
    {
        foreach (var module in modules)
        {
            _moduleStore!.Add(module);
        }

        foreach (var lesson in lessons)
        {
            _lessonStore!.Add(lesson);
        }

        foreach (var guide in guides)
        {
            _guideStore!.Add(guide);
        }
    }

    private static string GetOutputFolderPath()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        return Path.Combine(appData, "Scriptum", "Content");
    }
}
