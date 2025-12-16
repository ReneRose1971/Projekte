// src/TypeTutor.WPF/LessonListViewModel.cs
using System;
using System.Threading.Tasks;
using TypeTutor.Logic.Core;
using System.Collections.Generic;
using System.Linq;
using CustomWPFControls.ViewModels;
using System.ComponentModel;

namespace TypeTutor.WPF
{
    /// <summary>
    /// ViewModel für die Lesson-Liste. Erweitert die allgemeine CollectionViewModel-Implementierung
    /// und bietet zusätzliche UI-spezifische Eigenschaften (Filter, Module, Mehrfachauswahl).
    /// </summary>
    public sealed class LessonListViewModel : CollectionViewModel<Lesson>
    {
        private readonly ILessonRepository _repo;

        // New: expose multiple selection
        private IReadOnlyList<Lesson> _selectedItems = Array.Empty<Lesson>();
        public IReadOnlyList<Lesson> SelectedItems
        {
            get => _selectedItems;
            set { _selectedItems = value; OnPropertyChanged(nameof(SelectedItems)); }
        }

        // Modules for filtering
        public System.Collections.ObjectModel.ObservableCollection<string> Modules { get; } = new();

        private string? _selectedModuleFilter;
        public string? SelectedModuleFilter
        {
            get => _selectedModuleFilter;
            set { if (_selectedModuleFilter == value) return; _selectedModuleFilter = value; OnPropertyChanged(); }
        }

        public RelayCommand CmdReload { get; }

        public LessonListViewModel(ILessonRepository repo)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            CmdReload = new RelayCommand(async () => await ReloadAsync());
            // Do not auto-start ReloadAsync in constructor to avoid implicit background work
            // during test construction which can cause synchronization issues in parallel runs.
            // Callers (views) should invoke ReloadAsync explicitly when ready.
        }

        public async Task ReloadAsync()
        {
            Items.Clear();
            var list = await _repo.LoadAllAsync();
            foreach (var l in list) Items.Add(l);

            // populate modules list (distinct non-empty ModuleId)
            Modules.Clear();
            var modules = list.Select(l => l.Meta.ModuleId)
                              .Where(s => !string.IsNullOrWhiteSpace(s))
                              .Select(s => s!.Trim())
                              .Distinct(StringComparer.OrdinalIgnoreCase)
                              .OrderBy(s => s, StringComparer.CurrentCultureIgnoreCase);
            foreach (var m in modules) Modules.Add(m);

            if (Items.Count > 0) SelectedItem = Items[0];
        }

        // Helper used by view to set multiple selection
        public void UpdateSelectionFromView(IEnumerable<object>? selected)
        {
            if (selected is null)
            {
                SelectedItems = Array.Empty<Lesson>();
                return;
            }

            var items = new List<Lesson>();
            foreach (var o in selected)
            {
                if (o is Lesson l) items.Add(l);
            }

            SelectedItems = items;
        }

        // no forwarding; use base PropertyChanged and OnPropertyChanged
    }
}
