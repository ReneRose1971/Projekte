using System;
using PropertyChanged;
using Scriptum.Wpf.Navigation;

namespace Scriptum.Wpf.ViewModels;

/// <summary>
/// ViewModel für die Error-Heatmap.
/// </summary>
[AddINotifyPropertyChangedInterface]
public sealed class ErrorHeatmapViewModel
{
    private readonly INavigationService _navigationService;

    public ErrorHeatmapViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
    }

    public string HeatmapText { get; private set; } = "TODO: Error-Heatmap-Visualisierung";

    public void NavigateBack()
    {
        _navigationService.NavigateToStatisticsDashboard();
    }
}
