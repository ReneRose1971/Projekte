using Scriptum.Core;

namespace Scriptum.Engine;

/// <summary>
/// Rein fachlicher Zustandsschnappschuss einer laufenden Übung.
/// </summary>
/// <remarks>
/// Der Trainingszustand enthält keine Historie.
/// </remarks>
public sealed class TrainingState
{
    /// <summary>
    /// Die Zielsequenz der Übung.
    /// </summary>
    public TargetSequence Sequence { get; }
    
    /// <summary>
    /// Die aktuelle Zielposition (0-basiert).
    /// </summary>
    public int CurrentTargetIndex { get; }
    
    /// <summary>
    /// Startzeitpunkt der Übung.
    /// </summary>
    public DateTime StartTime { get; }
    
    /// <summary>
    /// Endzeitpunkt der Übung (null, wenn noch nicht abgeschlossen).
    /// </summary>
    public DateTime? EndTime { get; }
    
    /// <summary>
    /// Gibt an, ob aktuell ein Fehler aktiv ist.
    /// </summary>
    public bool IstFehlerAktiv { get; }
    
    /// <summary>
    /// Die Position des aktiven Fehlers (nur relevant wenn IstFehlerAktiv == true).
    /// </summary>
    public int FehlerPosition { get; }
    
    /// <summary>
    /// Anzahl der Gesamteingaben (inkl. Rücktasten).
    /// </summary>
    public int GesamtEingaben { get; }
    
    /// <summary>
    /// Anzahl der Fehler.
    /// </summary>
    public int Fehler { get; }
    
    /// <summary>
    /// Anzahl der Korrekturen (Rücktaste bei aktivem Fehler).
    /// </summary>
    public int Korrekturen { get; }
    
    /// <summary>
    /// Anzahl der Rücktasten (insgesamt).
    /// </summary>
    public int Ruecktasten { get; }
    
    /// <summary>
    /// Gibt an, ob die Übung abgeschlossen ist.
    /// </summary>
    public bool IstAbgeschlossen => EndTime.HasValue;
    
    /// <summary>
    /// Erstellt einen neuen Trainingszustand.
    /// </summary>
    /// <param name="sequence">Die Zielsequenz (darf nicht null sein).</param>
    /// <param name="currentTargetIndex">Die aktuelle Zielposition (muss >= 0 und < Sequence.Length sein).</param>
    /// <param name="startTime">Startzeitpunkt.</param>
    /// <param name="endTime">Endzeitpunkt (optional).</param>
    /// <param name="istFehlerAktiv">Gibt an, ob ein Fehler aktiv ist.</param>
    /// <param name="fehlerPosition">Die Position des Fehlers.</param>
    /// <param name="gesamtEingaben">Anzahl Gesamteingaben (muss >= 0 sein).</param>
    /// <param name="fehler">Anzahl Fehler (muss >= 0 sein).</param>
    /// <param name="korrekturen">Anzahl Korrekturen (muss >= 0 sein).</param>
    /// <param name="ruecktasten">Anzahl Rücktasten (muss >= 0 sein).</param>
    /// <exception cref="ArgumentNullException">sequence ist null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Ungültige Werte für Indizes oder Zähler.</exception>
    /// <exception cref="ArgumentException">EndTime liegt vor StartTime.</exception>
    public TrainingState(
        TargetSequence sequence,
        int currentTargetIndex,
        DateTime startTime,
        DateTime? endTime = null,
        bool istFehlerAktiv = false,
        int fehlerPosition = 0,
        int gesamtEingaben = 0,
        int fehler = 0,
        int korrekturen = 0,
        int ruecktasten = 0)
    {
        if (sequence is null)
            throw new ArgumentNullException(nameof(sequence));
        
        if (currentTargetIndex < 0)
            throw new ArgumentOutOfRangeException(nameof(currentTargetIndex), "currentTargetIndex muss >= 0 sein.");
        
        if (currentTargetIndex > sequence.Length)
            throw new ArgumentOutOfRangeException(nameof(currentTargetIndex), 
                $"currentTargetIndex ({currentTargetIndex}) darf nicht größer als Sequence.Length ({sequence.Length}) sein.");
        
        if (gesamtEingaben < 0)
            throw new ArgumentOutOfRangeException(nameof(gesamtEingaben), "gesamtEingaben muss >= 0 sein.");
        
        if (fehler < 0)
            throw new ArgumentOutOfRangeException(nameof(fehler), "fehler muss >= 0 sein.");
        
        if (korrekturen < 0)
            throw new ArgumentOutOfRangeException(nameof(korrekturen), "korrekturen muss >= 0 sein.");
        
        if (ruecktasten < 0)
            throw new ArgumentOutOfRangeException(nameof(ruecktasten), "ruecktasten muss >= 0 sein.");
        
        if (endTime.HasValue && endTime.Value < startTime)
            throw new ArgumentException("endTime darf nicht vor startTime liegen.", nameof(endTime));
        
        Sequence = sequence;
        CurrentTargetIndex = currentTargetIndex;
        StartTime = startTime;
        EndTime = endTime;
        IstFehlerAktiv = istFehlerAktiv;
        FehlerPosition = fehlerPosition;
        GesamtEingaben = gesamtEingaben;
        Fehler = fehler;
        Korrekturen = korrekturen;
        Ruecktasten = ruecktasten;
    }
    
