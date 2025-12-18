using System;
using System.Linq;
using DataToolKit.Abstractions.DataStores;
using PropertyChanged;
using Scriptum.Content.Data;
using Scriptum.Wpf.Navigation;

namespace Scriptum.Wpf.ViewModels;

/// <summary>
/// ViewModel für die Lektions-Details.
/// </summary>
[AddINotifyPropertyChangedInterface]
public sealed class LessonDetailsViewModel
{
    private readonly INavigationService _navigationService;
    private readonly IDataStore<LessonData> _lessonDataStore;
    private readonly IDataStore<LessonGuideData> _guideDataStore;

    public LessonDetailsViewModel(
        INavigationService navigationService,
        IDataStore<LessonData> lessonDataStore,
        IDataStore<LessonGuideData> guideDataStore)
    {
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _lessonDataStore = lessonDataStore ?? throw new ArgumentNullException(nameof(lessonDataStore));
        _guideDataStore = guideDataStore ?? throw new ArgumentNullException(nameof(guideDataStore));
    }

    public string ModuleId { get; private set; } = string.Empty;
    public string LessonId { get; private set; } = string.Empty;
    public string Titel { get; private set; } = string.Empty;
    public string Beschreibung { get; private set; } = string.Empty;
    public int Schwierigkeit { get; private set; }
    public bool HasGuide { get; private set; }

    public void Initialize(string moduleId, string lessonId)
    {
        ModuleId = moduleId;
        LessonId = lessonId;

        var lesson = _lessonDataStore.Items.FirstOrDefault(l => l.LessonId == lessonId);
        if (lesson != null)
        {
            Titel = lesson.Titel;
            Beschreibung = lesson.Beschreibung;
            Schwierigkeit = lesson.Schwierigkeit;
        }
        else
        {
            Titel = "Unbekannte Lektion";
            Beschreibung = string.Empty;
        }

        HasGuide = _guideDataStore.Items.Any(g => g.LessonId == lessonId);
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
}
