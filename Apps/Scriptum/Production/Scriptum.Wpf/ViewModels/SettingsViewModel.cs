using System;
using PropertyChanged;
using Scriptum.Wpf.Navigation;

namespace Scriptum.Wpf.ViewModels;

/// <summary>
/// ViewModel für die Einstellungen.
/// </summary>
[AddINotifyPropertyChangedInterface]
public sealed class SettingsViewModel
{
    private readonly INavigationService _navigationService;

    public SettingsViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
    }

    public string SettingsText { get; private set; } = "TODO: UI-Optionen (Theme, Sounds, etc.)";

    public void NavigateBack()
    {
        _navigationService.NavigateToHome();
    }
}
