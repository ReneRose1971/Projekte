namespace Scriptum.Wpf.Projections.Models;

/// <summary>
/// UI-Projektion für Session-Details.
/// </summary>
public sealed record SessionDetailModel(
    int SessionId,
    SessionHeader Header,
    IReadOnlyList<SessionEventRow> Events,
    IReadOnlyList<SessionErrorRow> Errors,
    SessionMetrics Metrics);
