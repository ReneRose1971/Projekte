using System;
using PropertyChanged;
using Scriptum.Wpf.Navigation;
using Scriptum.Wpf.Projections.Services;

namespace Scriptum.Wpf.ViewModels;

/// <summary>
/// ViewModel für den Lektions-Guide.
/// </summary>
[AddINotifyPropertyChangedInterface]
public sealed class LessonGuideViewModel
{
    private readonly INavigationService _navigationService;
    private readonly IContentQueryService _contentQuery;

    public LessonGuideViewModel(
        INavigationService navigationService,
        IContentQueryService contentQuery)
    {
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _contentQuery = contentQuery ?? throw new ArgumentNullException(nameof(contentQuery));
    }

    public string LessonId { get; private set; } = string.Empty;
    public string Titel { get; private set; } = string.Empty;
    public string GuideText { get; private set; } = string.Empty;

    public void Initialize(string lessonId)
    {
        LessonId = lessonId;
        _ = LoadGuideAsync(lessonId);
    }

    public void NavigateBack()
    {
        _navigationService.NavigateToHome();
    }

    private async System.Threading.Tasks.Task LoadGuideAsync(string lessonId)
    {
        try
        {
            var guide = await _contentQuery.GetLessonGuideAsync(lessonId);

            if (guide != null)
            {
                Titel = guide.Title;
                GuideText = guide.GuideText;
            }
            else
            {
                Titel = "Anleitung nicht gefunden";
                GuideText = "Keine Anleitung verfügbar.";
            }
        }
        catch
        {
            Titel = "Fehler";
            GuideText = "Fehler beim Laden der Anleitung.";
        }
    }
}
