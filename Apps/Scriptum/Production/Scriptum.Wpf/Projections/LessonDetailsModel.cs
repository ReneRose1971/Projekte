using Scriptum.Wpf.Projections.Models;

namespace Scriptum.Wpf.Projections;

/// <summary>
/// UI-Projektion für Lektionsdetails.
/// </summary>
public sealed record LessonDetailsModel(
    string LessonId,
    string ModuleId,
    string Title,
    string? Description,
    string PreviewText,
    bool HasGuide,
    LessonProgressSummary? Progress);
