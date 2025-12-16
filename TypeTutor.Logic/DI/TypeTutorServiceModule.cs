using Common.Bootstrap;
using DataToolKit.Abstractions.DI;
using DataToolKit.Abstractions.Repositories;
using DataToolKit.Storage.Repositories;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using TypeTutor.Logic.Data;

namespace TypeTutor.Logic.DI;

/// <summary>
/// Service-Modul für TypeTutor.Logic.
/// Registriert EqualityComparer und JSON-Repositories für LessonData und LessonGuideData.
/// 
/// Verwendung:
/// <code>
/// var services = new ServiceCollection();
/// services.AddModulesFromAssemblies(typeof(TypeTutorServiceModule).Assembly);
/// </code>
/// 
/// Oder manuell:
/// <code>
/// var services = new ServiceCollection();
/// new TypeTutorServiceModule().Register(services);
/// </code>
/// </summary>
public sealed class TypeTutorServiceModule : IServiceModule
{
    /// <summary>
    /// Registriert alle TypeTutor-spezifischen Services.
    /// </summary>
    /// <param name="services">Die Service-Collection.</param>
    public void Register(IServiceCollection services)
    {
        RegisterEqualityComparers(services);
        RegisterRepositories(services);
        RegisterDataStoreWrapper(services);
    }

    /// <summary>
    /// Registriert EqualityComparer für LessonData und LessonGuideData.
    /// Diese werden für Repository-Operationen (Duplicate-Detection) benötigt.
    /// </summary>
    private static void RegisterEqualityComparers(IServiceCollection services)
    {
        // LessonData EqualityComparer (basierend auf Title)
        services.AddSingleton<IEqualityComparer<LessonData>>(
            new LessonDataEqualityComparer());

        // LessonGuideData EqualityComparer (basierend auf Title)
        services.AddSingleton<IEqualityComparer<LessonGuideData>>(
            new LessonGuideDataEqualityComparer());
    }

    /// <summary>
    /// Registriert JSON-Repositories für LessonData und LessonGuideData.
    /// 
    /// Speicherort:
    /// - Windows: %USERPROFILE%\Documents\TypeTutor\Data\
    /// - Mac/Linux: ~/Documents/TypeTutor/Data/
    /// 
    /// Dateinamen:
    /// - LessonData: lessons.json
    /// - LessonGuideData: lesson-guides.json
    /// </summary>
    private static void RegisterRepositories(IServiceCollection services)
    {
        // LessonData Repository
        services.AddJsonRepository<LessonData>(
            appSubFolder: "TypeTutor",
            fileNameBase: "lessons",
            subFolder: "Data");

        // LessonGuideData Repository
        services.AddJsonRepository<LessonGuideData>(
            appSubFolder: "TypeTutor",
            fileNameBase: "lesson-guides",
            subFolder: "Data");
    }

    /// <summary>
    /// Registriert den DataStoreWrapper als Singleton.
    /// Dieser bietet einen einfachen Zugriff auf die DataStore-Collections.
    /// </summary>
    private static void RegisterDataStoreWrapper(IServiceCollection services)
    {
        services.AddSingleton<DataStoreWrapper>();
    }
}
