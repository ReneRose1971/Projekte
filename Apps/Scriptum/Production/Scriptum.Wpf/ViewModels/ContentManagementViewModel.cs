using System;
using PropertyChanged;
using Scriptum.Wpf.Navigation;

namespace Scriptum.Wpf.ViewModels;

/// <summary>
/// ViewModel für das Content-Management.
/// </summary>
[AddINotifyPropertyChangedInterface]
public sealed class ContentManagementViewModel
{
    private readonly INavigationService _navigationService;

    public ContentManagementViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
    }

    public string ContentInfo { get; private set; } = "TODO: Content-Pfad anzeigen, Reload-Button (disabled)";

    public void NavigateBack()
    {
        _navigationService.NavigateToHome();
    }
}
