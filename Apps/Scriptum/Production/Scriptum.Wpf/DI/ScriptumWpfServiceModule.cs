using Common.Bootstrap;
using Microsoft.Extensions.DependencyInjection;
using Scriptum.Wpf.Keyboard.ViewModels;
using Scriptum.Wpf.Navigation;
using Scriptum.Wpf.Projections.Services;
using Scriptum.Wpf.ViewModels;

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
        
        services.AddSingleton<IContentQueryService, ContentQueryService>();
        services.AddSingleton<ISessionQueryService, SessionQueryService>();
        services.AddSingleton<IStatisticsQueryService, StatisticsQueryService>();
        
        services.AddSingleton<ShellViewModel>();
        services.AddSingleton<INavigationService, NavigationService>();
        
        services.AddTransient<HomeViewModel>();
        services.AddTransient<ModuleListViewModel>();
        services.AddTransient<LessonListViewModel>();
        services.AddTransient<LessonDetailsViewModel>();
        services.AddTransient<LessonGuideViewModel>();
        services.AddTransient<TrainingViewModel>();
        services.AddTransient<TrainingSummaryViewModel>();
        services.AddTransient<SessionHistoryViewModel>();
        services.AddTransient<SessionDetailViewModel>();
        services.AddTransient<StatisticsDashboardViewModel>();
        services.AddTransient<ErrorHeatmapViewModel>();
        services.AddTransient<SettingsViewModel>();
        services.AddTransient<ContentManagementViewModel>();
        
        services.AddTransient<MainWindow>();
    }
}
