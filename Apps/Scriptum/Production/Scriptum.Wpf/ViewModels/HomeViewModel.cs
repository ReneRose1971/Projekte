using System;
using PropertyChanged;
using Scriptum.Wpf.Navigation;

namespace Scriptum.Wpf.ViewModels;

/// <summary>
/// ViewModel für die Home-Ansicht.
/// </summary>
[AddINotifyPropertyChangedInterface]
public sealed class HomeViewModel
{
    private readonly INavigationService _navigationService;

    public HomeViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
    }

    public void NavigateToModules()
    {
        _navigationService.NavigateToModuleList();
    }

    public void NavigateToStatistics()
    {
        _navigationService.NavigateToStatisticsDashboard();
    }

    public void NavigateToSettings()
    {
        _navigationService.NavigateToSettings();
    }

    public void NavigateToLastSession()
    {
        _navigationService.NavigateToSessionHistory();
    }
}
