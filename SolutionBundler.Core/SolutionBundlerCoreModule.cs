using Common.Bootstrap;
using Common.Extensions;
using DataToolKit.Abstractions.DI;
using Microsoft.Extensions.DependencyInjection;
using SolutionBundler.Core.Abstractions;
using SolutionBundler.Core.Implementations;
using SolutionBundler.Core.Models;
using SolutionBundler.Core.Storage;

namespace SolutionBundler.Core;

/// <summary>
/// Service-Modul für SolutionBundler.Core.
/// Registriert alle Core-Services und Implementierungen.
/// </summary>
public sealed class SolutionBundlerCoreModule : IServiceModule
{
    public void Register(IServiceCollection services)
    {
        // DataToolKit-Module registrieren (für IDataStoreProvider)
        new DataToolKitServiceModule().Register(services);
        
        // EqualityComparer automatisch registrieren
        services.AddEqualityComparersFromAssembly<SolutionBundlerCoreModule>();

        // JSON Repository für ProjectInfo registrieren
        services.AddJsonRepository<ProjectInfo>(
            appSubFolder: "SolutionBundler",
            fileNameBase: "projects");

        // ProjectStore als Singleton mit Factory-Pattern
        // Die Factory stellt sicher, dass der DataStore VOR der ProjectStore-Instanziierung erstellt wird
        services.AddSingleton<ProjectStore>(sp =>
        {
            var provider = sp.GetRequiredService<DataToolKit.Abstractions.DataStores.IDataStoreProvider>();
            var repositoryFactory = sp.GetRequiredService<DataToolKit.Storage.Repositories.IRepositoryFactory>();
            
            // WICHTIG: Erstelle PersistentDataStore über den Provider
            // Dies registriert den DataStore intern im Provider-Cache
            var dataStore = provider.GetPersistent<ProjectInfo>(
                repositoryFactory,
                isSingleton: true,
                trackPropertyChanges: false,
                autoLoad: true);
            
            // Jetzt kann ProjectStore sicher erstellt werden
            // provider.GetDataStore<ProjectInfo>() findet nun den registrierten DataStore
            return new ProjectStore(provider);
        });

        // Core-Implementierungen
        services.AddSingleton<IFileScanner, DefaultFileScanner>();
        services.AddSingleton<IProjectMetadataReader, MsBuildProjectMetadataReader>();
        services.AddSingleton<IContentClassifier, SimpleContentClassifier>();
        services.AddSingleton<IHashCalculator, Sha1HashCalculator>();
        services.AddSingleton<ISecretMasker, RegexSecretMasker>();
        services.AddSingleton<IBundleWriter, MarkdownBundleWriter>();
        services.AddSingleton<IBundleOrchestrator, BundleOrchestrator>();
    }
}
