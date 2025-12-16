using System;
using DataToolKit.Abstractions.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace DataToolKit.Storage.Repositories
{
    /// <summary>
    /// Factory, die Repositories aus dem DI-Container auflöst.
    /// Die eigentlichen Repository-Instanzen sind als Singletons registriert.
    /// </summary>
    public sealed class RepositoryFactory : IRepositoryFactory
    {
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Erstellt eine neue <see cref="RepositoryFactory"/>.
        /// </summary>
        /// <param name="serviceProvider">Der DI-Container.</param>
        public RepositoryFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        /// <inheritdoc/>
        public IRepositoryBase<T> GetJsonRepository<T>()
        {
            var repo = _serviceProvider.GetService<IRepositoryBase<T>>();
            if (repo is null)
            {
                throw new InvalidOperationException(
                    $"Kein JSON-Repository für {typeof(T).Name} registriert. " +
                    $"Bitte services.AddJsonRepository<{typeof(T).Name}>() aufrufen.");
            }

            // Sicherstellen, dass es wirklich ein JsonRepository ist
            if (repo is not JsonRepository<T>)
            {
                throw new InvalidOperationException(
                    $"Das registrierte IRepositoryBase<{typeof(T).Name}> ist kein JsonRepository. " +
                    $"Typ: {repo.GetType().Name}");
            }

            return repo;
        }

        /// <inheritdoc/>
        public IRepository<T> GetLiteDbRepository<T>() where T : class
        {
            var repo = _serviceProvider.GetService<IRepository<T>>();
            if (repo is null)
            {
                throw new InvalidOperationException(
                    $"Kein LiteDB-Repository für {typeof(T).Name} registriert. " +
                    $"Bitte services.AddLiteDbRepository<{typeof(T).Name}>() aufrufen.");
            }

            return repo;
        }
    }
}
