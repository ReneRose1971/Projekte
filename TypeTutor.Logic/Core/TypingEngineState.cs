using System;
using System.Collections.Generic;
using System.Linq;

namespace TypeTutor.Logic.Core;

/// <summary>
/// Repräsentiert eine Momentaufnahme der Tipp-Session relativ zu einem Zieltext.
/// Dieser Typ ist UI-agnostisch und dient als DTO zwischen Engine und UI.
/// 
/// Design:
/// - Immutable record: sicher für Tests und Datenbindung.
/// - Enthält nur Status, keine Logik zur Eingabeverarbeitung.
/// - Fehlerpositionen sind 0-basiert relativ zum TargetText.
/// </summary>
public sealed record TypingEngineState
{
    /// <summary>Zieltext der aktuellen Lesson (kann leer sein, aber nie null).</summary>
    public string TargetText { get; init; }

    /// <summary>Bereits getippter Text (Engine-Interpretation; nie null).</summary>
    public string InputText { get; init; }

    /// <summary>Anzahl der korrekt übereinstimmenden Zeichen am Anfang (Prefix-Match).</summary>
    public int CorrectPrefixLength { get; init; }

    /// <summary>Gesamtzahl der bisher als Fehler klassifizierten Zeichen.</summary>
    public int ErrorCount { get; init; }

    /// <summary>0-basierter Index des nächsten erwarteten Zeichens im TargetText.</summary>
    public int NextIndex { get; init; }

    /// <summary>True, wenn die Eingabe alle Zeichen des Zieltexts korrekt erfasst (und keine offenen Fehler).</summary>
    public bool IsComplete { get; init; }

    /// <summary>Optional: Liste der Fehlerpositionen relativ zum TargetText (0-basiert, aufsteigend).</summary>
    public IReadOnlyList<int> ErrorPositions { get; init; }

    /// <summary>Erwartetes nächstes Zeichen oder null, falls fertig oder Target leer.</summary>
    public char? ExpectedNextChar { get; init; }

    /// <summary>Zuletzt eingegebenes Zeichen (falls vorhanden, bereits interpretiert) – nur zur UI-Anzeige.</summary>
    public char? LastInputChar { get; init; }

    /// <summary>Erzeugt einen konsistenten Status. Strings werden auf leer normalisiert, Listen auf read-only.</summary>
    public TypingEngineState(
        string? targetText,
        string? inputText,
        int correctPrefixLength,
        int errorCount,
        int nextIndex,
        bool isComplete,
        IEnumerable<int>? errorPositions,
        char? expectedNextChar,
        char? lastInputChar)
    {
        TargetText = targetText ?? string.Empty;
        InputText = inputText ?? string.Empty;

        if (correctPrefixLength < 0) throw new ArgumentOutOfRangeException(nameof(correctPrefixLength));
        if (errorCount < 0) throw new ArgumentOutOfRangeException(nameof(errorCount));
        if (nextIndex < 0) throw new ArgumentOutOfRangeException(nameof(nextIndex));
        if (nextIndex > TargetText.Length)
            throw new ArgumentOutOfRangeException(nameof(nextIndex), "NextIndex darf nicht hinter dem Target liegen.");

        CorrectPrefixLength = correctPrefixLength;
        ErrorCount = errorCount;
        NextIndex = nextIndex;
        IsComplete = isComplete;
        ErrorPositions = (errorPositions ?? Array.Empty<int>()).OrderBy(x => x).ToArray();
        ExpectedNextChar = expectedNextChar;
        LastInputChar = lastInputChar;
    }

    /// <summary>
    /// Factory für einen leeren Anfangszustand zu einem gegebenen Zieltext.
    /// </summary>
    public static TypingEngineState Start(string? targetText)
        => new(
            targetText: targetText ?? string.Empty,
            inputText: string.Empty,
            correctPrefixLength: 0,
            errorCount: 0,
            nextIndex: 0,
            isComplete: string.IsNullOrEmpty(targetText),
            errorPositions: Array.Empty<int>(),
            expectedNextChar: string.IsNullOrEmpty(targetText) ? (char?)null : (targetText![0]),
            lastInputChar: null
        );

    /// <summary>
    /// Bequeme Textdarstellung für Debug/Logs.
    /// </summary>
    public override string ToString()
        => $"TypingState: len(Target)={TargetText.Length}, len(Input)={InputText.Length}, CorrectPrefix={CorrectPrefixLength}, Errors={ErrorCount}, NextIndex={NextIndex}, Complete={IsComplete}";
}
