namespace Scriptum.Wpf.Navigation;

/// <summary>
/// Navigationsservice für das Umschalten zwischen Views.
/// </summary>
public interface INavigationService
{
    /// <summary>
    /// Navigiert zur Home-Ansicht.
    /// </summary>
    void NavigateToHome();

    /// <summary>
    /// Navigiert zur Modul-Liste.
    /// </summary>
    void NavigateToModuleList();

    /// <summary>
    /// Navigiert zur Lektions-Liste eines Moduls.
    /// </summary>
    void NavigateToLessonList(string moduleId);

    /// <summary>
    /// Navigiert zu den Details einer Lektion.
    /// </summary>
    void NavigateToLessonDetails(string moduleId, string lessonId);

    /// <summary>
    /// Navigiert zum Lektions-Guide.
    /// </summary>
    void NavigateToLessonGuide(string lessonId);

    /// <summary>
    /// Navigiert zur Trainingsansicht.
    /// </summary>
    void NavigateToTraining(string moduleId, string lessonId);

    /// <summary>
    /// Navigiert zur Trainings-Zusammenfassung.
    /// </summary>
    void NavigateToTrainingSummary(int? sessionId = null);

    /// <summary>
    /// Navigiert zur Session-Verlaufsansicht.
    /// </summary>
    void NavigateToSessionHistory();

    /// <summary>
    /// Navigiert zu den Details einer Session.
    /// </summary>
    void NavigateToSessionDetail(int sessionId);

    /// <summary>
    /// Navigiert zum Statistik-Dashboard.
    /// </summary>
    void NavigateToStatisticsDashboard();

    /// <summary>
    /// Navigiert zur Error-Heatmap.
    /// </summary>
    void NavigateToErrorHeatmap();

    /// <summary>
    /// Navigiert zu den Einstellungen.
    /// </summary>
    void NavigateToSettings();

    /// <summary>
    /// Navigiert zum Content-Management.
    /// </summary>
    void NavigateToContentManagement();

    /// <summary>
    /// Navigiert zum Content-Import.
    /// </summary>
    void NavigateToContentImport();
}
