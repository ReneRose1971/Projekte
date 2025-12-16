using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Common.Extensions
{
    /// <summary>
    /// Extensions zur automatischen Registrierung von <see cref="IEqualityComparer{T}"/>-Implementierungen
    /// aus Assemblies in <see cref="IServiceCollection"/>.
    /// </summary>
    public static class ServiceCollectionEqualityComparerExtensions
    {
        /// <summary>
        /// Scannt die Assembly des angegebenen Marker-Typs nach allen konkreten Implementierungen
        /// von <see cref="IEqualityComparer{T}"/> und registriert sie als Singleton.
        /// </summary>
        /// <typeparam name="TMarker">
        /// Ein beliebiger Typ aus der zu scannenden Assembly (z.B. das ServiceModule selbst).
        /// </typeparam>
        /// <param name="services">Die zu erweiternde <see cref="IServiceCollection"/>.</param>
        /// <returns>Die erweiterte <see cref="IServiceCollection"/> für Fluent-API.</returns>
        /// <exception cref="ArgumentNullException">
        /// Wenn <paramref name="services"/> <c>null</c> ist.
        /// </exception>
        /// <remarks>
        /// <para>
        /// <b>Filter-Kriterien:</b> Diese Methode findet nur Typen, die <b>alle</b> folgenden Bedingungen erfüllen:
        /// </para>
        /// <list type="bullet">
        /// <item>Konkrete Klassen (nicht abstract, nicht interface)</item>
        /// <item>Öffentlich (<c>IsPublic</c>) oder nested public (<c>IsNestedPublic</c>)</item>
        /// <item>Haben einen öffentlichen parameterlosen Konstruktor</item>
        /// <item>Keine offenen generischen Typen (kein <c>ContainsGenericParameters</c>)</item>
        /// <item>Implementieren <see cref="IEqualityComparer{T}"/></item>
        /// </list>
        /// <para>
        /// <b>Registrierung:</b> Gefundene Comparer werden als Singleton mittels 
        /// <see cref="ServiceCollectionDescriptorExtensions.TryAddSingleton(IServiceCollection, Type, Type)"/> registriert,
        /// d.h. bestehende Registrierungen werden nicht überschrieben (idempotent).
        /// </para>
        /// <para>
        /// <b>Fehlerbehandlung:</b> <see cref="ReflectionTypeLoadException"/> wird automatisch behandelt -
        /// nur erfolgreich geladene Typen werden verarbeitet.
        /// </para>
        /// <para>
        /// <b>Hinweis:</b> Offene generische Typen wie <c>FallbackEqualsComparer&lt;T&gt;</c> werden
        /// automatisch übersprungen, da sie nicht instanziiert werden können. Nur geschlossene
        /// generische Typen (z.B. <c>CustomerComparer : IEqualityComparer&lt;Customer&gt;</c>) werden gefunden.
        /// </para>
        /// </remarks>
        /// <example>
        /// Typische Verwendung in einem ServiceModule:
        /// <code>
        /// public class MyModule : IServiceModule
        /// {
        ///     public void Register(IServiceCollection services)
        ///     {
        ///         // Scannt die Assembly nach allen Comparer-Implementierungen
        ///         services.AddEqualityComparersFromAssembly&lt;MyModule&gt;();
        ///     }
        /// }
        /// 
        /// // Automatisch gefundene Comparer (im gleichen Assembly):
        /// public class CustomerComparer : IEqualityComparer&lt;Customer&gt;
        /// {
        ///     public bool Equals(Customer? x, Customer? y) => x?.Id == y?.Id;
        ///     public int GetHashCode(Customer obj) => obj.Id.GetHashCode();
        /// }
        /// </code>
        /// </example>
        public static IServiceCollection AddEqualityComparersFromAssembly<TMarker>(this IServiceCollection services)
        {
            if (services is null)
                throw new ArgumentNullException(nameof(services));

            var assembly = typeof(TMarker).Assembly;

            foreach (var type in SafeGetTypes(assembly))
            {
                // Nur konkrete Klassen berücksichtigen
                if (type.IsAbstract || type.IsInterface)
                    continue;

                // Nur öffentliche Typen (auch nested public)
                if (!type.IsPublic && !type.IsNestedPublic)
                    continue;

                // Offene generische Typen überspringen (z.B. FallbackEqualsComparer<T>)
                if (type.ContainsGenericParameters)
                    continue;

                // Nur Typen mit öffentlichem parameterlosen Konstruktor
                if (!HasPublicParameterlessConstructor(type))
                    continue;

                // Alle IEqualityComparer<T>-Interfaces finden, die dieser Typ implementiert
                var comparerInterfaces = type
                    .GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEqualityComparer<>))
                    .Distinct();

                foreach (var serviceType in comparerInterfaces)
                {
                    // Idempotent registrieren: TryAdd überschreibt keine bestehenden Registrierungen
                    services.TryAddSingleton(serviceType, type);
                }
            }

            return services;
        }

        /// <summary>
        /// Prüft, ob ein Typ einen öffentlichen parameterlosen Konstruktor hat.
        /// </summary>
        /// <param name="type">Der zu prüfende Typ.</param>
        /// <returns>
        /// <c>true</c>, wenn ein öffentlicher parameterloser Konstruktor existiert; andernfalls <c>false</c>.
        /// </returns>
        private static bool HasPublicParameterlessConstructor(Type type)
        {
            return type.GetConstructor(
                BindingFlags.Public | BindingFlags.Instance,
                null,
                Type.EmptyTypes,
                null) != null;
        }

        /// <summary>
        /// Liefert bei TypeLoad-Problemen nur die erfolgreich ladbaren Typen zurück.
        /// </summary>
        /// <param name="assembly">Die zu scannende Assembly.</param>
        /// <returns>
        /// Alle erfolgreich geladenen Typen. Bei <see cref="ReflectionTypeLoadException"/>
        /// werden nur die nicht-null Typen aus <see cref="ReflectionTypeLoadException.Types"/> zurückgegeben.
        /// </returns>
        private static IEnumerable<Type> SafeGetTypes(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                return ex.Types.Where(t => t != null)!;
            }
        }
    }
}
