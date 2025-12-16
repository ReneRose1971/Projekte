using System;
using System.Text.RegularExpressions;
using SolutionBundler.Core.Abstractions;

namespace SolutionBundler.Core.Implementations;

/// <summary>
/// Maskiert vertrauliche Werte in JSON-/Config-Texten anhand einfacher Schlüsselheuristik.
/// </summary>
public sealed class RegexSecretMasker : ISecretMasker
{
    // einfache Heuristik: maskiert typische Keys in JSON/Config
    static readonly Regex KeyValue =
        new(@"(?i)(""?(password|apikey|api_key|connectionstring|secret|token)""?\s*[:=]\s*""?).+?(""|\r?\n)",
            RegexOptions.Compiled);

    /// <summary>
    /// Maskiert erkannte Geheimnisse im übergebenen Inhalt.
    /// </summary>
    /// <param name="relativePath">Relative Pfadangabe; nur bestimmte Dateitypen werden maskiert (z. B. .json, .config).</param>
    /// <param name="content">Der rohe Dateiinhalt.</param>
    /// <returns>Der Inhalt mit maskierten Werten oder unverändert, falls nicht anwendbar.</returns>
    public string Process(string relativePath, string content)
    {
        if (!relativePath.EndsWith(".json", StringComparison.OrdinalIgnoreCase) &&
            !relativePath.EndsWith(".config", StringComparison.OrdinalIgnoreCase))
            return content;

        return KeyValue.Replace(content, m => $"{m.Groups[1].Value}***MASKED***{m.Groups[3].Value}");
    }
}