    /// <summary>
    /// Erstellt einen neuen Trainingszustand mit erhöhter GesamtEingaben-Zählung.
    /// </summary>
    /// <param name="current">Der aktuelle Zustand.</param>
    /// <returns>Ein neuer Zustand mit inkrementierter GesamtEingaben.</returns>
    public static TrainingState WithIncrementedInput(TrainingState current)
    {
        return new TrainingState(
            sequence: current.Sequence,
            currentTargetIndex: current.CurrentTargetIndex,
            startTime: current.StartTime,
            endTime: current.EndTime,
            istFehlerAktiv: current.IstFehlerAktiv,
            fehlerPosition: current.FehlerPosition,
            gesamtEingaben: current.GesamtEingaben + 1,
            fehler: current.Fehler,
            korrekturen: current.Korrekturen,
            ruecktasten: current.Ruecktasten);
    }
    
    /// <summary>
    /// Erstellt einen neuen Trainingszustand für eine korrekte Eingabe.
    /// </summary>
    /// <param name="current">Der aktuelle Zustand.</param>
    /// <param name="newTargetIndex">Der neue Zielindex nach korrekter Eingabe.</param>
    /// <param name="endTime">Der Endzeitpunkt (wenn die Übung abgeschlossen ist).</param>
    /// <returns>Ein neuer Zustand mit erhöhtem Index und aktualisierter GesamtEingaben.</returns>
    public static TrainingState WithCorrectInput(TrainingState current, int newTargetIndex, DateTime? endTime)
    {
        return new TrainingState(
            sequence: current.Sequence,
            currentTargetIndex: newTargetIndex,
            startTime: current.StartTime,
            endTime: endTime,
            istFehlerAktiv: false,
            fehlerPosition: current.FehlerPosition,
            gesamtEingaben: current.GesamtEingaben + 1,
            fehler: current.Fehler,
            korrekturen: current.Korrekturen,
            ruecktasten: current.Ruecktasten);
    }
    
    /// <summary>
    /// Erstellt einen neuen Trainingszustand für eine fehlerhafte Eingabe.
    /// </summary>
    /// <param name="current">Der aktuelle Zustand.</param>
    /// <returns>Ein neuer Zustand mit aktivem Fehler und erhöhten Zählern.</returns>
    public static TrainingState WithIncorrectInput(TrainingState current)
    {
        return new TrainingState(
            sequence: current.Sequence,
            currentTargetIndex: current.CurrentTargetIndex,
            startTime: current.StartTime,
            endTime: null,
            istFehlerAktiv: true,
            fehlerPosition: current.CurrentTargetIndex,
            gesamtEingaben: current.GesamtEingaben + 1,
            fehler: current.Fehler + 1,
            korrekturen: current.Korrekturen,
            ruecktasten: current.Ruecktasten);
    }
    
    /// <summary>
    /// Erstellt einen neuen Trainingszustand für eine Korrektur-Eingabe (Rücktaste bei aktivem Fehler).
    /// </summary>
    /// <param name="current">Der aktuelle Zustand.</param>
    /// <returns>Ein neuer Zustand mit deaktiviertem Fehler und erhöhten Zählern.</returns>
    public static TrainingState WithCorrectionInput(TrainingState current)
    {
        return new TrainingState(
            sequence: current.Sequence,
            currentTargetIndex: current.CurrentTargetIndex,
            startTime: current.StartTime,
            endTime: null,
            istFehlerAktiv: false,
            fehlerPosition: current.FehlerPosition,
            gesamtEingaben: current.GesamtEingaben + 1,
            fehler: current.Fehler,
            korrekturen: current.Korrekturen + 1,
            ruecktasten: current.Ruecktasten + 1);
    }
    
    /// <summary>
    /// Erstellt einen neuen Trainingszustand für eine Rücktaste-Eingabe ohne aktiven Fehler.
    /// </summary>
    /// <param name="current">Der aktuelle Zustand.</param>
    /// <returns>Ein neuer Zustand mit erhöhten Rücktasten-Zählern.</returns>
    public static TrainingState WithBackspaceInput(TrainingState current)
    {
        return new TrainingState(
            sequence: current.Sequence,
            currentTargetIndex: current.CurrentTargetIndex,
            startTime: current.StartTime,
            endTime: null,
            istFehlerAktiv: false,
            fehlerPosition: current.FehlerPosition,
            gesamtEingaben: current.GesamtEingaben + 1,
            fehler: current.Fehler,
            korrekturen: current.Korrekturen,
            ruecktasten: current.Ruecktasten + 1);
    }
}
