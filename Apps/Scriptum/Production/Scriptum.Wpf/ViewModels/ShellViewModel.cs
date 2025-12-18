using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using PropertyChanged;
using Scriptum.Wpf.Navigation;

namespace Scriptum.Wpf.ViewModels;

/// <summary>
/// Shell-ViewModel: Verwaltet die Navigation zwischen Views.
/// </summary>
[AddINotifyPropertyChangedInterface]
public sealed class ShellViewModel
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Stack<ViewState> _history = new();

    public ShellViewModel(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        Title = "Scriptum Discipulus";
        Subtitle = "DE-QWERTZ Training";
        
        InitializeNavigation();
        UpdateBreadcrumbs("Home");
        UpdateStatusBar("Bereit");
    }

    public string Title { get; }
    public string Subtitle { get; }
    public string AppVersion => "Version 1.0.0";

    public object? CurrentViewModel { get; private set; }
    public string CurrentSectionTitle { get; private set; } = "Home";
    
    public ObservableCollection<BreadcrumbItem> Breadcrumbs { get; } = new();
    public ObservableCollection<NavigationItem> PrimaryNavigation { get; } = new();
    public ObservableCollection<NavigationItem> SecondaryNavigation { get; } = new();
    
    public bool CanGoBack => _history.Count > 0;
    public string SessionStatus { get; private set; } = "Keine aktive Session";
    public string StatusMessage { get; private set; } = "Bereit";
    public string CurrentTime { get; private set; } = DateTime.Now.ToString("HH:mm");

    private void InitializeNavigation()
    {
        PrimaryNavigation.Add(new NavigationItem("home", "Home", ShowHome, "\uE10F"));
        PrimaryNavigation.Add(new NavigationItem("modules", "Module", ShowModuleList, "\uE8F1"));
        PrimaryNavigation.Add(new NavigationItem("sessions", "Sitzungen", ShowSessionHistory, "\uE81C"));
        PrimaryNavigation.Add(new NavigationItem("statistics", "Statistik", ShowStatisticsDashboard, "\uE9D9"));
        
        SecondaryNavigation.Add(new NavigationItem("settings", "Einstellungen", ShowSettings, "\uE713"));
        SecondaryNavigation.Add(new NavigationItem("content", "Inhalte", ShowContentManagement, "\uE8B7"));
        
        PrimaryNavigation[0].IsSelected = true;
    }

    private void SetActiveNavigation(string key)
    {
        foreach (var item in PrimaryNavigation)
            item.IsSelected = item.Key == key;
        foreach (var item in SecondaryNavigation)
            item.IsSelected = item.Key == key;
    }

    private void UpdateBreadcrumbs(params string[] titles)
    {
        Breadcrumbs.Clear();
        foreach (var title in titles)
        {
            Breadcrumbs.Add(new BreadcrumbItem(title));
        }
    }

    private void UpdateStatusBar(string message, string? sessionStatus = null)
    {
        StatusMessage = message;
        if (sessionStatus != null)
            SessionStatus = sessionStatus;
        CurrentTime = DateTime.Now.ToString("HH:mm");
    }

    private void PushHistory(string title, params string[] breadcrumbTitles)
    {
        if (CurrentViewModel != null)
        {
            var breadcrumbs = Breadcrumbs.Select(b => new BreadcrumbItem(b.Title, b.NavigateAction)).ToArray();
            _history.Push(new ViewState(CurrentViewModel, CurrentSectionTitle, breadcrumbs));
        }
    }

    public void GoBack()
    {
        if (!CanGoBack) return;

        var state = _history.Pop();
        CurrentViewModel = state.ViewModel;
        CurrentSectionTitle = state.Title;
        
        Breadcrumbs.Clear();
        foreach (var breadcrumb in state.Breadcrumbs)
            Breadcrumbs.Add(breadcrumb);
    }

    public void ShowHome()
    {
        _history.Clear();
        CurrentViewModel = _serviceProvider.GetRequiredService<HomeViewModel>();
        CurrentSectionTitle = "Home";
        SetActiveNavigation("home");
        UpdateBreadcrumbs("Home");
        UpdateStatusBar("Bereit");
    }

    public void ShowModuleList()
    {
        _history.Clear();
        CurrentViewModel = _serviceProvider.GetRequiredService<ModuleListViewModel>();
        CurrentSectionTitle = "Module";
        SetActiveNavigation("modules");
        UpdateBreadcrumbs("Module");
        UpdateStatusBar("Module geladen");
    }

    public void ShowLessonList(string moduleId)
    {
        PushHistory(CurrentSectionTitle);
        var vm = _serviceProvider.GetRequiredService<LessonListViewModel>();
        vm.Initialize(moduleId);
        CurrentViewModel = vm;
        CurrentSectionTitle = "Lektionen";
        SetActiveNavigation("modules");
        UpdateBreadcrumbs("Module", "Lektionen");
        UpdateStatusBar("Lektionen geladen");
    }

    public void ShowLessonDetails(string moduleId, string lessonId)
    {
        PushHistory(CurrentSectionTitle);
        var vm = _serviceProvider.GetRequiredService<LessonDetailsViewModel>();
        vm.Initialize(moduleId, lessonId);
        CurrentViewModel = vm;
        CurrentSectionTitle = "Lektionsdetails";
        SetActiveNavigation("modules");
        UpdateBreadcrumbs("Module", "Lektionen", "Details");
        UpdateStatusBar("Lektionsdetails geladen");
    }

    public void ShowLessonGuide(string lessonId)
    {
        PushHistory(CurrentSectionTitle);
        var vm = _serviceProvider.GetRequiredService<LessonGuideViewModel>();
        vm.Initialize(lessonId);
        CurrentViewModel = vm;
        CurrentSectionTitle = "Lektionsanleitung";
        SetActiveNavigation("modules");
        UpdateBreadcrumbs("Module", "Lektionen", "Anleitung");
        UpdateStatusBar("Anleitung geladen");
    }

    public void ShowTraining(string moduleId, string lessonId)
    {
        PushHistory(CurrentSectionTitle);
        var vm = _serviceProvider.GetRequiredService<TrainingViewModel>();
        vm.Initialize(moduleId, lessonId);
        CurrentViewModel = vm;
        CurrentSectionTitle = "Training";
        SetActiveNavigation("modules");
        UpdateBreadcrumbs("Module", "Lektionen", "Training");
        UpdateStatusBar("Training läuft", "Session: Aktiv");
    }

    public void ShowTrainingSummary(int? sessionId = null)
    {
        PushHistory(CurrentSectionTitle);
        var vm = _serviceProvider.GetRequiredService<TrainingSummaryViewModel>();
        vm.Initialize(sessionId);
        CurrentViewModel = vm;
        CurrentSectionTitle = "Trainingsauswertung";
        SetActiveNavigation("modules");
        UpdateBreadcrumbs("Module", "Training", "Auswertung");
        UpdateStatusBar("Auswertung angezeigt", "Session: Abgeschlossen");
    }

    public void ShowSessionHistory()
    {
        _history.Clear();
        CurrentViewModel = _serviceProvider.GetRequiredService<SessionHistoryViewModel>();
        CurrentSectionTitle = "Sitzungsverlauf";
        SetActiveNavigation("sessions");
        UpdateBreadcrumbs("Sitzungen");
        UpdateStatusBar("Sitzungen geladen");
    }

    public void ShowSessionDetail(int sessionId)
    {
        PushHistory(CurrentSectionTitle);
        var vm = _serviceProvider.GetRequiredService<SessionDetailViewModel>();
        vm.Initialize(sessionId);
        CurrentViewModel = vm;
        CurrentSectionTitle = "Sitzungsdetails";
        SetActiveNavigation("sessions");
        UpdateBreadcrumbs("Sitzungen", "Details");
        UpdateStatusBar("Sitzungsdetails geladen");
    }

    public void ShowStatisticsDashboard()
    {
        _history.Clear();
        CurrentViewModel = _serviceProvider.GetRequiredService<StatisticsDashboardViewModel>();
        CurrentSectionTitle = "Statistik";
        SetActiveNavigation("statistics");
        UpdateBreadcrumbs("Statistik");
        UpdateStatusBar("Statistik geladen");
    }

    public void ShowErrorHeatmap()
    {
        PushHistory(CurrentSectionTitle);
        CurrentViewModel = _serviceProvider.GetRequiredService<ErrorHeatmapViewModel>();
        CurrentSectionTitle = "Fehler-Heatmap";
        SetActiveNavigation("statistics");
        UpdateBreadcrumbs("Statistik", "Heatmap");
        UpdateStatusBar("Heatmap geladen");
    }

    public void ShowSettings()
    {
        _history.Clear();
        CurrentViewModel = _serviceProvider.GetRequiredService<SettingsViewModel>();
        CurrentSectionTitle = "Einstellungen";
        SetActiveNavigation("settings");
        UpdateBreadcrumbs("Einstellungen");
        UpdateStatusBar("Einstellungen geladen");
    }

    public void ShowContentManagement()
    {
        _history.Clear();
        CurrentViewModel = _serviceProvider.GetRequiredService<ContentManagementViewModel>();
        CurrentSectionTitle = "Inhalte verwalten";
        SetActiveNavigation("content");
        UpdateBreadcrumbs("Inhalte");
        UpdateStatusBar("Inhaltsverwaltung geladen");
    }
}
