using Common.Bootstrap;
using DataToolKit.Abstractions.DataStores;
using DataToolKit.Abstractions.Repositories;
using DataToolKit.Storage.DataStores;
using DataToolKit.Storage.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DataToolKit.Abstractions.DI;

/// <summary>
/// ServiceModule für die DataToolKit-Infrastruktur.
/// Registriert IRepositoryFactory, IDataStoreFactory, IDataStoreProvider und andere zentrale Services.
/// 
/// Repositories für spezifische Entitätstypen werden über die Extension-Methoden
/// <see cref="RepositoryRegistrationExtensions.AddJsonRepository{T}"/> und
/// <see cref="RepositoryRegistrationExtensions.AddLiteDbRepository{T}"/> registriert.
/// </summary>
/// <remarks>
/// <para>
/// <b>Abhängigkeit:</b> Dieses Modul setzt voraus, dass <see cref="CommonBootstrapServiceModule"/>
/// bereits registriert wurde, um <c>IEqualityComparer&lt;T&gt;</c> (via <c>FallbackEqualsComparer&lt;T&gt;</c>)
/// verfügbar zu machen.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // 1) Bootstrap-Prozess in Program.cs
/// var builder = Host.CreateApplicationBuilder(args);
/// 
/// // Automatische Modul-Registrierung
/// builder.Services.AddModulesFromAssemblies(
///     typeof(Program).Assembly,
///     typeof(DataToolKitServiceModule).Assembly);
/// 
/// // 2) In einem App-spezifischen ServiceModule: Repository registrieren
/// public class MyAppDataModule : IServiceModule
/// {
///     public void Register(IServiceCollection services)
///     {
///         // JSON-Repository für einfache DTOs (alles in einem Aufruf)
///         services.AddJsonRepository&lt;Customer&gt;("MyApp", "customers", "Data");
///         
///         // LiteDB-Repository für Entitäten mit EntityBase (alles in einem Aufruf)
///         services.AddLiteDbRepository&lt;Order&gt;("MyApp", "orders", "Databases");
///     }
/// }
/// 
/// // 3) Verwendung in Services via DI
/// public class CustomerService
/// {
///     private readonly IRepositoryBase&lt;Customer&gt; _customerRepo;
///     private readonly IRepository&lt;Order&gt; _orderRepo;
///     
///     public CustomerService(
///         IRepositoryBase&lt;Customer&gt; customerRepo,
///         IRepository&lt;Order&gt; orderRepo)
///     {
///         _customerRepo = customerRepo;
///         _orderRepo = orderRepo;
///     }
/// }
/// </code>
/// </example>
public sealed class DataToolKitServiceModule : IServiceModule
{
    /// <summary>
    /// Registriert alle benötigten DataToolKit-Services:
    /// - <see cref="IRepositoryFactory"/> als Singleton
    /// - <see cref="IDataStoreFactory"/> als Singleton
    /// - <see cref="IDataStoreProvider"/> als Singleton
    /// </summary>
    public void Register(IServiceCollection services)
    {
        // RepositoryFactory als Singleton registrieren
        services.TryAddSingleton<IRepositoryFactory, RepositoryFactory>();
        
        // DataStoreFactory als Singleton registrieren
        services.TryAddSingleton<IDataStoreFactory, DataStoreFactory>();
        
        // DataStoreProvider als Singleton registrieren
        services.TryAddSingleton<IDataStoreProvider, DataStoreProvider>();
    }
}
