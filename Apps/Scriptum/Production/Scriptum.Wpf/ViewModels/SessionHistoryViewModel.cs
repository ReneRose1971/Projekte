using System;
using System.Collections.ObjectModel;
using PropertyChanged;
using Scriptum.Wpf.Navigation;
using Scriptum.Wpf.Projections;
using Scriptum.Wpf.Projections.Models;
using Scriptum.Wpf.Projections.Services;

namespace Scriptum.Wpf.ViewModels;

/// <summary>
/// ViewModel für die Session-Verlaufsansicht.
/// </summary>
[AddINotifyPropertyChangedInterface]
public sealed class SessionHistoryViewModel
{
    private readonly INavigationService _navigationService;
    private readonly ISessionQueryService _sessionQuery;

    public SessionHistoryViewModel(
        INavigationService navigationService,
        ISessionQueryService sessionQuery)
    {
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _sessionQuery = sessionQuery ?? throw new ArgumentNullException(nameof(sessionQuery));
        
        Sessions = new ObservableCollection<SessionListItem>();
        
        _ = LoadSessionsAsync();
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

    private async System.Threading.Tasks.Task LoadSessionsAsync()
    {
        try
        {
            var filter = new SessionFilter(null, null, null, null, null);
            var sessions = await _sessionQuery.GetSessionsByFilterAsync(filter);
            
            Sessions.Clear();
            foreach (var session in sessions)
            {
                Sessions.Add(session);
            }
        }
        catch
        {
            // Defensive: Bei Fehler leer lassen
        }
    }
}
