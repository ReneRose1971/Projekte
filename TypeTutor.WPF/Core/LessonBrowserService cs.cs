// src/TypeTutor.WPF/LessonBrowserService.cs
using System;
using System.Windows;
using TypeTutor.Logic.Core;

namespace TypeTutor.WPF
{
    public sealed class LessonBrowserService : ILessonBrowserService
    {
        private readonly Func<LessonsWindow> _factory;
        private LessonsWindow? _current;

        public LessonBrowserService(Func<LessonsWindow> factory) => _factory = factory;

        public Lesson? ShowDialogAndPick()
        {
            // If window already open, bring to front and do not open a second one
            if (_current is not null && _current.IsVisible)
            {
                try
                {
                    _current.Activate();
                    _current.Focus();
                }
                catch { }
                return null;
            }

            var win = _factory();
            _current = win;
            win.Owner = Application.Current?.MainWindow;

            // Clear reference when window is closed
            win.Closed += (s, e) => { if (ReferenceEquals(_current, win)) _current = null; };

            win.ShowDialog();
            return win.VM.Picked;
        }
    }
}
