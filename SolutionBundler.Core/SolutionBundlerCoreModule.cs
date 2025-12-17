using Common.Bootstrap;
using Common.Extensions;
using DataToolKit.Abstractions.DataStores;
using DataToolKit.Abstractions.DI;
using DataToolKit.Abstractions.Repositories;
using Microsoft.Extensions.DependencyInjection;
using SolutionBundler.Core.Abstractions;
using SolutionBundler.Core.Implementations;
using SolutionBundler.Core.Models;
using SolutionBundler.Core.Models.Persistence;
using SolutionBundler.Core.Storage;

namespace SolutionBundler.Core;

/// <summary>
/// Service-Modul für SolutionBundler.Core.
/// Registriert alle Core-Services und Implementierungen.
/// </summary>
/// <remarks>
/// <para>
/// <b>DataStore-Initialisierung:</b> DataStores werden NICHT hier initialisiert,
/// sondern durch <see cref="SolutionBundlerDataStoreInitializer"/> nach dem Build
/// des Containers. Siehe <see cref="IDataStoreInitializer"/> für Details.
/// </para>
/// </remarks>
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

        // ProjectStore als Singleton
        // WICHTIG: Der DataStore für ProjectInfo wird durch SolutionBundlerDataStoreInitializer
        // nach BuildServiceProvider() erstellt. ProjectStore holt sich den DataStore dann via
        // provider.GetDataStore<ProjectInfo>()
        services.AddSingleton<ProjectStore>(sp =>
        {
            var provider = sp.GetRequiredService<IDataStoreProvider>();
            var repository = sp.GetRequiredService<IRepositoryBase<ProjectInfo>>();
            return new ProjectStore(provider, repository);
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
