// src/TypeTutor.WPF/LessonMenuViewModel.cs
using System;
using System.Windows.Input;
using TypeTutor.Logic.Core;

namespace TypeTutor.WPF
{
    public sealed class LessonMenuViewModel
    {
        private readonly ILessonBrowserService _browser;
        private readonly ILessonEditorService _editor;
        private readonly Func<ModuleGuideWindow>? _moduleGuideWindowFactory;

        public ICommand CmdOpenLessonBrowser { get; }
        public ICommand CmdOpenLessonEditor { get; }
        public ICommand CmdOpenModuleGuideEditor { get; }

        // Host (MainVM) abonniert dieses Event und setzt dann SelectedLesson
        public event EventHandler<Lesson?>? LessonPicked;

        public LessonMenuViewModel(ILessonBrowserService browser, ILessonEditorService editor, Func<ModuleGuideWindow>? moduleGuideWindowFactory = null)
        {
            _browser = browser ?? throw new ArgumentNullException(nameof(browser));
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _moduleGuideWindowFactory = moduleGuideWindowFactory;

            CmdOpenLessonBrowser = new RelayCommand(() =>
            {
                var picked = _browser.ShowDialogAndPick();
                LessonPicked?.Invoke(this, picked);
            });

            CmdOpenLessonEditor = new RelayCommand(() => _editor.ShowEditor());

            CmdOpenModuleGuideEditor = new RelayCommand(() =>
            {
                if (_moduleGuideWindowFactory is null) return;
                try
                {
                    var win = _moduleGuideWindowFactory();
                    win.Owner = System.Windows.Application.Current?.MainWindow;
                    win.Show();
                }
                catch { }
            });
        }
    }
}
