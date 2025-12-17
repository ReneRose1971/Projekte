using System.Windows;
using Common.Bootstrap;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SolutionBundler.Core;

namespace SolutionBundler.WPF;

public partial class App : System.Windows.Application
{
    private IHost? _host;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var builder = Host.CreateApplicationBuilder();

        // Phase 1: Service-Registrierung
        // SolutionBundler.Core-Module registrieren (enthält DataToolKit, ProjectStore, Services)
        builder.Services.AddModulesFromAssemblies(
            typeof(SolutionBundlerCoreModule).Assembly);

        // SolutionBundler.WPF-Module registrieren (enthält ViewModels, Windows, Factories)
        builder.Services.AddModulesFromAssemblies(
            typeof(SolutionBundlerWpfModule).Assembly);

        // Phase 2: Container bauen
        _host = builder.Build();

        // Phase 3: DataStores initialisieren (NACH Build!)
        _host.Services.InitializeDataStores(
            typeof(SolutionBundlerCoreModule).Assembly,
            typeof(SolutionBundlerWpfModule).Assembly);

        // Phase 4: MainWindowWithSplitView starten
        var mainWindow = _host.Services.GetRequiredService<MainWindowWithSplitView>();
        mainWindow.Show();
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (_host != null)
        {
            await _host.StopAsync();
            _host.Dispose();
        }
        base.OnExit(e);
    }
}

