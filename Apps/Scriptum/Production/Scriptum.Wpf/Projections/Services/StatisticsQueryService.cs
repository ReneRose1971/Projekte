using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DataToolKit.Abstractions.DataStores;
using DataToolKit.Storage.Repositories;
using Scriptum.Content.Data;
using Scriptum.Core;
using Scriptum.Progress;
using Scriptum.Wpf.Projections.Models;

namespace Scriptum.Wpf.Projections.Services;

/// <summary>
/// Implementierung des Statistik-Query-Service.
/// </summary>
internal sealed class StatisticsQueryService : IStatisticsQueryService
{
    private readonly ReadOnlyObservableCollection<TrainingSession> _sessions;
    private readonly ReadOnlyObservableCollection<ModuleData> _modules;
    private readonly ReadOnlyObservableCollection<LessonData> _lessons;

    public StatisticsQueryService(
        IDataStoreProvider dataStoreProvider,
        IRepositoryFactory repositoryFactory)
    {
        if (dataStoreProvider == null) throw new ArgumentNullException(nameof(dataStoreProvider));
        if (repositoryFactory == null) throw new ArgumentNullException(nameof(repositoryFactory));

        var sessionStore = dataStoreProvider.GetPersistent<TrainingSession>(
            repositoryFactory,
            isSingleton: true,
            trackPropertyChanges: false,
            autoLoad: true);

        var moduleStore = dataStoreProvider.GetPersistent<ModuleData>(
            repositoryFactory,
            isSingleton: true,
            trackPropertyChanges: false,
            autoLoad: true);

        var lessonStore = dataStoreProvider.GetPersistent<LessonData>(
            repositoryFactory,
            isSingleton: true,
            trackPropertyChanges: false,
            autoLoad: true);

        _sessions = sessionStore.Items;
        _modules = moduleStore.Items;
        _lessons = lessonStore.Items;
    }

    public Task<StatisticsDashboardModel> BuildDashboardAsync(StatisticsFilter filter, CancellationToken ct = default)
    {
        var sessions = ApplyFilter(_sessions, filter).ToList();

        var moduleStats = BuildModuleStats(sessions);
        var lessonStats = BuildLessonStats(sessions);

        var model = new StatisticsDashboardModel(moduleStats, lessonStats);
        return Task.FromResult(model);
    }

    public Task<ErrorHeatmapModel> BuildErrorHeatmapAsync(StatisticsFilter filter, CancellationToken ct = default)
    {
        var sessions = ApplyFilter(_sessions, filter).ToList();

        var errorGroups = sessions
            .SelectMany(s => s.Evaluations)
            .Where(e => e.Ergebnis == EvaluationOutcome.Falsch)
            .GroupBy(e => new { Expected = NormalizeSymbol(e.Erwartet), Actual = NormalizeSymbol(e.Tatsaechlich) })
            .Select(g => new ErrorHeatmapRow(g.Key.Expected, g.Key.Actual, g.Count()))
            .OrderByDescending(r => r.Count)
            .Take(50)
            .ToList();

        var hintText = errorGroups.Count == 0
            ? "Keine Fehlerdaten verfügbar. Trainiere einige Lektionen, um Statistiken zu sehen."
            : $"Top {errorGroups.Count} häufigste Fehler";

        var model = new ErrorHeatmapModel(errorGroups, hintText);
        return Task.FromResult(model);
    }

    private static IEnumerable<TrainingSession> ApplyFilter(
        IEnumerable<TrainingSession> sessions,
        StatisticsFilter filter)
    {
        var query = sessions;

        if (!string.IsNullOrWhiteSpace(filter.ModuleId))
            query = query.Where(s => s.ModuleId == filter.ModuleId);

        if (!string.IsNullOrWhiteSpace(filter.LessonId))
            query = query.Where(s => s.LessonId == filter.LessonId);

        if (filter.From.HasValue)
            query = query.Where(s => s.StartedAt.DateTime >= filter.From.Value);

        if (filter.To.HasValue)
            query = query.Where(s => s.StartedAt.DateTime <= filter.To.Value);

        return query;
    }

