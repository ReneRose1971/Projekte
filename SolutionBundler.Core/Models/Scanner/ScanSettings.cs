namespace SolutionBundler.Core.Models;

/// <summary>
/// Einstellungen, die das Scan-Verhalten und die Ausgabe steuern.
/// </summary>
public sealed class ScanSettings
{
    /// <summary>
    /// Muster von Dateitypen/Dateinamen, die beim Scan eingeschlossen werden.
    /// </summary>
    public string[] IncludePatterns { get; init; } =
    {
        "*.sln","*.csproj","*.props","*.targets","*.cs","*.xaml","*.resx",
        "*.config","*.json",".editorconfig","app.manifest","Directory.Build.props","Directory.Build.targets","Directory.Packages.props"
    };

    /// <summary>
    /// Ordnernamen, die vom Scan ausgeschlossen werden (z. B. bin/obj/.git).
    /// </summary>
    public string[] ExcludeDirs { get; init; } = { "bin", "obj", ".git", ".vs", "node_modules" };

    /// <summary>
    /// Dateinamensmuster, die ausgeschlossen werden (z. B. generierte Dateien).
    /// </summary>
    public string[] ExcludeGlobs { get; init; } = { "*.g.cs", "*.designer.cs", "*.user", "*.pfx", "*.pem" };

    /// <summary>
    /// Gibt an, ob potenziell sensible Werte in der Ausgabe maskiert werden sollen.
    /// </summary>
    public bool MaskSecrets { get; init; } = true;

    /// <summary>
    /// Standardname der Ausgabedatei (wird von der UI ggf. überschrieben).
    /// </summary>
    public string OutputFileName { get; init; } = "Bundle.md";
}
