using Microsoft.Extensions.DependencyInjection;
using Common.Bootstrap;
using DataToolKit.Abstractions.DI;
using CustomWPFControls.Services;

namespace CustomWPFControls;

/// <summary>
/// Registriert alle Services aus der CustomWPFControls-Bibliothek.
/// </summary>
public sealed class CustomWpfControlsModule : IServiceModule
{
    public void Register(IServiceCollection services)
    {
        // 1. JSON-Repository für WindowLayoutData (vereinfacht - alles in einem Aufruf)
        services.AddJsonRepository<WindowLayoutData>(
            appSubFolder: "CustomWPFControls",
            fileNameBase: "WindowLayouts");

        // 2. WindowLayoutService registrieren
        services.AddSingleton<WindowLayoutService>();
    }
}
