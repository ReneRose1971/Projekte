// src/TypeTutor.WPF/LessonEditorViewModel.cs
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TypeTutor.Logic.Core;
using Microsoft.Win32;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Collections.Generic;
using TypeTutor.Logic.Data; // <- Hier das missing using für LessonStorageOptions rein

namespace TypeTutor.WPF
{
    /// <summary>
    /// Editor-VM: links die LessonList, rechts Eingabefelder, Create/Delete.
    /// </summary>
    public sealed class LessonEditorViewModel : INotifyPropertyChanged
    {
        private readonly ILessonRepository _repo;
        private readonly ILessonFactory _factory;

        public event PropertyChangedEventHandler? PropertyChanged;

        public LessonListViewModel ListVM { get; }

        // Eingabefelder (rechts)
        // Id wurde entfernt; LessonMetaData verwendet nur noch Title
        private string _title = "";
        public string Title { get => _title; set { if (_title == value) return; _title = value; OnPropertyChanged(); } }

        private string _description = "";
        public string Description { get => _description; set { if (_description == value) return; _description = value; OnPropertyChanged(); } }

        private int _difficulty = 1;
        public int Difficulty { get => _difficulty; set { if (_difficulty == value) return; _difficulty = value; OnPropertyChanged(); } }

        private string _tags = ""; // komma-separiert
        public string Tags { get => _tags; set { if (_tags == value) return; _tags = value; OnPropertyChanged(); } }

        private string _text = "";
        public string Text { get => _text; set { if (_text == value) return; _text = value; OnPropertyChanged(); } }

        private string _moduleId = "";
        public string ModuleId { get => _moduleId; set { if (_moduleId == value) return; _moduleId = value; OnPropertyChanged(); } }

        public RelayCommand CmdCreate { get; }
        public RelayCommand CmdDelete { get; }
        public RelayCommand CmdLoadFromSelection { get; }
        public RelayCommand CmdImport { get; }

        public LessonEditorViewModel(ILessonRepository repo, ILessonFactory factory, LessonListViewModel listVm)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            ListVM = listVm ?? throw new ArgumentNullException(nameof(listVm));

            CmdCreate = new RelayCommand(async () => await CreateAsync());
            CmdDelete = new RelayCommand(async () => await DeleteAsync(), () => (ListVM.SelectedItem != null) || (ListVM.SelectedItems?.Count > 0));
            CmdLoadFromSelection = new RelayCommand(async () => await LoadSelectionAsync());
            CmdImport = new RelayCommand(async () => await ImportJsonAsync());

