using System;
using System.Linq;
using DataToolKit.Abstractions.DataStores;
using PropertyChanged;
using Scriptum.Content.Data;
using Scriptum.Wpf.Navigation;

namespace Scriptum.Wpf.ViewModels;

/// <summary>
/// ViewModel für den Lektions-Guide.
/// </summary>
[AddINotifyPropertyChangedInterface]
public sealed class LessonGuideViewModel
{
    private readonly INavigationService _navigationService;
    private readonly IDataStore<LessonGuideData> _guideDataStore;
    private readonly IDataStore<LessonData> _lessonDataStore;

    public LessonGuideViewModel(
        INavigationService navigationService,
        IDataStore<LessonGuideData> guideDataStore,
        IDataStore<LessonData> lessonDataStore)
    {
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _guideDataStore = guideDataStore ?? throw new ArgumentNullException(nameof(guideDataStore));
        _lessonDataStore = lessonDataStore ?? throw new ArgumentNullException(nameof(lessonDataStore));
    }

    public string LessonId { get; private set; } = string.Empty;
    public string Titel { get; private set; } = string.Empty;
    public string GuideText { get; private set; } = string.Empty;

    public void Initialize(string lessonId)
    {
        LessonId = lessonId;

        var lesson = _lessonDataStore.Items.FirstOrDefault(l => l.LessonId == lessonId);
        Titel = lesson?.Titel ?? "Unbekannte Lektion";

        var guide = _guideDataStore.Items.FirstOrDefault(g => g.LessonId == lessonId);
        GuideText = guide?.GuideTextMarkdown ?? "Keine Anleitung verfügbar.";
    }

    public void NavigateBack()
    {
        var lesson = _lessonDataStore.Items.FirstOrDefault(l => l.LessonId == LessonId);
        if (lesson != null)
        {
            _navigationService.NavigateToLessonDetails(lesson.ModuleId, LessonId);
        }
        else
        {
            _navigationService.NavigateToHome();
        }
    }
}
