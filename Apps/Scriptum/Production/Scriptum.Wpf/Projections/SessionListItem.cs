namespace Scriptum.Wpf.Projections;

/// <summary>
/// UI-Projektion für eine Session in der Liste.
/// </summary>
public sealed record SessionListItem(string SessionId, string LessonTitel, string Datum, int Fehler, bool IsCompleted);
