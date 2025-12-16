using System.Security.Cryptography;
using System.Text;
using SolutionBundler.Core.Abstractions;

namespace SolutionBundler.Core.Implementations;

/// <summary>
/// Berechnet SHA1-Hashes von Byte-Arrays.
/// </summary>
public sealed class Sha1HashCalculator : IHashCalculator
{
    /// <summary>
    /// Berechnet die SHA1-Prüfsumme als hexadezimale Zeichenfolge.
    /// </summary>
    /// <param name="bytes">Die zu hashenden Bytes.</param>
    /// <returns>40-stellige hexadezimale SHA1-Repräsentation.</returns>
    public string Sha1(byte[] bytes)
    {
        using var sha1 = SHA1.Create();
        var hash = sha1.ComputeHash(bytes);
        var sb = new StringBuilder(hash.Length * 2);
        foreach (var b in hash) sb.Append(b.ToString("x2"));
        return sb.ToString();
    }
}