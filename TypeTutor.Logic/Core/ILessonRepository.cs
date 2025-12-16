// TypeTutor.Logic.Core/ILessonRepository.cs
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TypeTutor.Logic.Core
{
    public interface ILessonRepository
    {
        /// <summary>Wurzelordner, in dem die JSON-Lektionen gespeichert werden.</summary>
        string RootFolder { get; }

        /// <summary>Alle vorhandenen Lessons laden (sortiert ist Implementierungsdetail).</summary>
        Task<IReadOnlyList<Lesson>> LoadAllAsync();

        /// <summary>Eine Lesson per Title laden; null, wenn nicht vorhanden.</summary>
        Task<Lesson?> LoadLessonAsync(string title);

        /// <summary>
        /// Neue Lesson anlegen oder (bei overwrite=true) überschreiben.
        /// Gibt die persistierte Lesson zurück (inkl. finaler Normalisierung über ILessonFactory).
        /// </summary>
        Task<Lesson> CreateLessonAsync(LessonMetaData meta, string text, bool overwrite = false);

        /// <summary>Eine Lesson per Title löschen. true, wenn gelöscht; false, wenn nicht gefunden.</summary>
        Task<bool> DeleteLessonAsync(string title);
    }
}
