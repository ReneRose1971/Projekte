using DataToolKit.Abstractions.DataStores;
using Scriptum.Content.Data;
using Scriptum.Progress;
using Scriptum.Wpf.Projections;
using Scriptum.Wpf.Projections.Models;

namespace Scriptum.Wpf.Projections.Services;

/// <summary>
/// Implementierung des Content-Query-Service.
/// </summary>
internal sealed class ContentQueryService : IContentQueryService
{
    private readonly IDataStore<ModuleData> _moduleStore;
    private readonly IDataStore<LessonData> _lessonStore;
    private readonly IDataStore<LessonGuideData> _guideStore;
    private readonly IDataStore<TrainingSession> _sessionStore;

    public ContentQueryService(
        IDataStore<ModuleData> moduleStore,
        IDataStore<LessonData> lessonStore,
        IDataStore<LessonGuideData> guideStore,
        IDataStore<TrainingSession> sessionStore)
    {
        _moduleStore = moduleStore ?? throw new ArgumentNullException(nameof(moduleStore));
        _lessonStore = lessonStore ?? throw new ArgumentNullException(nameof(lessonStore));
        _guideStore = guideStore ?? throw new ArgumentNullException(nameof(guideStore));
        _sessionStore = sessionStore ?? throw new ArgumentNullException(nameof(sessionStore));
    }

    public Task<IReadOnlyList<ModuleListItem>> GetModulesAsync(CancellationToken ct = default)
    {
        var modules = _moduleStore.Items
            .OrderBy(m => m.Order)
            .ThenBy(m => m.Titel)
            .Select(m =>
            {
                var lessonCount = _lessonStore.Items.Count(l => l.ModuleId == m.ModuleId);
                var progress = CalculateModuleProgress(m.ModuleId);

                return new ModuleListItem(
                    m.ModuleId,
                    m.Titel,
                    string.IsNullOrWhiteSpace(m.Beschreibung) ? null : m.Beschreibung,
                    lessonCount,
                    progress);
            })
            .ToList();

        return Task.FromResult<IReadOnlyList<ModuleListItem>>(modules);
    }

    public Task<IReadOnlyList<LessonListItem>> GetLessonsByModuleAsync(string moduleId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(moduleId))
            return Task.FromResult<IReadOnlyList<LessonListItem>>(Array.Empty<LessonListItem>());

        var lessons = _lessonStore.Items
            .Where(l => l.ModuleId == moduleId)
            .OrderBy(l => l.Schwierigkeit)
            .ThenBy(l => l.Titel)
            .Select(l =>
            {
                var progress = CalculateLessonProgress(l.LessonId);

                return new LessonListItem(
                    l.LessonId,
                    l.ModuleId,
                    l.Titel,
                    string.IsNullOrWhiteSpace(l.Beschreibung) ? null : l.Beschreibung,
                    l.Uebungstext?.Length ?? 0,
                    progress);
            })
            .ToList();

        return Task.FromResult<IReadOnlyList<LessonListItem>>(lessons);
    }

    public Task<LessonDetailsModel?> GetLessonDetailsAsync(string moduleId, string lessonId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(moduleId) || string.IsNullOrWhiteSpace(lessonId))
            return Task.FromResult<LessonDetailsModel?>(null);

        var lesson = _lessonStore.Items.FirstOrDefault(l => l.LessonId == lessonId && l.ModuleId == moduleId);
        if (lesson == null)
            return Task.FromResult<LessonDetailsModel?>(null);

        var hasGuide = _guideStore.Items.Any(g => g.LessonId == lessonId);
        var progress = CalculateLessonProgress(lessonId);
        var previewText = CreatePreviewText(lesson.Uebungstext);

        var model = new LessonDetailsModel(
            lesson.LessonId,
            lesson.ModuleId,
            lesson.Titel,
            string.IsNullOrWhiteSpace(lesson.Beschreibung) ? null : lesson.Beschreibung,
            previewText,
            hasGuide,
            progress);

        return Task.FromResult<LessonDetailsModel?>(model);
    }

    public Task<LessonGuideModel?> GetLessonGuideAsync(string lessonId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(lessonId))
            return Task.FromResult<LessonGuideModel?>(null);

        var guide = _guideStore.Items.FirstOrDefault(g => g.LessonId == lessonId);
        if (guide == null)
            return Task.FromResult<LessonGuideModel?>(null);

        var lesson = _lessonStore.Items.FirstOrDefault(l => l.LessonId == lessonId);
        var title = lesson?.Titel ?? lessonId;

        var model = new LessonGuideModel(
            lessonId,
            title,
            guide.GuideTextMarkdown ?? string.Empty);

        return Task.FromResult<LessonGuideModel?>(model);
    }

    private ModuleProgressSummary? CalculateModuleProgress(string moduleId)
    {
        var moduleSessions = _sessionStore.Items
            .Where(s => s.ModuleId == moduleId)
            .ToList();

        if (moduleSessions.Count == 0)
            return null;

        var completedCount = moduleSessions.Count(s => s.IsCompleted);
        var lastTrained = moduleSessions
            .Select(s => s.EndedAt?.DateTime ?? s.StartedAt.DateTime)
            .Max();

        return new ModuleProgressSummary(moduleSessions.Count, completedCount, lastTrained);
    }

    private LessonProgressSummary? CalculateLessonProgress(string lessonId)
    {
        var lessonSessions = _sessionStore.Items
            .Where(s => s.LessonId == lessonId)
            .ToList();

        if (lessonSessions.Count == 0)
            return null;

        var completedSessions = lessonSessions.Where(s => s.IsCompleted).ToList();
        var completedCount = completedSessions.Count;

        var bestAccuracy = 0.0;
        TimeSpan? bestDuration = null;

        foreach (var session in completedSessions)
        {
            var totalInputs = session.Inputs.Count;
            var totalErrors = session.Evaluations.Count(e => e.Ergebnis == Core.EvaluationOutcome.Falsch);
            var accuracy = totalInputs > 0 ? 1.0 - (totalErrors / (double)totalInputs) : 0.0;

            if (accuracy > bestAccuracy)
                bestAccuracy = accuracy;

            if (session.EndedAt.HasValue)
            {
                var duration = session.EndedAt.Value - session.StartedAt;
                if (bestDuration == null || duration < bestDuration.Value)
                    bestDuration = duration;
            }
        }

        var lastTrained = lessonSessions
            .Select(s => s.EndedAt?.DateTime ?? s.StartedAt.DateTime)
            .Max();

        return new LessonProgressSummary(
            lessonSessions.Count,
            completedCount,
            bestAccuracy,
            bestDuration,
            lastTrained);
    }

    private static string CreatePreviewText(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        const int maxLength = 120;
        var preview = text.Length <= maxLength ? text : text.Substring(0, maxLength);
        
        preview = preview.Replace("\n", "?");
        
        if (text.Length > maxLength)
            preview += "...";

        return preview;
    }
}
