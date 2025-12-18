using System;
using PropertyChanged;
using Scriptum.Wpf.Navigation;

namespace Scriptum.Wpf.ViewModels;

/// <summary>
/// ViewModel für das Statistik-Dashboard.
/// </summary>
[AddINotifyPropertyChangedInterface]
public sealed class StatisticsDashboardViewModel
{
    private readonly INavigationService _navigationService;

    public StatisticsDashboardViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
    }

    public string StatsText { get; private set; } = "TODO: Aggregierte Statistiken anzeigen";

    public void NavigateBack()
    {
        _navigationService.NavigateToHome();
    }
}
