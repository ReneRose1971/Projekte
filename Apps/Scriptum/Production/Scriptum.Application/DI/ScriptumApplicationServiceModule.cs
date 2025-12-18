using Common.Bootstrap;
using Microsoft.Extensions.DependencyInjection;
using Scriptum.Engine;

namespace Scriptum.Application.DI;

/// <summary>
/// Service-Modul für Scriptum.Application.
/// </summary>
/// <remarks>
/// <para>
/// <b>Abhängigkeiten:</b> Dieses Modul setzt voraus, dass folgende Module
/// bereits registriert wurden:
/// </para>
/// <list type="bullet">
/// <item><see cref="DataToolKit"/> (DataStoreProvider, Repositories)</item>
/// <item><see cref="Scriptum.Engine"/> (ITrainingEngine, IInputInterpreter, IClock)</item>
/// <item><see cref="Scriptum.Content"/> (Content-DataStores)</item>
/// <item><see cref="Scriptum.Progress"/> (Progress-DataStores)</item>
/// </list>
/// <para>
/// Bei Verwendung von <c>AddModulesFromAssemblies</c> mit allen relevanten
/// Assemblies wird dies automatisch sichergestellt.
/// </para>
/// </remarks>
public sealed class ScriptumApplicationServiceModule : IServiceModule
{
    /// <summary>
    /// Registriert die Services des Application-Layers.
    /// </summary>
    /// <param name="services">Die Service-Collection.</param>
    public void Register(IServiceCollection services)
    {
        services.AddSingleton<ITrainingSessionCoordinator, TrainingSessionCoordinator>();
        services.AddSingleton<IInputInterpreter, DeQwertzInputInterpreter>();
        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton<ITrainingEngine, TrainingEngine>();
    }
}
