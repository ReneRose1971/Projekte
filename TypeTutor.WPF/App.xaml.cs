// src/TypeTutor.WPF/App.xaml.cs
using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using TypeTutor.Logic.Core;
using TypeTutor.Logic.Data;
using TypeTutor.Logic.Engine;
using TTVisualKeyboard.ViewModels;

namespace TypeTutor.WPF
{
    public partial class App : Application
    {
        public static IServiceProvider Services { get; private set; } = default!;

        protected override void OnStartup(StartupEventArgs e)
        {
            var sc = new ServiceCollection();

            //
            // CORE / ENGINE
            //
            sc.AddSingleton<IKeyToCharMapper, GermanKeyToCharMapper>();
            sc.AddSingleton<ITypingEngine>(sp =>
                new TypingEngine(
                    sp.GetRequiredService<IKeyToCharMapper>(),
                    CaseSensitivity.Strict));

            //
            // REPOSITORY + FACTORY
            //
            sc.AddSingleton<ILessonFactory, LessonFactory>();
            sc.AddSingleton<ILessonRepository>(sp =>
                new LessonRepository(
                    sp.GetRequiredService<ILessonFactory>(),
                    LessonStorageOptions.CreateDefault()
                ));

            // ModuleGuide repository
            sc.AddSingleton<ModuleGuideRepository>(sp => new ModuleGuideRepository(LessonStorageOptions.CreateDefault()));

            //
            // VIEWMODELS (alle Sub-ViewModels)
            //
            sc.AddSingleton<TypingTextViewModel>();
            sc.AddSingleton<KeyboardViewModel>();
            sc.AddSingleton<TypingEngineStateViewModel>();
            sc.AddSingleton<LessonListViewModel>();
            sc.AddSingleton<LessonsWindowViewModel>();
            sc.AddSingleton<LessonEditorViewModel>();
            sc.AddSingleton<ModuleGuideViewModel>(sp => new ModuleGuideViewModel(sp.GetRequiredService<ModuleGuideRepository>(), sp.GetRequiredService<ILessonRepository>()));

            // Visual keyboard VM
            sc.AddSingleton<VisualKeyboardViewModel>();

            //
            // SERVICES (für Fenster öffnen etc.)
            //
            sc.AddSingleton<ILessonBrowserService, LessonBrowserService>();
            sc.AddSingleton<ILessonEditorService, LessonEditorService>();

            //
            // WINDOWS als Transients (jedes Öffnen → neue Instanz)
            //
            sc.AddTransient<LessonsWindow>();
            sc.AddTransient<LessonEditorWindow>();
            sc.AddTransient<ModuleGuideWindow>();
            sc.AddTransient<ModuleGuideDetailsWindow>();

            //
            // FACTORY-DELEGATES für Fenster
            //
            sc.AddSingleton<Func<LessonsWindow>>(sp => () => sp.GetRequiredService<LessonsWindow>());
            sc.AddSingleton<Func<LessonEditorWindow>>(sp => () => sp.GetRequiredService<LessonEditorWindow>());
            sc.AddSingleton<Func<ModuleGuideWindow>>(sp => () => sp.GetRequiredService<ModuleGuideWindow>());
            sc.AddSingleton<Func<ModuleGuideDetailsWindow>>(sp => () => sp.GetRequiredService<ModuleGuideDetailsWindow>());

            //
            // MAINVIEWMODEL – wird weiter unten für das Menü benötigt
            //
            sc.AddSingleton<MainViewModel>();

            //
            // MENÜ-VIEWMODEL (benötigt MainViewModel + Services)
            //
            sc.AddSingleton<LessonMenuViewModel>(sp =>
                new LessonMenuViewModel(
                    sp.GetRequiredService<ILessonBrowserService>(),
                    sp.GetRequiredService<ILessonEditorService>(),
                    sp.GetRequiredService<Func<ModuleGuideWindow>>()
                ));

            //
            // KERNEL-KOMPONENTEN
            //
            sc.AddSingleton<KeyboardAdapter>();
            sc.AddSingleton<MainWindow>();

            //
            // Provider erstellen
            //
            Services = sc.BuildServiceProvider();

            //
            // Hauptfenster starten
            //
            var main = Services.GetRequiredService<MainWindow>();
            main.Show();

            base.OnStartup(e);
        }
    }
}
