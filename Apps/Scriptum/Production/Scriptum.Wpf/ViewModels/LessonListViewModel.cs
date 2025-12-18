using System;
using System.Collections.ObjectModel;
using PropertyChanged;
using Scriptum.Wpf.Navigation;
using Scriptum.Wpf.Projections;
using Scriptum.Wpf.Projections.Services;

namespace Scriptum.Wpf.ViewModels;

/// <summary>
/// ViewModel für die Lektions-Liste eines Moduls.
/// </summary>
[AddINotifyPropertyChangedInterface]
public sealed class LessonListViewModel
{
    private readonly INavigationService _navigationService;
    private readonly IContentQueryService _contentQuery;

    public LessonListViewModel(
        INavigationService navigationService,
        IContentQueryService contentQuery)
    {
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _contentQuery = contentQuery ?? throw new ArgumentNullException(nameof(contentQuery));

        Lessons = new ObservableCollection<LessonListItem>();
    }

    public string ModuleId { get; private set; } = string.Empty;
    public string ModuleTitel { get; private set; } = string.Empty;
    public ObservableCollection<LessonListItem> Lessons { get; }

    public void Initialize(string moduleId)
    {
        ModuleId = moduleId;
        _ = LoadLessonsAsync(moduleId);
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

    private async System.Threading.Tasks.Task LoadLessonsAsync(string moduleId)
    {
        try
        {
            var lessons = await _contentQuery.GetLessonsByModuleAsync(moduleId);
            
            Lessons.Clear();
            foreach (var lesson in lessons)
            {
                Lessons.Add(lesson);
            }

            ModuleTitel = lessons.Count > 0 ? "Lektionen" : "Keine Lektionen";
        }
        catch
        {
            ModuleTitel = "Fehler beim Laden";
        }
    }
}
