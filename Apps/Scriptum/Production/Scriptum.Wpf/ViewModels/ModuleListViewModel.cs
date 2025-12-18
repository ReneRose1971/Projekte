using System;
using System.Collections.ObjectModel;
using System.Linq;
using DataToolKit.Abstractions.DataStores;
using PropertyChanged;
using Scriptum.Content.Data;
using Scriptum.Wpf.Navigation;
using Scriptum.Wpf.Projections;

namespace Scriptum.Wpf.ViewModels;

/// <summary>
/// ViewModel für die Modul-Liste.
/// </summary>
[AddINotifyPropertyChangedInterface]
public sealed class ModuleListViewModel
{
    private readonly INavigationService _navigationService;
    private readonly IDataStore<ModuleData> _moduleDataStore;

    public ModuleListViewModel(
        INavigationService navigationService,
        IDataStore<ModuleData> moduleDataStore)
    {
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _moduleDataStore = moduleDataStore ?? throw new ArgumentNullException(nameof(moduleDataStore));

        Modules = new ObservableCollection<ModuleListItem>(
            _moduleDataStore.Items
                .OrderBy(m => m.Order)
                .ThenBy(m => m.Titel)
                .Select(m => new ModuleListItem(m.ModuleId, m.Titel, m.Beschreibung)));
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
}
