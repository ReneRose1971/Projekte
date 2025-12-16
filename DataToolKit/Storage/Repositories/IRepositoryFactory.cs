using DataToolKit.Abstractions.Repositories;

namespace DataToolKit.Storage.Repositories
{
    /// <summary>
    /// Vereinfachte Factory zum Auflösen von Repositories aus dem DI-Container.
    /// Die eigentlichen Repository-Instanzen sind als Singletons im Container registriert.
    /// </summary>
    /// <remarks>
    /// Diese Factory dient als Alternative zur direkten DI-Injection, wenn:
    /// - Der Repository-Typ erst zur Laufzeit bekannt ist
    /// - Dynamische Auflösung basierend auf Konfiguration erforderlich ist
    /// 
    /// Für statische Szenarien ist die direkte Injection von <see cref="IRepositoryBase{T}"/> 
    /// oder <see cref="IRepository{T}"/> zu bevorzugen.
    /// </remarks>
    public interface IRepositoryFactory
    {
        /// <summary>
        /// Liefert das über DI registrierte JSON-Repository für <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Der Entitätstyp.</typeparam>
        /// <returns>Das registrierte JSON-Repository.</returns>
        /// <exception cref="System.InvalidOperationException">
        /// Wenn kein JSON-Repository für <typeparamref name="T"/> registriert wurde.
        /// </exception>
        /// <example>
        /// <code>
        /// var factory = serviceProvider.GetRequiredService&lt;IRepositoryFactory&gt;();
        /// var customerRepo = factory.GetJsonRepository&lt;Customer&gt;();
        /// </code>
        /// </example>
        IRepositoryBase<T> GetJsonRepository<T>();

        /// <summary>
        /// Liefert das über DI registrierte LiteDB-Repository für <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Der Entitätstyp.</typeparam>
        /// <returns>Das registrierte LiteDB-Repository.</returns>
        /// <exception cref="System.InvalidOperationException">
        /// Wenn kein LiteDB-Repository für <typeparamref name="T"/> registriert wurde.
        /// </exception>
        /// <example>
        /// <code>
        /// var factory = serviceProvider.GetRequiredService&lt;IRepositoryFactory&gt;();
        /// var orderRepo = factory.GetLiteDbRepository&lt;Order&gt;();
        /// orderRepo.Update(order); // Update/Delete verfügbar
        /// </code>
        /// </example>
        IRepository<T> GetLiteDbRepository<T>() where T : class;
    }
}
