using System;
using System.Collections.ObjectModel;
using System.Linq;
using DataToolKit.Abstractions.DataStores;
using PropertyChanged;
using Scriptum.Content.Data;
using Scriptum.Wpf.Navigation;
using Scriptum.Wpf.Projections;

namespace Scriptum.Wpf.ViewModels;

/// <summary>
/// ViewModel für die Lektions-Liste eines Moduls.
/// </summary>
[AddINotifyPropertyChangedInterface]
public sealed class LessonListViewModel
{
    private readonly INavigationService _navigationService;
    private readonly IDataStore<LessonData> _lessonDataStore;
    private readonly IDataStore<ModuleData> _moduleDataStore;

    public LessonListViewModel(
        INavigationService navigationService,
        IDataStore<LessonData> lessonDataStore,
        IDataStore<ModuleData> moduleDataStore)
    {
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _lessonDataStore = lessonDataStore ?? throw new ArgumentNullException(nameof(lessonDataStore));
        _moduleDataStore = moduleDataStore ?? throw new ArgumentNullException(nameof(moduleDataStore));

        Lessons = new ObservableCollection<LessonListItem>();
    }

    public string ModuleId { get; private set; } = string.Empty;
    public string ModuleTitel { get; private set; } = string.Empty;
    public ObservableCollection<LessonListItem> Lessons { get; }

    public void Initialize(string moduleId)
    {
        ModuleId = moduleId;

        var module = _moduleDataStore.Items.FirstOrDefault(m => m.ModuleId == moduleId);
        ModuleTitel = module?.Titel ?? "Unbekanntes Modul";

        Lessons.Clear();
        var lessons = _lessonDataStore.Items
            .Where(l => l.ModuleId == moduleId)
            .OrderBy(l => l.Schwierigkeit)
            .ThenBy(l => l.Titel)
            .Select(l => new LessonListItem(l.LessonId, l.Titel, l.Beschreibung, l.Schwierigkeit));

        foreach (var lesson in lessons)
        {
            Lessons.Add(lesson);
        }
    }

    public void ShowDetails(LessonListItem lesson)
    {
        if (lesson == null) return;
        _navigationService.NavigateToLessonDetails(ModuleId, lesson.LessonId);
    }

    public void StartTraining(LessonListItem lesson)
    {
        if (lesson == null) return;
        _navigationService.NavigateToTraining(ModuleId, lesson.LessonId);
    }

    public void NavigateBack()
    {
        _navigationService.NavigateToModuleList();
    }
}
