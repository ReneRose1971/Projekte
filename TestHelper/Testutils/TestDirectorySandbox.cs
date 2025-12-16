using System;
using System.IO;

namespace TestHelper.TestUtils
{
    /// <summary>
    /// Automatisch verwalteter Sandbox-Ordner für I/O-lastige Tests.
    /// Erstellt bei Instanziierung einen eindeutigen temporären Ordner
    /// und löscht ihn beim Dispose rekursiv.
    ///
    /// Einsatzfälle:
    /// - Tests, die Dateien schreiben/lesen
    /// - Tests, die temporäre Directory-Strukturen benötigen
    /// - saubere Trennung ohne Testreste im Dateisystem
    /// </summary>
    public sealed class TestDirectorySandbox : IDisposable
    {
        /// <summary>
        /// Der absolute Pfad zum Sandbox-Wurzelverzeichnis.
        /// </summary>
        public string Root { get; }

        public TestDirectorySandbox()
        {
            Root = Path.Combine(
                Path.GetTempPath(),
                "DTK_Tests_Sandbox",
                Guid.NewGuid().ToString("N"));

            Directory.CreateDirectory(Root);
        }

        /// <summary>
        /// Kombiniert die Sandbox-Root mit einem relativen Teilpfad.
        /// </summary>
        public string PathOf(string relative)
            => Path.Combine(Root, relative);

        /// <summary>
        /// Erzeugt (falls nötig) einen Unterordner relativ zur Sandbox-Root.
        /// </summary>
        public string EnsureFolder(string relativeFolder)
        {
            var path = PathOf(relativeFolder);
            Directory.CreateDirectory(path);
            return path;
        }

        /// <summary>
        /// Löscht den kompletten Sandbox-Baum rekursiv.
        /// Fehler werden abgefangen, um Tests nicht zu blockieren.
        /// </summary>
        public void Dispose()
        {
            try
            {
                if (Directory.Exists(Root))
                    Directory.Delete(Root, recursive: true);
            }
            catch
            {
                // Best-effort Cleanup, keine Testabbrüche riskieren.
            }
        }
    }
}
