using DataToolKit.Abstractions;
using DataToolKit.Abstractions.Repositories;
using DataToolKit.Storage.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;

namespace DataToolKit.Abstractions.DI
{
    /// <summary>
    /// Extensions zur vereinfachten Registrierung von Repository-Implementierungen als Singletons.
    /// Jede Extension registriert automatisch die zugehörigen <see cref="IStorageOptions{T}"/> und das Repository.
    /// </summary>
    public static class RepositoryRegistrationExtensions
    {
        /// <summary>
        /// Registriert ein JSON-Repository für <typeparamref name="T"/> als Singleton.
        /// Erstellt und registriert automatisch die <see cref="JsonStorageOptions{T}"/>.
        /// </summary>
        /// <typeparam name="T">Der Entitätstyp.</typeparam>
        /// <param name="services">Die Service-Collection.</param>
        /// <param name="appSubFolder">
        /// Anwendungs-Unterordner in "Eigene Dokumente" (z.B. "MyApp").
        /// Dieser Ordner wird unterhalb von <c>Environment.SpecialFolder.MyDocuments</c> erstellt.
        /// </param>
        /// <param name="fileNameBase">
        /// Basisname der JSON-Datei ohne Erweiterung (z.B. "customers" ? "customers.json").
        /// </param>
        /// <param name="subFolder">
        /// Optionaler zusätzlicher Unterordner innerhalb von <paramref name="appSubFolder"/> (z.B. "Data").
        /// Kann null sein für keine weitere Verschachtelung.
        /// </param>
        /// <returns>Die Service-Collection für Fluent-API.</returns>
        /// <remarks>
        /// <para>
        /// <b>Automatische Registrierung:</b>
        /// </para>
        /// <list type="bullet">
        /// <item><see cref="IStorageOptions{T}"/> ? <see cref="JsonStorageOptions{T}"/> (Singleton)</item>
        /// <item><see cref="IRepositoryBase{T}"/> ? <see cref="JsonRepository{T}"/> (Singleton)</item>
        /// </list>
        /// <para>
        /// <b>Beispiel-Pfad:</b> <c>C:\Users\Name\Documents\MyApp\Data\customers.json</c>
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// // Vereinfachte Registrierung (alles in einem Aufruf)
        /// services.AddJsonRepository&lt;Customer&gt;("MyApp", "customers", "Data");
        /// 
        /// // Verwendung via DI
        /// public class CustomerService
        /// {
        ///     public CustomerService(IRepositoryBase&lt;Customer&gt; repository) { }
        /// }
        /// </code>
        /// </example>
        public static IServiceCollection AddJsonRepository<T>(
            this IServiceCollection services,
            string appSubFolder,
            string fileNameBase,
            string? subFolder = null)
        {
            return AddJsonRepositoryInternal<T>(services, appSubFolder, fileNameBase, subFolder, rootFolder: null);
        }

        /// <summary>
        /// Interne Überladung für Tests: Erlaubt die Angabe eines benutzerdefinierten Root-Ordners.
        /// </summary>
        internal static IServiceCollection AddJsonRepositoryInternal<T>(
            this IServiceCollection services,
            string appSubFolder,
            string fileNameBase,
            string? subFolder,
            string? rootFolder)
        {
            // 1) IStorageOptions<T> registrieren
            services.TryAddSingleton<IStorageOptions<T>>(
                new JsonStorageOptions<T>(appSubFolder, fileNameBase, subFolder, rootFolder));

            // 2) IRepositoryBase<T> registrieren
            services.TryAddSingleton<IRepositoryBase<T>>(sp =>
            {
                var options = sp.GetRequiredService<IStorageOptions<T>>();
                return new JsonRepository<T>(options);
            });

            return services;
        }

