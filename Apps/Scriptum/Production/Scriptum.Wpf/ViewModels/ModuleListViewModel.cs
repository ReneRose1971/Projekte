using System;
using System.Collections.ObjectModel;
using PropertyChanged;
using Scriptum.Wpf.Navigation;
using Scriptum.Wpf.Projections;
using Scriptum.Wpf.Projections.Services;

namespace Scriptum.Wpf.ViewModels;

/// <summary>
/// ViewModel für die Modul-Liste.
/// </summary>
[AddINotifyPropertyChangedInterface]
public sealed class ModuleListViewModel
{
    private readonly INavigationService _navigationService;
    private readonly IContentQueryService _contentQuery;

    public ModuleListViewModel(
        INavigationService navigationService,
        IContentQueryService contentQuery)
    {
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _contentQuery = contentQuery ?? throw new ArgumentNullException(nameof(contentQuery));

        Modules = new ObservableCollection<ModuleListItem>();
        
        _ = LoadModulesAsync();
    }

    public ObservableCollection<ModuleListItem> Modules { get; }

    public void SelectModule(ModuleListItem module)
    {
        if (module == null) return;
        _navigationService.NavigateToLessonList(module.ModuleId);
    }

    public void NavigateBack()
    {
        _navigationService.NavigateToHome();
    }

    private async System.Threading.Tasks.Task LoadModulesAsync()
    {
        try
        {
            var modules = await _contentQuery.GetModulesAsync();
            
            Modules.Clear();
            foreach (var module in modules)
            {
                Modules.Add(module);
            }
        }
        catch
        {
            // Defensive: Bei Fehler einfach leer lassen
        }
    }
}
