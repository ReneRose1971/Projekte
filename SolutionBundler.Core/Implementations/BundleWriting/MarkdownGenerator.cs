using System.Text;
using SolutionBundler.Core.Models;

namespace SolutionBundler.Core.Implementations.BundleWriting;

/// <summary>
/// Generates Markdown content for bundle files.
/// </summary>
internal static class MarkdownGenerator
{
    /// <summary>
    /// Generates the complete Markdown document.
    /// </summary>
    /// <param name="projectName">Name of the project.</param>
    /// <param name="files">List of files to include in the bundle.</param>
    /// <param name="contentReader">Reader for file contents.</param>
    /// <param name="maskSecrets">Whether to mask secrets in file contents.</param>
    /// <returns>Complete Markdown document as string.</returns>
    public static string Generate(
        string projectName,
        IList<FileEntry> files,
        FileContentReader contentReader,
        bool maskSecrets)
    {
        var sb = new StringBuilder();

        AppendFrontMatter(sb, projectName);
        AppendTableOfContents(sb, files);
        AppendFileSections(sb, files, contentReader, maskSecrets);

        return sb.ToString();
    }

    private static void AppendFrontMatter(StringBuilder sb, string projectName)
    {
        sb.AppendLine("---");
        sb.AppendLine($"project_root: {projectName}");
        sb.AppendLine($"generated_at: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine("tool: SolutionBundler v1");
        sb.AppendLine("---");
    }

    private static void AppendTableOfContents(StringBuilder sb, IList<FileEntry> files)
    {
        sb.AppendLine("# Inhaltsverzeichnis");

        foreach (var file in files)
        {
            var anchor = MarkdownAnchorGenerator.Generate(file.RelativePath);
            sb.AppendLine($"* [{file.RelativePath}](#{anchor})");
        }

        sb.AppendLine();
    }

    private static void AppendFileSections(
        StringBuilder sb,
        IList<FileEntry> files,
        FileContentReader contentReader,
        bool maskSecrets)
    {
        sb.AppendLine("# Dateien");

        foreach (var file in files)
        {
            AppendFileSection(sb, file, contentReader, maskSecrets);
        }
    }

    private static void AppendFileSection(
        StringBuilder sb,
        FileEntry file,
        FileContentReader contentReader,
        bool maskSecrets)
    {
        sb.AppendLine();
        sb.AppendLine($"## {file.RelativePath}");
        sb.AppendLine($"_size_: {file.Size} bytes - _sha1_: {file.Sha1} - _action_: {file.Action}");
        sb.AppendLine();
        sb.AppendLine($"--- FILE: {file.RelativePath} | HASH: {file.Sha1} | ACTION: {file.Action} ---");

        var fence = string.IsNullOrWhiteSpace(file.Language) ? "```" : $"```{file.Language}";
        sb.AppendLine(fence);

        var content = contentReader.ReadContent(file.FullPath, file.RelativePath, maskSecrets);
        sb.AppendLine(content);

        sb.AppendLine("```");
    }
}
