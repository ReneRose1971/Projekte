using System;
using System.Reflection;
using Common.Bootstrap;
using Microsoft.Extensions.DependencyInjection;
using Scriptum.Application.DI;
using Scriptum.Persistence;

namespace Scriptum.Wpf;

public partial class App : System.Windows.Application
{
    private IServiceProvider? _serviceProvider;

    protected override void OnStartup(System.Windows.StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();

        services.AddModulesFromAssemblies(
            typeof(DataToolKit.Abstractions.DI.DataToolKitServiceModule).Assembly,
            typeof(ScriptumPersistenceServiceModule).Assembly,
            typeof(ScriptumApplicationServiceModule).Assembly,
            typeof(App).Assembly);

        _serviceProvider = services.BuildServiceProvider();

        _serviceProvider.InitializeDataStores(
            typeof(ScriptumDataStoreInitializer).Assembly,
            typeof(App).Assembly);

        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    protected override void OnExit(System.Windows.ExitEventArgs e)
    {
        if (_serviceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }

        base.OnExit(e);
    }
}
