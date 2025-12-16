// src/TypeTutor.WPF/LessonEditorService.cs
using System;
using System.Windows;

namespace TypeTutor.WPF
{
    public sealed class LessonEditorService : ILessonEditorService
    {
        private readonly Func<LessonEditorWindow> _factory;
        private LessonEditorWindow? _current;

        public LessonEditorService(Func<LessonEditorWindow> factory) => _factory = factory;

        public void ShowEditor()
        {
            if (_current is not null && _current.IsVisible)
            {
                try { _current.Activate(); _current.Focus(); } catch { }
                return;
            }

            var win = _factory();
            _current = win;
            win.Owner = Application.Current?.MainWindow;
            win.Closed += (s, e) => { if (ReferenceEquals(_current, win)) _current = null; };
            win.Show();
        }
    }
}
