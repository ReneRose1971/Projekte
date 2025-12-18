using System;
using System.Collections.ObjectModel;
using PropertyChanged;
using Scriptum.Wpf.Navigation;
using Scriptum.Wpf.Projections.Models;
using Scriptum.Wpf.Projections.Services;

namespace Scriptum.Wpf.ViewModels;

/// <summary>
/// ViewModel für die Error-Heatmap.
/// </summary>
[AddINotifyPropertyChangedInterface]
public sealed class ErrorHeatmapViewModel
{
    private readonly INavigationService _navigationService;
    private readonly IStatisticsQueryService _statisticsQuery;

    public ErrorHeatmapViewModel(
        INavigationService navigationService,
        IStatisticsQueryService statisticsQuery)
    {
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _statisticsQuery = statisticsQuery ?? throw new ArgumentNullException(nameof(statisticsQuery));
        
        ErrorRows = new ObservableCollection<ErrorHeatmapRow>();
        
        _ = LoadHeatmapAsync();
    }

    public string HintText { get; private set; } = string.Empty;
    public ObservableCollection<ErrorHeatmapRow> ErrorRows { get; }

    public void NavigateBack()
    {
        _navigationService.NavigateToStatisticsDashboard();
    }

    private async System.Threading.Tasks.Task LoadHeatmapAsync()
    {
        try
        {
            var filter = new StatisticsFilter(null, null, null, null);
            var heatmap = await _statisticsQuery.BuildErrorHeatmapAsync(filter);
            
            HintText = heatmap.HintText;
            
            ErrorRows.Clear();
            foreach (var row in heatmap.Rows)
            {
                ErrorRows.Add(row);
            }
        }
        catch
        {
            HintText = "Fehler beim Laden der Heatmap";
        }
    }
}
