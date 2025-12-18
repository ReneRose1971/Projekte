using Scriptum.Wpf.Projections;
using Scriptum.Wpf.Projections.Models;

namespace Scriptum.Wpf.Projections.Services;

/// <summary>
/// Query-Service für Session-Daten.
/// </summary>
public interface ISessionQueryService
{
    /// <summary>
    /// Lädt die letzten N Sessions.
    /// </summary>
    Task<IReadOnlyList<SessionListItem>> GetRecentSessionsAsync(int take, CancellationToken ct = default);

    /// <summary>
    /// Lädt Sessions nach Filter.
    /// </summary>
    Task<IReadOnlyList<SessionListItem>> GetSessionsByFilterAsync(SessionFilter filter, CancellationToken ct = default);

    /// <summary>
    /// Lädt Session-Details.
    /// </summary>
    Task<SessionDetailModel?> GetSessionDetailAsync(int sessionId, CancellationToken ct = default);

    /// <summary>
    /// Lädt die letzte Session.
    /// </summary>
    Task<SessionListItem?> GetLastSessionAsync(CancellationToken ct = default);
}
