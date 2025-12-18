namespace Scriptum.Wpf.Projections;

/// <summary>
/// UI-Projektion für eine Lektions-Anleitung.
/// </summary>
public sealed record LessonGuideModel(
    string LessonId,
    string Title,
    string GuideText);
