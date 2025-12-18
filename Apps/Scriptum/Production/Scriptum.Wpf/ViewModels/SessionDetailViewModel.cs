using System;
using PropertyChanged;
using Scriptum.Wpf.Navigation;

namespace Scriptum.Wpf.ViewModels;

/// <summary>
/// ViewModel für die Session-Details.
/// </summary>
[AddINotifyPropertyChangedInterface]
public sealed class SessionDetailViewModel
{
    private readonly INavigationService _navigationService;

    public SessionDetailViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
    }

    public string SessionId { get; private set; } = string.Empty;
    public string DetailText { get; private set; } = "TODO: Session-Details anzeigen (Inputs/Evaluations)";

    public void Initialize(string sessionId)
    {
        SessionId = sessionId;
    }

    public void NavigateBack()
    {
        _navigationService.NavigateToSessionHistory();
    }
}
