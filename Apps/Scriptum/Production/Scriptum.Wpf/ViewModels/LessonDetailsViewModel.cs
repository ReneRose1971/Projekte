using System;
using PropertyChanged;
using Scriptum.Wpf.Navigation;
using Scriptum.Wpf.Projections;
using Scriptum.Wpf.Projections.Services;

namespace Scriptum.Wpf.ViewModels;

/// <summary>
/// ViewModel für die Lektions-Details.
/// </summary>
[AddINotifyPropertyChangedInterface]
public sealed class LessonDetailsViewModel
{
    private readonly INavigationService _navigationService;
    private readonly IContentQueryService _contentQuery;

    public LessonDetailsViewModel(
        INavigationService navigationService,
        IContentQueryService contentQuery)
    {
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _contentQuery = contentQuery ?? throw new ArgumentNullException(nameof(contentQuery));
    }

    public string ModuleId { get; private set; } = string.Empty;
    public string LessonId { get; private set; } = string.Empty;
    public string Titel { get; private set; } = string.Empty;
    public string Beschreibung { get; private set; } = string.Empty;
    public string PreviewText { get; private set; } = string.Empty;
    public bool HasGuide { get; private set; }

    public void Initialize(string moduleId, string lessonId)
    {
        ModuleId = moduleId;
        LessonId = lessonId;

        _ = LoadDetailsAsync(moduleId, lessonId);
    }

    public void ShowGuide()
    {
        if (!HasGuide) return;
        _navigationService.NavigateToLessonGuide(LessonId);
    }

    public void StartTraining()
    {
        _navigationService.NavigateToTraining(ModuleId, LessonId);
    }

    public void NavigateBack()
    {
        _navigationService.NavigateToLessonList(ModuleId);
    }

    private async System.Threading.Tasks.Task LoadDetailsAsync(string moduleId, string lessonId)
    {
        try
        {
            var details = await _contentQuery.GetLessonDetailsAsync(moduleId, lessonId);

            if (details != null)
            {
                Titel = details.Title;
                Beschreibung = details.Description ?? string.Empty;
                PreviewText = details.PreviewText;
                HasGuide = details.HasGuide;
            }
            else
            {
                Titel = "Lektion nicht gefunden";
                Beschreibung = string.Empty;
                PreviewText = string.Empty;
                HasGuide = false;
            }
        }
        catch
        {
            Titel = "Fehler beim Laden";
            Beschreibung = string.Empty;
            PreviewText = string.Empty;
            HasGuide = false;
        }
    }
}
