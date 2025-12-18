using System.Collections.Generic;
using Common.Bootstrap;
using DataToolKit.Abstractions.DI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Scriptum.Content.Comparers;
using Scriptum.Content.Data;
using Scriptum.Progress;

namespace Scriptum.Persistence;

/// <summary>
/// Service-Modul für Scriptum.Persistence.
/// Registriert Repositories und Comparer für Scriptum.Content und Scriptum.Progress.
/// </summary>
/// <remarks>
/// <para>
/// <b>Abhängigkeiten:</b> Dieses Modul setzt voraus, dass 
/// DataToolKitServiceModule bereits registriert wurde.
/// </para>
/// <para>
/// Bei Verwendung von <c>AddModulesFromAssemblies</c> mit beiden Assemblies 
/// wird dies automatisch sichergestellt.
/// </para>
/// <para>
/// <b>Datenpfade:</b>
/// </para>
/// <list type="bullet">
/// <item><description>TrainingSession: %APPDATA%\Scriptum\scriptum.db (LiteDB)</description></item>
/// <item><description>ModuleData: %APPDATA%\Scriptum\Content\modules.json</description></item>
/// <item><description>LessonData: %APPDATA%\Scriptum\Content\lessons.json</description></item>
/// <item><description>LessonGuideData: %APPDATA%\Scriptum\Content\lesson-guides.json</description></item>
/// </list>
/// </remarks>
public sealed class ScriptumPersistenceServiceModule : IServiceModule
{
    /// <summary>
    /// Registriert Repositories und Comparer für Scriptum.
    /// </summary>
    /// <param name="services">Die Service-Collection.</param>
    public void Register(IServiceCollection services)
    {
        RegisterComparers(services);
        RegisterRepositories(services);
    }

    private static void RegisterComparers(IServiceCollection services)
    {
        services.TryAddSingleton<IEqualityComparer<TrainingSession>, TrainingSessionComparer>();
        services.TryAddSingleton<IEqualityComparer<ModuleData>, ModuleDataComparer>();
        services.TryAddSingleton<IEqualityComparer<LessonData>, LessonDataComparer>();
        services.TryAddSingleton<IEqualityComparer<LessonGuideData>, LessonGuideDataComparer>();
    }

    private static void RegisterRepositories(IServiceCollection services)
    {
        services.AddLiteDbRepository<TrainingSession>(
            appSubFolder: "Scriptum",
            fileNameBase: "scriptum",
            subFolder: null);

        services.AddJsonRepository<ModuleData>(
            appSubFolder: "Scriptum",
            fileNameBase: "modules",
            subFolder: "Content");

        services.AddJsonRepository<LessonData>(
            appSubFolder: "Scriptum",
            fileNameBase: "lessons",
            subFolder: "Content");

        services.AddJsonRepository<LessonGuideData>(
            appSubFolder: "Scriptum",
            fileNameBase: "lesson-guides",
            subFolder: "Content");
    }
}
