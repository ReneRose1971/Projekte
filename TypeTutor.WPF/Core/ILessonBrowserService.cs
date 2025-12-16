// src/TypeTutor.WPF/ILessonBrowserService.cs
using System;
using TypeTutor.Logic.Core;

namespace TypeTutor.WPF
{
    public interface ILessonBrowserService
    {
        /// <summary>Zeigt das Fenster und liefert die Auswahl (oder null) zurück.</summary>
        Lesson? ShowDialogAndPick();
    }
}