    private IReadOnlyList<ModuleStatRow> BuildModuleStats(List<TrainingSession> sessions)
    {
        var moduleGroups = sessions
            .GroupBy(s => s.ModuleId)
            .Select(g =>
            {
                var moduleId = g.Key;
                var moduleTitle = _modules.FirstOrDefault(m => m.ModuleId == moduleId)?.Titel ?? moduleId;

                var completedSessions = g.Where(s => s.IsCompleted).ToList();
                var avgErrors = g.Any() ? g.Average(s => s.Evaluations.Count(e => e.Ergebnis == EvaluationOutcome.Falsch)) : 0.0;
                var avgAccuracy = CalculateAverageAccuracy(g.ToList());

                TimeSpan? bestDuration = null;
                foreach (var session in completedSessions.Where(s => s.EndedAt.HasValue))
                {
                    var duration = session.EndedAt!.Value - session.StartedAt;
                    if (bestDuration == null || duration < bestDuration.Value)
                        bestDuration = duration;
                }

                var lastTrained = g
                    .Select(s => s.EndedAt?.DateTime ?? s.StartedAt.DateTime)
                    .Max();

                return new ModuleStatRow(
                    moduleId,
                    moduleTitle,
                    g.Count(),
                    completedSessions.Count,
                    avgErrors,
                    avgAccuracy,
                    bestDuration,
                    lastTrained);
            })
            .OrderByDescending(r => r.SessionsCount)
            .ToList();

        return moduleGroups;
    }

    private IReadOnlyList<LessonStatRow> BuildLessonStats(List<TrainingSession> sessions)
    {
        var lessonGroups = sessions
            .GroupBy(s => new { s.LessonId, s.ModuleId })
            .Select(g =>
            {
                var lessonId = g.Key.LessonId;
                var moduleId = g.Key.ModuleId;
                var lessonTitle = _lessons.FirstOrDefault(l => l.LessonId == lessonId)?.Titel ?? lessonId;

                var completedSessions = g.Where(s => s.IsCompleted).ToList();
                var avgErrors = g.Any() ? g.Average(s => s.Evaluations.Count(e => e.Ergebnis == EvaluationOutcome.Falsch)) : 0.0;
                var avgAccuracy = CalculateAverageAccuracy(g.ToList());

                TimeSpan? bestDuration = null;
                foreach (var session in completedSessions.Where(s => s.EndedAt.HasValue))
                {
                    var duration = session.EndedAt!.Value - session.StartedAt;
                    if (bestDuration == null || duration < bestDuration.Value)
                        bestDuration = duration;
                }

                var lastTrained = g
                    .Select(s => s.EndedAt?.DateTime ?? s.StartedAt.DateTime)
                    .Max();

                return new LessonStatRow(
                    lessonId,
                    moduleId,
                    lessonTitle,
                    g.Count(),
                    completedSessions.Count,
                    avgErrors,
                    avgAccuracy,
                    bestDuration,
                    lastTrained);
            })
            .OrderByDescending(r => r.SessionsCount)
            .ToList();

        return lessonGroups;
    }

    private static double CalculateAverageAccuracy(List<TrainingSession> sessions)
    {
        if (sessions.Count == 0)
            return 0.0;

        var accuracies = sessions.Select(s =>
        {
            var totalInputs = s.Inputs.Count;
            var totalErrors = s.Evaluations.Count(e => e.Ergebnis == EvaluationOutcome.Falsch);
            return totalInputs > 0 ? 1.0 - (totalErrors / (double)totalInputs) : 0.0;
        }).ToList();

        return accuracies.Average();
    }

    private static string NormalizeSymbol(string symbol)
    {
        if (string.IsNullOrEmpty(symbol))
            return "<leer>";

        return symbol switch
        {
            "\n" => "?",
            "\r" => "?",
            "\t" => "?",
            " " => "?",
            _ => symbol
        };
    }
}
