using System.Threading.Tasks;
using TypeTutor.Logic.Core;
using TypeTutor.Logic.Data;
using System.Windows;
using Microsoft.Win32;
using System.Linq;
using System.Text.Json;
using CustomWPFControls.ViewModels;

namespace TypeTutor.WPF
{
    /// <summary>
    /// ViewModel für ModuleGuides. Erbt von CollectionViewModel, um Standard-Collection-Funktionalität
    /// wiederzuverwenden.
    /// </summary>
    public sealed class ModuleGuideViewModel : CollectionViewModel<ModuleGuide>
    {
        private readonly ModuleGuideRepository _repo;
        private readonly ILessonRepository _lessonRepo;

        public RelayCommand CmdImport { get; }

        public ModuleGuideViewModel(ModuleGuideRepository repo, ILessonRepository lessonRepo)
        {
            _repo = repo;
            _lessonRepo = lessonRepo;
            CmdImport = new RelayCommand(async () => await ImportAsync());
            _ = LoadAsync();
        }

        public async Task LoadAsync()
        {
            Items.Clear();
            var all = (await _repo.LoadAllAsync()).ToList();

            // Ensure ModuleGuides exist for all ModuleIds referenced by Lessons
            var lessons = await _lessonRepo.LoadAllAsync();
            var moduleIds = lessons.Select(l => l.Meta.ModuleId).Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s!.Trim()).Distinct(System.StringComparer.OrdinalIgnoreCase);

            bool createdAny = false;
            foreach (var mid in moduleIds)
            {
                if (!all.Any(m => string.Equals(m.Title, mid, System.StringComparison.OrdinalIgnoreCase)))
                {
                    // create empty placeholder and persist
                    var empty = new ModuleGuide(mid) { BodyMarkDown = string.Empty };
                    try { await _repo.SaveAsync(empty, overwrite: true); createdAny = true; }
                    catch { /* ignore write errors */ }
                }
            }

            if (createdAny)
            {
                all = (await _repo.LoadAllAsync()).ToList();
            }

            foreach (var m in all) Items.Add(m);
            if (Items.Count > 0) SelectedItem = Items[0];
        }

        private async Task ImportAsync()
        {
            var dlg = new OpenFileDialog() { Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*" };
            if (dlg.ShowDialog() != true) return;
            string json;
            try { json = await System.IO.File.ReadAllTextAsync(dlg.FileName); }
            catch (System.Exception ex) { MessageBox.Show("Konnte Datei nicht lesen: " + ex.Message); return; }

            ModuleGuide[] candidates;
            try
            {
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                JsonElement arrElem;
                if (root.ValueKind == JsonValueKind.Array)
                {
                    arrElem = root;
                }
                else if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty("ModuleGuides", out var prop) && prop.ValueKind == JsonValueKind.Array)
                {
                    arrElem = prop;
                }
                else
                {
                    MessageBox.Show("Ungültiges Format: JSON muss ein Array oder ein Objekt mit 'ModuleGuides' enthalten.");
                    return;
                }

                var list = new System.Collections.Generic.List<ModuleGuide>();
                foreach (var item in arrElem.EnumerateArray())
                {
                    try
                    {
                        var mg = JsonSerializer.Deserialize<ModuleGuide>(item.GetRawText(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        if (mg is not null && !string.IsNullOrWhiteSpace(mg.Title))
                            list.Add(mg);
                    }
                    catch { /* ignore invalid entry */ }
                }

                candidates = list.ToArray();
            }
            catch
            {
                MessageBox.Show("Ungültiges JSON-Format");
                return;
            }

            if (candidates.Length == 0) { MessageBox.Show("Keine gültigen ModuleGuide Einträge gefunden."); return; }

            // Filter: only import guides that have at least one lesson assigned to this module id
            var existingLessons = (await _lessonRepo.LoadAllAsync()).Select(l => l.Meta.ModuleId).Where(s => !string.IsNullOrWhiteSpace(s)).ToHashSet(System.StringComparer.OrdinalIgnoreCase);
            var toImport = candidates.Where(c => existingLessons.Contains(c.Title)).ToArray();

            if (toImport.Length == 0)
            {
                MessageBox.Show("Keine ModuleGuides in der Datei entsprechen vorhandenen Lessons (ModuleID fehlt).\nHinweis: Die Datei enthält ModuleGuides, aber keine ModuleId stimmt mit vorhandenen Lessons überein.");
                return;
            }

            foreach (var mg in toImport)
            {
                var existing = await _repo.LoadAsync(mg.Title);
                if (existing != null)
                {
                    var res = MessageBox.Show($"Module '{mg.Title}' existiert bereits. Überschreiben?", "Überschreiben bestätigen", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                    if (res == MessageBoxResult.Cancel) break;
                    if (res == MessageBoxResult.No) continue;
                }

                await _repo.SaveAsync(mg, overwrite: true);
            }

            await LoadAsync();
            MessageBox.Show("Import abgeschlossen.");
        }
    }
}
