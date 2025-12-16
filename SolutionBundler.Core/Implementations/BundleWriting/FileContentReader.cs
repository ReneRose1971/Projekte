using SolutionBundler.Core.Abstractions;

namespace SolutionBundler.Core.Implementations.BundleWriting;

/// <summary>
/// Reads file contents with error handling and optional secret masking.
/// </summary>
internal sealed class FileContentReader
{
    private readonly ISecretMasker _masker;

    public FileContentReader(ISecretMasker masker)
    {
        _masker = masker;
    }

    /// <summary>
    /// Reads the content of a file with error handling.
    /// </summary>
    /// <param name="fullPath">Full path to the file.</param>
    /// <param name="relativePath">Relative path for error messages and masking context.</param>
    /// <param name="maskSecrets">Whether to apply secret masking.</param>
    /// <returns>The file content or an error message.</returns>
    public string ReadContent(string fullPath, string relativePath, bool maskSecrets)
    {
        try
        {
            var content = File.ReadAllText(fullPath);
            
            if (maskSecrets)
            {
                content = _masker.Process(relativePath, content);
            }

            return content;
        }
        catch
        {
            return $"/* FEHLER: Datei konnte nicht gelesen werden: {fullPath} */";
        }
    }
}
