using System;
using System.Collections.Generic;
using System.IO;
using Common.Bootstrap;
using DataToolKit.Abstractions.DataStores;
using DataToolKit.Abstractions.Repositories;
using DataToolKit.Storage.DataStores;
using DataToolKit.Storage.Repositories;
using DataToolKit.Tests.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace DataToolKit.Tests.Common
{
    /// <summary>
    /// ServiceModule für Integration-Tests.
    /// Registriert Repositories, Storage-Options, EqualityComparer und DataStore-Infrastructure.
    /// </summary>
    public class IntegrationTestModule : IServiceModule
    {
        private readonly string _testDataPath;

        /// <summary>
        /// Erstellt ein IntegrationTestModule.
        /// </summary>
        /// <param name="testDataPath">
        /// Root-Verzeichnis für Test-Daten. Wenn null, wird ein temporärer Pfad verwendet.
        /// </param>
        public IntegrationTestModule(string? testDataPath = null)
        {
            _testDataPath = testDataPath ?? Path.Combine(Path.GetTempPath(), "DataToolKit_IntegrationTests");
        }

        public void Register(IServiceCollection services)
        {
            // ============================================================
            // JSON Storage für TestDto
            // ============================================================
            services.AddSingleton<IStorageOptions<TestDto>>(sp =>
                new JsonStorageOptions<TestDto>(
                    appSubFolder: "DataToolKit.Tests",
                    fileNameBase: "TestDto",
                    subFolder: Path.Combine(_testDataPath, "Json")));

            services.AddSingleton<IRepositoryBase<TestDto>>(sp =>
            {
                var options = sp.GetRequiredService<IStorageOptions<TestDto>>();
                return new JsonRepository<TestDto>(options);
            });

            // ============================================================
            // LiteDB Storage für TestEntity
            // ============================================================
            services.AddSingleton<IStorageOptions<TestEntity>>(sp =>
                new LiteDbStorageOptions<TestEntity>(
                    appSubFolder: "DataToolKit.Tests",
                    fileNameBase: "TestEntity",
                    subFolder: Path.Combine(_testDataPath, "LiteDb")));

            services.AddSingleton<IRepository<TestEntity>>(sp =>
            {
                var options = sp.GetRequiredService<IStorageOptions<TestEntity>>();
                var comparer = sp.GetRequiredService<IEqualityComparer<TestEntity>>();
                return new LiteDbRepository<TestEntity>(options, comparer);
            });

            // Auch als IRepositoryBase registrieren (für GetJsonRepository-Fallback in Tests)
            services.AddSingleton<IRepositoryBase<TestEntity>>(sp =>
                sp.GetRequiredService<IRepository<TestEntity>>());

            // ============================================================
            // EqualityComparer registrieren
            // ============================================================
            services.AddSingleton<IEqualityComparer<TestDto>, TestDtoComparer>();
            services.AddSingleton<IEqualityComparer<TestEntity>, TestEntityComparer>();

            // ============================================================
            // RepositoryFactory
            // ============================================================
            services.AddSingleton<IRepositoryFactory, RepositoryFactory>();

            // ============================================================
            // DataStore Infrastructure
            // ============================================================
            services.AddSingleton<IDataStoreFactory, DataStoreFactory>();
            services.AddSingleton<IDataStoreProvider, DataStoreProvider>();
        }
    }
}
