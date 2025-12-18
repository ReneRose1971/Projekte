namespace Scriptum.Wpf.Projections.Models;

/// <summary>
/// Error-Heatmap-Zeile (Erwartet -> Tatsächlich -> Häufigkeit).
/// </summary>
public sealed record ErrorHeatmapRow(
    string ExpectedSymbol,
    string ActualSymbol,
    int Count);
