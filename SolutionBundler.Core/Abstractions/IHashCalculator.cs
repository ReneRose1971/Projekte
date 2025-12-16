namespace SolutionBundler.Core.Abstractions;

/// <summary>
/// Berechnet Prüfsummen (z. B. SHA1) von Inhalten.
/// </summary>
public interface IHashCalculator
{
    /// <summary>
    /// Berechnet die SHA1-Prüfsumme der angegebenen Bytefolge und gibt sie hex-kodiert zurück.
    /// </summary>
    /// <param name="bytes">Die zu hashenden Bytes.</param>
    /// <returns>Hex-kodierter SHA1-Hash (40 Zeichen).</returns>
    string Sha1(byte[] bytes);
}