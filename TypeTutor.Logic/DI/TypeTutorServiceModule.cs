using Common.Bootstrap;
using DataToolKit.Abstractions.DI;
using DataToolKit.Abstractions.Repositories;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using TypeTutor.Logic.Data;

namespace TypeTutor.Logic.DI;

/// <summary>
/// Service-Modul für TypeTutor.Logic.
/// Registriert EqualityComparer und JSON-Repositories für LessonData, LessonGuideData und ModuleData.
/// 
/// Verwendung:
/// <code>
/// var services = new ServiceCollection();
/// services.AddModulesFromAssemblies(
///     typeof(DataToolKitServiceModule).Assembly,  // DataToolKit-Infrastruktur
///     typeof(TypeTutorServiceModule).Assembly);   // TypeTutor-Services
/// </code>
/// </summary>
/// <remarks>
/// <para>
/// <b>Abhängigkeit:</b> Dieses Modul setzt voraus, dass <see cref="DataToolKitServiceModule"/>
/// bereits registriert wurde, um die DataToolKit-Infrastruktur (IRepositoryFactory, IDataStoreProvider, etc.)
/// verfügbar zu machen.
/// </para>
/// <para>
/// Bei Verwendung von <c>AddModulesFromAssemblies</c> mit beiden Assemblies wird dies automatisch sichergestellt.
/// </para>
/// </remarks>
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
    /// Registriert EqualityComparer für LessonData, LessonGuideData und ModuleData.
    /// Diese werden für Repository-Operationen (Duplicate-Detection) benötigt.
    /// </summary>
    private static void RegisterEqualityComparers(IServiceCollection services)
    {
        // LessonData EqualityComparer (basierend auf LessonId)
        services.AddSingleton<IEqualityComparer<LessonData>>(
            new LessonDataEqualityComparer());

        // LessonGuideData EqualityComparer (basierend auf LessonId)
        services.AddSingleton<IEqualityComparer<LessonGuideData>>(
            new LessonGuideDataEqualityComparer());

        // ModuleData EqualityComparer (basierend auf ModuleId)
        services.AddSingleton<IEqualityComparer<ModuleData>>(
            new ModuleDataEqualityComparer());
    }

    /// <summary>
    /// Registriert JSON-Repositories für LessonData, LessonGuideData und ModuleData.
    /// 
    /// Speicherort:
    /// - Windows: %USERPROFILE%\Documents\TypeTutor\Data\
    /// - Mac/Linux: ~/Documents/TypeTutor/Data/
    /// 
    /// Dateinamen:
    /// - LessonData: lessons.json
    /// - LessonGuideData: lesson-guides.json
    /// - ModuleData: modules.json
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

        // ModuleData Repository
        services.AddJsonRepository<ModuleData>(
            appSubFolder: "TypeTutor",
            fileNameBase: "modules",
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
