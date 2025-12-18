using DataToolKit.Abstractions.DataStores;
using DataToolKit.Abstractions.DI;
using DataToolKit.Storage.DataStores;
using DataToolKit.Storage.Repositories;
using Scriptum.Content.Data;
using Scriptum.Core;
using Scriptum.Progress;
using Scriptum.Wpf.Projections;
using Scriptum.Wpf.Projections.Models;

namespace Scriptum.Wpf.Projections.Services;

/// <summary>
/// Implementierung des Session-Query-Service.
/// </summary>
internal sealed class SessionQueryService : ISessionQueryService
{
    private readonly PersistentDataStore<TrainingSession> _sessionStore;
    private readonly PersistentDataStore<ModuleData> _moduleStore;
    private readonly PersistentDataStore<LessonData> _lessonStore;

    /// <summary>
    /// Erstellt eine neue Instanz des SessionQueryService.
    /// </summary>
    /// <param name="dataStoreProvider">Provider für DataStores.</param>
    /// <param name="repositoryFactory">Factory für Repositories.</param>
    public SessionQueryService(
        IDataStoreProvider dataStoreProvider,
        IRepositoryFactory repositoryFactory)
    {
        if (dataStoreProvider == null)
            throw new ArgumentNullException(nameof(dataStoreProvider));
        if (repositoryFactory == null)
            throw new ArgumentNullException(nameof(repositoryFactory));

        _sessionStore = dataStoreProvider.GetPersistent<TrainingSession>(
            repositoryFactory,
            isSingleton: true,
            trackPropertyChanges: false,
            autoLoad: true);

        _moduleStore = dataStoreProvider.GetPersistent<ModuleData>(
            repositoryFactory,
            isSingleton: true,
            trackPropertyChanges: false,
            autoLoad: true);

        _lessonStore = dataStoreProvider.GetPersistent<LessonData>(
            repositoryFactory,
            isSingleton: true,
            trackPropertyChanges: false,
            autoLoad: true);
    }

    public Task<IReadOnlyList<SessionListItem>> GetRecentSessionsAsync(int take, CancellationToken ct = default)
    {
        var sessions = _sessionStore.Items
            .OrderByDescending(s => s.StartedAt)
            .Take(take)
            .Select(CreateSessionListItem)
            .ToList();

        return Task.FromResult<IReadOnlyList<SessionListItem>>(sessions);
    }

    public Task<IReadOnlyList<SessionListItem>> GetSessionsByFilterAsync(SessionFilter filter, CancellationToken ct = default)
    {
        var query = _sessionStore.Items.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(filter.ModuleId))
            query = query.Where(s => s.ModuleId == filter.ModuleId);

        if (!string.IsNullOrWhiteSpace(filter.LessonId))
            query = query.Where(s => s.LessonId == filter.LessonId);

        if (filter.From.HasValue)
            query = query.Where(s => s.StartedAt.DateTime >= filter.From.Value);

        if (filter.To.HasValue)
            query = query.Where(s => s.StartedAt.DateTime <= filter.To.Value);

        if (filter.OnlyCompleted.HasValue)
            query = query.Where(s => s.IsCompleted == filter.OnlyCompleted.Value);

        var sessions = query
            .OrderByDescending(s => s.StartedAt)
            .Select(CreateSessionListItem)
            .ToList();