        /// <summary>
        /// Registriert ein LiteDB-Repository für <typeparamref name="T"/> als Singleton.
        /// Erstellt und registriert automatisch die <see cref="LiteDbStorageOptions{T}"/>.
        /// </summary>
        /// <typeparam name="T">Der Entitätstyp (muss von <see cref="EntityBase"/> erben).</typeparam>
        /// <param name="services">Die Service-Collection.</param>
        /// <param name="appSubFolder">
        /// Anwendungs-Unterordner in "Eigene Dokumente" (z.B. "MyApp").
        /// Dieser Ordner wird unterhalb von <c>Environment.SpecialFolder.MyDocuments</c> erstellt.
        /// </param>
        /// <param name="fileNameBase">
        /// Basisname der LiteDB-Datenbankdatei ohne Erweiterung (z.B. "orders" ? "orders.db").
        /// </param>
        /// <param name="subFolder">
        /// Optionaler zusätzlicher Unterordner innerhalb von <paramref name="appSubFolder"/> (z.B. "Databases").
        /// Kann null sein für keine weitere Verschachtelung.
        /// </param>
        /// <returns>Die Service-Collection für Fluent-API.</returns>
        /// <remarks>
        /// <para>
        /// <b>Automatische Registrierung:</b>
        /// </para>
        /// <list type="bullet">
        /// <item><see cref="IStorageOptions{T}"/> ? <see cref="LiteDbStorageOptions{T}"/> (Singleton)</item>
        /// <item><see cref="IRepositoryBase{T}"/> ? <see cref="LiteDbRepository{T}"/> (Singleton)</item>
        /// <item><see cref="IRepository{T}"/> ? Gleiche Instanz wie IRepositoryBase (für Update/Delete)</item>
        /// </list>
        /// <para>
        /// <b>Benötigt:</b> <see cref="IEqualityComparer{T}"/> muss bereits im DI-Container registriert sein
        /// (z.B. via <c>CommonBootstrapServiceModule</c> ? <c>FallbackEqualsComparer&lt;T&gt;</c>).
        /// </para>
        /// <para>
        /// <b>Beispiel-Pfad:</b> <c>C:\Users\Name\Documents\MyApp\Databases\orders.db</c>
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// // Vereinfachte Registrierung (alles in einem Aufruf)
        /// services.AddLiteDbRepository&lt;Order&gt;("MyApp", "orders", "Databases");
        /// 
        /// // Verwendung via DI
        /// public class OrderService
        /// {
        ///     public OrderService(IRepository&lt;Order&gt; repository) { }
        /// }
        /// </code>
        /// </example>
        public static IServiceCollection AddLiteDbRepository<T>(
            this IServiceCollection services,
            string appSubFolder,
            string fileNameBase,
            string? subFolder = null)
            where T : EntityBase
        {
            return AddLiteDbRepositoryInternal<T>(services, appSubFolder, fileNameBase, subFolder, rootFolder: null);
        }

        /// <summary>
        /// Interne Überladung für Tests: Erlaubt die Angabe eines benutzerdefinierten Root-Ordners.
        /// </summary>
        internal static IServiceCollection AddLiteDbRepositoryInternal<T>(
            this IServiceCollection services,
            string appSubFolder,
            string fileNameBase,
            string? subFolder,
            string? rootFolder)
            where T : EntityBase
        {
            // 1) IStorageOptions<T> registrieren
            services.TryAddSingleton<IStorageOptions<T>>(
                new LiteDbStorageOptions<T>(appSubFolder, fileNameBase, subFolder, rootFolder));

            // 2) IRepositoryBase<T> registrieren
            services.TryAddSingleton<IRepositoryBase<T>>(sp =>
            {
                var options = sp.GetRequiredService<IStorageOptions<T>>();
                var comparer = sp.GetRequiredService<IEqualityComparer<T>>();
                return new LiteDbRepository<T>(options, comparer);
            });

            // 3) IRepository<T> registrieren (gleiche Instanz!)
            services.TryAddSingleton<IRepository<T>>(sp =>
                (IRepository<T>)sp.GetRequiredService<IRepositoryBase<T>>());

            return services;
        }
    }
}
