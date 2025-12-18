namespace Scriptum.Application.Import;

/// <summary>
/// Beschreibt die Parameter für einen Content-Import.
/// </summary>
/// <param name="ModulesImportJsonPath">Pfad zur JSON-Datei mit den zu importierenden Modulen.</param>
/// <param name="LessonsImportJsonPath">Pfad zur JSON-Datei mit den zu importierenden Lektionen.</param>
/// <param name="GuidesImportJsonPath">Pfad zur JSON-Datei mit den zu importierenden Anleitungen.</param>
/// <param name="OverwriteExisting">
/// Wenn <c>true</c>, werden vorhandene Inhalte überschrieben.
/// Wenn <c>false</c>, wird der Import abgebrochen, wenn bereits Inhalte vorhanden sind.
/// </param>
public sealed record ContentImportRequest(
    string ModulesImportJsonPath,
    string LessonsImportJsonPath,
    string GuidesImportJsonPath,
    bool OverwriteExisting);
