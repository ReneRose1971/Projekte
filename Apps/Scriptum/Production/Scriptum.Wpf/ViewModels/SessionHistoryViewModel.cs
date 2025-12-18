using System;
using System.Collections.ObjectModel;
using PropertyChanged;
using Scriptum.Wpf.Navigation;
using Scriptum.Wpf.Projections;

namespace Scriptum.Wpf.ViewModels;

/// <summary>
/// ViewModel für die Session-Verlaufsansicht.
/// </summary>
[AddINotifyPropertyChangedInterface]
public sealed class SessionHistoryViewModel
{
    private readonly INavigationService _navigationService;

    public SessionHistoryViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        
        Sessions = new ObservableCollection<SessionListItem>();
    }

    public ObservableCollection<SessionListItem> Sessions { get; }

    public void SelectSession(SessionListItem session)
    {
        if (session == null) return;
        _navigationService.NavigateToSessionDetail(session.SessionId);
    }

    public void NavigateBack()
    {
        _navigationService.NavigateToHome();
    }
}
