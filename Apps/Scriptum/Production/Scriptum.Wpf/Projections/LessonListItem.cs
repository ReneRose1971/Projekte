namespace Scriptum.Wpf.Projections;

/// <summary>
/// UI-Projektion für eine Lektion in der Liste.
/// </summary>
public sealed record LessonListItem(string LessonId, string Titel, string Beschreibung, int Schwierigkeit);
