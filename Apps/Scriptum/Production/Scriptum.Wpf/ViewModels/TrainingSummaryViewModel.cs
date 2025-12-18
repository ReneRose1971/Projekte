using System;
using PropertyChanged;
using Scriptum.Wpf.Navigation;

namespace Scriptum.Wpf.ViewModels;

/// <summary>
/// ViewModel für die Trainings-Zusammenfassung.
/// </summary>
[AddINotifyPropertyChangedInterface]
public sealed class TrainingSummaryViewModel
{
    private readonly INavigationService _navigationService;

    public TrainingSummaryViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
    }

    public int? SessionId { get; private set; }
    public string SummaryText { get; private set; } = "TODO: Session-Kennzahlen anzeigen";

    public void Initialize(int? sessionId)
    {
        SessionId = sessionId;
    }

    public void NavigateToModules()
    {
        _navigationService.NavigateToModuleList();
    }

    public void NavigateToHome()
    {
        _navigationService.NavigateToHome();
    }
}
