using Common.Bootstrap;
using CustomWPFControls.Factories;
using Microsoft.Extensions.DependencyInjection;
using SolutionBundler.Core.Models;
using SolutionBundler.WPF.ViewModels;

namespace SolutionBundler.WPF;

/// <summary>
/// Service-Modul für SolutionBundler.WPF.
/// Registriert alle WPF-spezifischen Services, ViewModels und Windows.
/// </summary>
public sealed class SolutionBundlerWpfModule : IServiceModule
{
    public void Register(IServiceCollection services)
    {
        // ViewModelFactory für ProjectInfo - Simple Factory mit Func
        services.AddSingleton<IViewModelFactory<ProjectInfo, ProjectInfoViewModel>>(
            new SimpleViewModelFactory<ProjectInfo, ProjectInfoViewModel>(
                model => new ProjectInfoViewModel(model)));

        // ViewModels
        services.AddTransient<ProjectListEditorViewModel>();

        // Windows
        services.AddTransient<MainWindowWithSplitView>();
    }
}