        return Task.FromResult<IReadOnlyList<SessionListItem>>(sessions);
    }

    public Task<SessionDetailModel?> GetSessionDetailAsync(int sessionId, CancellationToken ct = default)
    {
        var session = _sessionStore.Items.FirstOrDefault(s => s.Id == sessionId);
        if (session == null)
            return Task.FromResult<SessionDetailModel?>(null);

        var moduleTitle = _moduleStore.Items.FirstOrDefault(m => m.ModuleId == session.ModuleId)?.Titel ?? session.ModuleId;
        var lessonTitle = _lessonStore.Items.FirstOrDefault(l => l.LessonId == session.LessonId)?.Titel ?? session.LessonId;

        var header = new SessionHeader(
            session.StartedAt.DateTime,
            session.EndedAt?.DateTime,
            moduleTitle,
            lessonTitle,
            session.ModuleId,
            session.LessonId);

        var events = CreateEventRows(session);
        var errors = CreateErrorRows(session);
        var metrics = CalculateMetrics(session);

        var model = new SessionDetailModel(sessionId, header, events, errors, metrics);
        return Task.FromResult<SessionDetailModel?>(model);
    }

    public Task<SessionListItem?> GetLastSessionAsync(CancellationToken ct = default)
    {
        var lastSession = _sessionStore.Items
            .OrderByDescending(s => s.StartedAt)
            .FirstOrDefault();

        if (lastSession == null)
            return Task.FromResult<SessionListItem?>(null);

        var item = CreateSessionListItem(lastSession);
        return Task.FromResult<SessionListItem?>(item);
    }

    private SessionListItem CreateSessionListItem(TrainingSession session)
    {
        var moduleTitle = _moduleStore.Items.FirstOrDefault(m => m.ModuleId == session.ModuleId)?.Titel ?? session.ModuleId;
        var lessonTitle = _lessonStore.Items.FirstOrDefault(l => l.LessonId == session.LessonId)?.Titel ?? session.LessonId;

        var totalInputs = session.Inputs.Count;
        var totalErrors = session.Evaluations.Count(e => e.Ergebnis == EvaluationOutcome.Falsch);

        TimeSpan? duration = null;
        if (session.IsCompleted && session.EndedAt.HasValue)
            duration = session.EndedAt.Value - session.StartedAt;

        return new SessionListItem(
            session.Id,
            session.StartedAt.DateTime,
            session.EndedAt?.DateTime,
            session.ModuleId,
            session.LessonId,
            moduleTitle,
            lessonTitle,
            session.IsCompleted,
            totalInputs,
            totalErrors,
            duration);
    }

    private static IReadOnlyList<SessionEventRow> CreateEventRows(TrainingSession session)
    {
        var events = new List<SessionEventRow>();
        var index = 0;

        foreach (var eval in session.Evaluations.OrderBy(e => e.TokenIndex))
        {
            var isError = eval.Ergebnis == EvaluationOutcome.Falsch;
            
            events.Add(new SessionEventRow(
                index++,
                DateTime.MinValue,
                "Evaluation",
                NormalizeSymbol(eval.Erwartet),
                NormalizeSymbol(eval.Tatsaechlich),
                isError));
        }

        return events;
    }

    private static IReadOnlyList<SessionErrorRow> CreateErrorRows(TrainingSession session)
    {
        var errors = new List<SessionErrorRow>();
        var index = 0;

        foreach (var eval in session.Evaluations.Where(e => e.Ergebnis == EvaluationOutcome.Falsch))
        {
            errors.Add(new SessionErrorRow(
                index++,
                DateTime.MinValue,
                NormalizeSymbol(eval.Erwartet),
                NormalizeSymbol(eval.Tatsaechlich),
                eval.Ergebnis.ToString()));
        }

        return errors;
    }

    private static SessionMetrics CalculateMetrics(TrainingSession session)
    {
        var totalInputs = session.Inputs.Count;
        var totalErrors = session.Evaluations.Count(e => e.Ergebnis == EvaluationOutcome.Falsch);
        var accuracy = totalInputs > 0 ? 1.0 - (totalErrors / (double)totalInputs) : 0.0;

        TimeSpan? duration = null;
        double? inputsPerMinute = null;

        if (session.IsCompleted && session.EndedAt.HasValue)
        {
            duration = session.EndedAt.Value - session.StartedAt;
            if (duration.Value.TotalMinutes > 0)
                inputsPerMinute = totalInputs / duration.Value.TotalMinutes;
        }

        return new SessionMetrics(totalInputs, totalErrors, accuracy, duration, inputsPerMinute);
    }

    private static string NormalizeSymbol(string symbol)
    {
        if (string.IsNullOrEmpty(symbol))
            return string.Empty;

        return symbol switch
        {
            "\n" => "?",
            "\r" => "?",
            "\t" => "?",
            _ => symbol
        };
    }
}