            // Enable/Disable bei Selektion
            ListVM.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(ListVM.SelectedItem) || e.PropertyName == nameof(ListVM.SelectedItems))
                    CmdDelete.RaiseCanExecuteChanged();
            };
        }

        // using System;  // für StringSplitOptions

        private async Task CreateAsync()
        {
            // minimale Validierung (optional ausbauen)
            if (string.IsNullOrWhiteSpace(Title)) throw new ArgumentException("Title darf nicht leer sein.", nameof(Title));

            var tags = Tags.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            var meta = new LessonMetaData(Title)
            {
                Description = Description ?? "",
                Difficulty = Difficulty,
                Tags = tags,
                ModuleId = ModuleId
            };

            // Persistieren (Factory/Repo übernehmen Normalisierung/Blocks)
            await _repo.CreateLessonAsync(meta, Text, overwrite: false); // Signatur siehe ILessonRepository. :contentReference[oaicite:1]{index=1}

            // Ensure ModuleGuide exists for ModuleId (create empty if missing)
            if (!string.IsNullOrWhiteSpace(ModuleId))
            {
                try
                {
                    var options = LessonStorageOptions.CreateDefault();
                    var mgRepo = new TypeTutor.Logic.Data.ModuleGuideRepository(options);
                    var existing = await mgRepo.LoadAsync(ModuleId);
                    if (existing is null)
                    {
                        var empty = new TypeTutor.Logic.Core.ModuleGuide(ModuleId) { BodyMarkDown = string.Empty };
                        await mgRepo.SaveAsync(empty, overwrite: true);
                    }
                }
                catch { /* ignore */ }
            }

            await ListVM.ReloadAsync();
        }

        private async Task DeleteAsync()
        {
            // Collect targets: either multiple selected items or single SelectedItem
            var toDelete = new List<string>();

            if (ListVM.SelectedItems != null && ListVM.SelectedItems.Count > 0)
            {
                foreach (var l in ListVM.SelectedItems)
                {
                    if (l is Lesson les) toDelete.Add(les.Meta.Title);
                }
            }
            else if (ListVM.SelectedItem != null)
            {
                toDelete.Add(ListVM.SelectedItem.Meta.Title);
            }

            if (toDelete.Count == 0) return;

            var message = toDelete.Count == 1 ? $"Lektion '{toDelete[0]}' wirklich löschen?" : $"{toDelete.Count} Lektionen wirklich löschen?";
            var res = MessageBox.Show(message, "Löschen bestätigen", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (res != MessageBoxResult.Yes) return;

            var failed = new List<string>();

            foreach (var title in toDelete)
            {
                try
                {
                    var ok = await _repo.DeleteLessonAsync(title);
                    if (!ok) failed.Add(title);
                }
                catch (Exception ex)
                {
                    // collect failure but keep going
                    failed.Add(title + $" (Fehler: {ex.Message})");
                }
            }

            // Refresh list regardless of failures to reflect any successful deletions
            await ListVM.ReloadAsync();

            // Update command state
            CmdDelete.RaiseCanExecuteChanged();

            if (failed.Count > 0)
            {
                var failMsg = "Die folgenden Lektionen konnten nicht gelöscht werden:\n" + string.Join("\n", failed);
                MessageBox.Show(failMsg, "Löschen fehlgeschlagen", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadSelectionAsync()
        {
            if (ListVM.SelectedItem is null) return;
            // Eingabefelder mit der Auswahl füllen
            var l = ListVM.SelectedItem;
            // Id removed — only fill Title and other fields
            Title = l.Meta.Title;
            Description = l.Meta.Description ?? "";
            Difficulty = l.Meta.Difficulty;
            Tags = string.Join(", ", l.Meta.Tags ?? Array.Empty<string>());
            Text = l.TargetText;
            ModuleId = l.Meta.ModuleId ?? string.Empty;
            await Task.CompletedTask;
        }

        private async Task ImportJsonAsync()
        {
            var dlg = new OpenFileDialog()
            {
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                Title = "Lesson JSON wählen"
            };

            if (dlg.ShowDialog() != true)
                return;

            var path = dlg.FileName;
            string status = "";

            try
            {
                var json = await File.ReadAllTextAsync(path);
                var doc = JsonDocument.Parse(json);

                if (!doc.RootElement.TryGetProperty("Lessons", out var lessonsElem) || lessonsElem.ValueKind != JsonValueKind.Array)
                {
                    MessageBox.Show("Ungültiges Format: 'Lessons' nicht gefunden.", "Import Fehler", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                int imported = 0;
                int skipped = 0;

                foreach (var item in lessonsElem.EnumerateArray())
                {
                    var title = JsonExtensions.GetPropertyOrDefault(item, "Title", "");
                    if (string.IsNullOrWhiteSpace(title)) { skipped++; continue; }

                    var existing = await _repo.LoadLessonAsync(title);
                    if (existing != null) { skipped++; continue; }

                    var description = JsonExtensions.GetPropertyOrDefault(item, "Description", "");
                    var difficulty = JsonExtensions.GetPropertyIntOrDefault(item, "Difficulty", 1);
                    var moduleId = JsonExtensions.GetPropertyOrDefault(item, "ModuleId", string.Empty);
                    var tags = JsonExtensions.GetPropertyArrayOrDefault(item, "Tags");
                    var content = JsonExtensions.GetPropertyOrDefault(item, "Content", string.Empty);

                    var meta = new LessonMetaData(title)
                    {
                        Description = description,
                        Difficulty = difficulty,
                        Tags = tags,
                        ModuleId = moduleId
                    };

                    await _repo.CreateLessonAsync(meta, content, overwrite: false);
                    imported++;
                }

                await ListVM.ReloadAsync();
                status = $"Importiert: {imported}, übersprungen: {skipped}";
                MessageBox.Show(status, "Import abgeschlossen", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fehler beim Import: " + ex.Message, "Import Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnPropertyChanged([CallerMemberName] string? n = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }

    internal static class JsonExtensions
    {
        public static string GetPropertyOrDefault(this JsonElement el, string name, string def)
        {
            if (el.ValueKind != JsonValueKind.Object) return def;
            if (!el.TryGetProperty(name, out var p)) return def;
            if (p.ValueKind == JsonValueKind.String) return p.GetString() ?? def;
            return def;
        }

        public static int GetPropertyIntOrDefault(this JsonElement el, string name, int def)
        {
            if (el.ValueKind != JsonValueKind.Object) return def;
            if (!el.TryGetProperty(name, out var p)) return def;
            if (p.ValueKind == JsonValueKind.Number && p.TryGetInt32(out var v)) return v;
            return def;
        }

        public static string[] GetPropertyArrayOrDefault(this JsonElement el, string name)
        {
            if (el.ValueKind != JsonValueKind.Object) return Array.Empty<string>();
            if (!el.TryGetProperty(name, out var p)) return Array.Empty<string>();
            if (p.ValueKind != JsonValueKind.Array) return Array.Empty<string>();
            var list = new System.Collections.Generic.List<string>();
            foreach (var it in p.EnumerateArray())
            {
                if (it.ValueKind == JsonValueKind.String) list.Add(it.GetString()!);
            }
            return list.ToArray();
        }
    }
}
