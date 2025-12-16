// src/TypeTutor.WPF/LessonsWindowViewModel.cs
using System;
using TypeTutor.Logic.Core;

namespace TypeTutor.WPF
{
    /// <summary>
    /// Host-VM fürs Lessons-Fenster: kapselt die Liste und liefert die Auswahl.
    /// </summary>
    public sealed class LessonsWindowViewModel
    {
        public LessonListViewModel ListVM { get; }
        public Lesson? Picked { get; private set; }

        public LessonsWindowViewModel(LessonListViewModel listVm) => ListVM = listVm;

        public void ConfirmSelection() => Picked = ListVM.SelectedItem;
        public void Cancel() => Picked = null;
    }
}
