using Common.Extensions;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;

namespace Common.Bootstrap;

/// <summary>
/// Service-Modul für Common.BootStrap: Registriert konkrete <see cref="IEqualityComparer{T}"/>-Implementierungen.
/// </summary>
/// <remarks>
/// Dieses Modul scannt die Assembly nach konkreten <see cref="IEqualityComparer{T}"/>-Implementierungen
/// und registriert sie automatisch als Singleton.
/// 
/// <para>
/// <b>Wichtig:</b> Es gibt keinen automatischen Fallback-Comparer mehr.
/// Entwickler müssen für jeden Typ, der einen <see cref="IEqualityComparer{T}"/> benötigt,
/// eine konkrete Implementierung registrieren.
/// </para>
/// </remarks>
/// <example>
/// Manuelle Registrierung eines Comparers:
/// <code>
/// services.AddSingleton&lt;IEqualityComparer&lt;MyType&gt;&gt;(
///     new FallbackEqualsComparer&lt;MyType&gt;());
/// </code>
/// </example>
public sealed class CommonBootstrapServiceModule : IServiceModule
{
    /// <summary>
    /// Registriert alle konkreten <see cref="IEqualityComparer{T}"/>-Implementierungen aus dieser Assembly.
    /// </summary>
    public void Register(IServiceCollection services)
    {
        // Automatisch alle konkreten IEqualityComparer<T> aus Common.Bootstrap scannen
        services.AddEqualityComparersFromAssembly<CommonBootstrapServiceModule>();
    }
}
