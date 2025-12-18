using System;
using System.Collections.ObjectModel;
using PropertyChanged;
using Scriptum.Wpf.Navigation;
using Scriptum.Wpf.Projections.Models;
using Scriptum.Wpf.Projections.Services;

namespace Scriptum.Wpf.ViewModels;

/// <summary>
/// ViewModel für die Session-Details.
/// </summary>
[AddINotifyPropertyChangedInterface]
public sealed class SessionDetailViewModel
{
    private readonly INavigationService _navigationService;
    private readonly ISessionQueryService _sessionQuery;

    public SessionDetailViewModel(
        INavigationService navigationService,
        ISessionQueryService sessionQuery)
    {
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _sessionQuery = sessionQuery ?? throw new ArgumentNullException(nameof(sessionQuery));
        
        Events = new ObservableCollection<SessionEventRow>();
        Errors = new ObservableCollection<SessionErrorRow>();
    }

    public int SessionId { get; private set; }
    public string LessonTitle { get; private set; } = string.Empty;
    public string ModuleTitle { get; private set; } = string.Empty;
    public DateTime StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public int TotalInputs { get; private set; }
    public int TotalErrors { get; private set; }
    public double Accuracy { get; private set; }
    public string Duration { get; private set; } = string.Empty;
    
    public ObservableCollection<SessionEventRow> Events { get; }
    public ObservableCollection<SessionErrorRow> Errors { get; }

    public void Initialize(int sessionId)
    {
        SessionId = sessionId;
        _ = LoadDetailsAsync(sessionId);
    }

    public void NavigateBack()
    {
        _navigationService.NavigateToSessionHistory();
    }

    private async System.Threading.Tasks.Task LoadDetailsAsync(int sessionId)
    {
        try
        {
            var details = await _sessionQuery.GetSessionDetailAsync(sessionId);
            
            if (details != null)
            {
                LessonTitle = details.Header.LessonTitle;
                ModuleTitle = details.Header.ModuleTitle;
                StartedAt = details.Header.StartedAt;
                CompletedAt = details.Header.CompletedAt;
                TotalInputs = details.Metrics.TotalInputs;
                TotalErrors = details.Metrics.TotalErrors;
                Accuracy = details.Metrics.Accuracy;
                Duration = details.Metrics.Duration?.ToString(@"mm\:ss") ?? "-";
                
                Events.Clear();
                foreach (var evt in details.Events)
                {
                    Events.Add(evt);
                }
                
                Errors.Clear();
                foreach (var error in details.Errors)
                {
                    Errors.Add(error);
                }
            }
            else
            {
                LessonTitle = "Session nicht gefunden";
                ModuleTitle = string.Empty;
            }
        }
        catch
        {
            LessonTitle = "Fehler beim Laden";
            ModuleTitle = string.Empty;
        }
    }
}
