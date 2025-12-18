using System;
using System.Collections.ObjectModel;
using PropertyChanged;
using Scriptum.Wpf.Navigation;
using Scriptum.Wpf.Projections.Models;
using Scriptum.Wpf.Projections.Services;

namespace Scriptum.Wpf.ViewModels;

/// <summary>
/// ViewModel für das Statistik-Dashboard.
/// </summary>
[AddINotifyPropertyChangedInterface]
public sealed class StatisticsDashboardViewModel
{
    private readonly INavigationService _navigationService;
    private readonly IStatisticsQueryService _statisticsQuery;

    public StatisticsDashboardViewModel(
        INavigationService navigationService,
        IStatisticsQueryService statisticsQuery)
    {
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _statisticsQuery = statisticsQuery ?? throw new ArgumentNullException(nameof(statisticsQuery));
        
        ModuleStats = new ObservableCollection<ModuleStatRow>();
        LessonStats = new ObservableCollection<LessonStatRow>();
        
        _ = LoadStatisticsAsync();
    }

    public ObservableCollection<ModuleStatRow> ModuleStats { get; }
    public ObservableCollection<LessonStatRow> LessonStats { get; }

    public void NavigateBack()
    {
        _navigationService.NavigateToHome();
    }

    private async System.Threading.Tasks.Task LoadStatisticsAsync()
    {
        try
        {
            var filter = new StatisticsFilter(null, null, null, null);
            var dashboard = await _statisticsQuery.BuildDashboardAsync(filter);
            
            ModuleStats.Clear();
            foreach (var stat in dashboard.Modules)
            {
                ModuleStats.Add(stat);
            }
            
            LessonStats.Clear();
            foreach (var stat in dashboard.Lessons)
            {
                LessonStats.Add(stat);
            }
        }
        catch
        {
            // Defensive: Bei Fehler leer lassen
        }
    }
}
