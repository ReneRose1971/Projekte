using Common.Bootstrap;
using Microsoft.Extensions.DependencyInjection;
using Scriptum.Wpf.Keyboard.ViewModels;

namespace Scriptum.Wpf.DI;

/// <summary>
/// Service-Modul für Scriptum.Wpf.
/// </summary>
/// <remarks>
/// <para>
/// <b>Abhängigkeiten:</b> Dieses Modul setzt voraus, dass folgende Module
/// bereits registriert wurden:
/// </para>
/// <list type="bullet">
/// <item><see cref="DataToolKit"/> (DataStoreProvider, Repositories)</item>
/// <item><see cref="Scriptum.Persistence"/> (Repositories, Comparer)</item>
/// <item><see cref="Scriptum.Application"/> (ITrainingSessionCoordinator)</item>
/// </list>
/// </remarks>
public sealed class ScriptumWpfServiceModule : IServiceModule
{
    public void Register(IServiceCollection services)
    {
        services.AddSingleton<IKeyChordAdapter, WpfKeyChordAdapter>();
        services.AddSingleton<VisualKeyboardViewModel>();
        services.AddTransient<MainViewModel>();
        services.AddTransient<MainWindow>();
    }
}
