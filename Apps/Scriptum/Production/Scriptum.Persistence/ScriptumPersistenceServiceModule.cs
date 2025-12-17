using System.Collections.Generic;
using Common.Bootstrap;
using DataToolKit.Abstractions.DI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Scriptum.Progress;

namespace Scriptum.Persistence;

/// <summary>
/// Service-Modul für Scriptum.Persistence.
/// Registriert LiteDB-Repository für <see cref="TrainingSession"/>.
/// </summary>
/// <remarks>
/// <para>
/// <b>Abhängigkeiten:</b> Dieses Modul setzt voraus, dass 
/// <see cref="DataToolKitServiceModule"/> bereits registriert wurde.
/// </para>
/// <para>
/// Bei Verwendung von <c>AddModulesFromAssemblies</c> mit beiden Assemblies 
/// wird dies automatisch sichergestellt.
/// </para>
/// <para>
/// <b>Datenbankpfad:</b> %APPDATA%\Scriptum\scriptum.db
/// </para>
/// </remarks>
public sealed class ScriptumPersistenceServiceModule : IServiceModule
{
    /// <summary>
    /// Registriert das LiteDB-Repository für TrainingSession und den zugehörigen Comparer.
    /// </summary>
    /// <param name="services">Die Service-Collection.</param>
    public void Register(IServiceCollection services)
    {
        new DataToolKitServiceModule().Register(services);

        services.TryAddSingleton<IEqualityComparer<TrainingSession>, TrainingSessionComparer>();

        services.AddLiteDbRepository<TrainingSession>(
            appSubFolder: "Scriptum",
            fileNameBase: "scriptum",
            subFolder: null);
    }
}
