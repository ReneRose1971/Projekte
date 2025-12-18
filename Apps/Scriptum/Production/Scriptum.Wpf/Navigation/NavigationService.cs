using System;
using Scriptum.Wpf.ViewModels;

namespace Scriptum.Wpf.Navigation;

/// <summary>
/// Standard-Implementierung des Navigationsservice.
/// </summary>
public sealed class NavigationService : INavigationService
{
    private readonly ShellViewModel _shellViewModel;

    public NavigationService(ShellViewModel shellViewModel)
    {
        _shellViewModel = shellViewModel ?? throw new ArgumentNullException(nameof(shellViewModel));
    }

    public void NavigateToHome() => _shellViewModel.ShowHome();

    public void NavigateToModuleList() => _shellViewModel.ShowModuleList();

    public void NavigateToLessonList(string moduleId) => _shellViewModel.ShowLessonList(moduleId);

    public void NavigateToLessonDetails(string moduleId, string lessonId) 
        => _shellViewModel.ShowLessonDetails(moduleId, lessonId);

    public void NavigateToLessonGuide(string lessonId) => _shellViewModel.ShowLessonGuide(lessonId);

    public void NavigateToTraining(string moduleId, string lessonId) 
        => _shellViewModel.ShowTraining(moduleId, lessonId);

    public void NavigateToTrainingSummary(int? sessionId = null) 
        => _shellViewModel.ShowTrainingSummary(sessionId);

    public void NavigateToSessionHistory() => _shellViewModel.ShowSessionHistory();

    public void NavigateToSessionDetail(int sessionId) => _shellViewModel.ShowSessionDetail(sessionId);

    public void NavigateToStatisticsDashboard() => _shellViewModel.ShowStatisticsDashboard();

    public void NavigateToErrorHeatmap() => _shellViewModel.ShowErrorHeatmap();

    public void NavigateToSettings() => _shellViewModel.ShowSettings();

    public void NavigateToContentManagement() => _shellViewModel.ShowContentManagement();

    public void NavigateToContentImport() => _shellViewModel.ShowContentImport();
}
