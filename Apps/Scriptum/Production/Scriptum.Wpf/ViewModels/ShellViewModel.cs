using System;
using Microsoft.Extensions.DependencyInjection;
using PropertyChanged;

namespace Scriptum.Wpf.ViewModels;

/// <summary>
/// Shell-ViewModel: Verwaltet die Navigation zwischen Views.
/// </summary>
[AddINotifyPropertyChangedInterface]
public sealed class ShellViewModel
{
    private readonly IServiceProvider _serviceProvider;

    public ShellViewModel(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        Title = "Scriptum Discipulus";
    }

    public string Title { get; }

    public object? CurrentViewModel { get; private set; }

    public void ShowHome()
    {
        CurrentViewModel = _serviceProvider.GetRequiredService<HomeViewModel>();
    }

    public void ShowModuleList()
    {
        CurrentViewModel = _serviceProvider.GetRequiredService<ModuleListViewModel>();
    }

    public void ShowLessonList(string moduleId)
    {
        var vm = _serviceProvider.GetRequiredService<LessonListViewModel>();
        vm.Initialize(moduleId);
        CurrentViewModel = vm;
    }

    public void ShowLessonDetails(string moduleId, string lessonId)
    {
        var vm = _serviceProvider.GetRequiredService<LessonDetailsViewModel>();
        vm.Initialize(moduleId, lessonId);
        CurrentViewModel = vm;
    }

    public void ShowLessonGuide(string lessonId)
    {
        var vm = _serviceProvider.GetRequiredService<LessonGuideViewModel>();
        vm.Initialize(lessonId);
        CurrentViewModel = vm;
    }

    public void ShowTraining(string moduleId, string lessonId)
    {
        var vm = _serviceProvider.GetRequiredService<TrainingViewModel>();
        vm.Initialize(moduleId, lessonId);
        CurrentViewModel = vm;
    }

    public void ShowTrainingSummary(int? sessionId = null)
    {
        var vm = _serviceProvider.GetRequiredService<TrainingSummaryViewModel>();
        vm.Initialize(sessionId);
        CurrentViewModel = vm;
    }

    public void ShowSessionHistory()
    {
        CurrentViewModel = _serviceProvider.GetRequiredService<SessionHistoryViewModel>();
    }

    public void ShowSessionDetail(int sessionId)
    {
        var vm = _serviceProvider.GetRequiredService<SessionDetailViewModel>();
        vm.Initialize(sessionId);
        CurrentViewModel = vm;
    }

    public void ShowStatisticsDashboard()
    {
        CurrentViewModel = _serviceProvider.GetRequiredService<StatisticsDashboardViewModel>();
    }

    public void ShowErrorHeatmap()
    {
        CurrentViewModel = _serviceProvider.GetRequiredService<ErrorHeatmapViewModel>();
    }

    public void ShowSettings()
    {
        CurrentViewModel = _serviceProvider.GetRequiredService<SettingsViewModel>();
    }

    public void ShowContentManagement()
    {
        CurrentViewModel = _serviceProvider.GetRequiredService<ContentManagementViewModel>();
    }
}
