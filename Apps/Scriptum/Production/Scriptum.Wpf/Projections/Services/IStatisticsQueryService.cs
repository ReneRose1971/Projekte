using Scriptum.Wpf.Projections.Models;

namespace Scriptum.Wpf.Projections.Services;

/// <summary>
/// Query-Service für Statistik-Daten.
/// </summary>
public interface IStatisticsQueryService
{
    /// <summary>
    /// Erstellt das Statistik-Dashboard.
    /// </summary>
    Task<StatisticsDashboardModel> BuildDashboardAsync(StatisticsFilter filter, CancellationToken ct = default);

    /// <summary>
    /// Erstellt die Error-Heatmap.
    /// </summary>
    Task<ErrorHeatmapModel> BuildErrorHeatmapAsync(StatisticsFilter filter, CancellationToken ct = default);
}
