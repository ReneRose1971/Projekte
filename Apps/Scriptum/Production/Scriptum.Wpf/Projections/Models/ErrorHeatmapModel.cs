namespace Scriptum.Wpf.Projections.Models;

/// <summary>
/// Error-Heatmap-Modell.
/// </summary>
public sealed record ErrorHeatmapModel(
    IReadOnlyList<ErrorHeatmapRow> Rows,
    string HintText);
