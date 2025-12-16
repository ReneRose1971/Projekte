using Microsoft.Extensions.DependencyInjection;
using SolutionBundler.Core.Abstractions;
using SolutionBundler.Core.Implementations;

namespace SolutionBundler.Core;

/// <summary>
/// Stellt Erweiterungsmethoden zur Verfügung, um die benötigten Services des SolutionBundler-Core in den DI-Container zu registrieren.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registriert die Standard-Implementierungen der Kern-Dienste (Scanner, Metadata-Reader, Klassifizierer, Hasher, Masker, Writer und Orchestrator) im angegebenen <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">Die Service-Collection, in die die Dienste eingetragen werden.</param>
    /// <returns>Die erweiterte <see cref="IServiceCollection"/>, sodass Registrierungen verkettet werden können.</returns>
    public static IServiceCollection AddSolutionBundlerCore(this IServiceCollection services)
    {
        services.AddSingleton<IFileScanner, DefaultFileScanner>();
        services.AddSingleton<IProjectMetadataReader, MsBuildProjectMetadataReader>();
        services.AddSingleton<IContentClassifier, SimpleContentClassifier>();
        services.AddSingleton<IHashCalculator, Sha1HashCalculator>();
        services.AddSingleton<ISecretMasker, RegexSecretMasker>();
        services.AddSingleton<IBundleWriter, MarkdownBundleWriter>(sp => new MarkdownBundleWriter(sp.GetRequiredService<ISecretMasker>()));
        services.AddSingleton<IBundleOrchestrator, SolutionBundler.Core.Implementations.BundleOrchestrator>();
        return services;
    }
}