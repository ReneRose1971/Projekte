using Common.Bootstrap;
using Common.Bootstrap.Defaults;
using DataToolKit.Abstractions;
using DataToolKit.Abstractions.DI;
using DataToolKit.Abstractions.Repositories;
using DataToolKit.Storage.Repositories;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using TestHelper.TestUtils;
using Xunit;

namespace DataToolKit.Tests.Abstractions.DI
{
    /// <summary>
    /// Tests für <see cref="RepositoryRegistrationExtensions"/>.
    /// Verwendet <see cref="TestDirectorySandbox"/> für saubere, isolierte Test-Umgebungen
    /// ohne MyDocuments zu verschmutzen.
    /// </summary>
    public sealed class RepositoryRegistrationExtensionsTests : IDisposable
    {
        private readonly TestDirectorySandbox _sandbox;

        public RepositoryRegistrationExtensionsTests()
        {
            _sandbox = new TestDirectorySandbox();
        }

        public void Dispose()
        {
            _sandbox.Dispose();
        }

        // Test-Entitäten
        private sealed class TestJsonEntity
        {
            public int Id { get; set; }
            public string? Name { get; set; }
        }

        private sealed class TestLiteDbEntity : EntityBase
        {
            public string? Name { get; set; }
        }

        [Fact]
        public void AddJsonRepository_Registers_IRepositoryBase()
        {
            // Arrange
            var services = new ServiceCollection();
            new CommonBootstrapServiceModule().Register(services);
            new DataToolKitServiceModule().Register(services);

            // Act - Vereinfachte Registrierung mit Sandbox
            services.AddJsonRepositoryInternal<TestJsonEntity>("TestApp", "test_json", "data", _sandbox.Root);
            var provider = services.BuildServiceProvider();

            // Assert
            var repo = provider.GetService<IRepositoryBase<TestJsonEntity>>();
            Assert.NotNull(repo);
            Assert.IsType<JsonRepository<TestJsonEntity>>(repo);
        }

        [Fact]
        public void AddJsonRepository_Returns_Singleton()
        {
            // Arrange
            var services = new ServiceCollection();
            new CommonBootstrapServiceModule().Register(services);
            new DataToolKitServiceModule().Register(services);

            services.AddJsonRepositoryInternal<TestJsonEntity>("TestApp", "test_json", "data", _sandbox.Root);
            var provider = services.BuildServiceProvider();

            // Act
            var repo1 = provider.GetRequiredService<IRepositoryBase<TestJsonEntity>>();
            var repo2 = provider.GetRequiredService<IRepositoryBase<TestJsonEntity>>();

            // Assert
            Assert.Same(repo1, repo2);
        }

        [Fact]
        public void AddJsonRepository_Registers_IStorageOptions_Automatically()
        {
            // Arrange
            var services = new ServiceCollection();
            new CommonBootstrapServiceModule().Register(services);
            new DataToolKitServiceModule().Register(services);

            // Act
            services.AddJsonRepositoryInternal<TestJsonEntity>("TestApp", "test_json", "data", _sandbox.Root);
            var provider = services.BuildServiceProvider();

            // Assert - StorageOptions wurden automatisch registriert
            var options = provider.GetService<IStorageOptions<TestJsonEntity>>();
            Assert.NotNull(options);
            Assert.IsType<JsonStorageOptions<TestJsonEntity>>(options);
            Assert.EndsWith(".json", options.FullPath);
            Assert.StartsWith(_sandbox.Root, options.FullPath);
        }

        [Fact]
        public void AddLiteDbRepository_Registers_Both_Interfaces()
        {
            // Arrange
            var services = new ServiceCollection();
            new CommonBootstrapServiceModule().Register(services);
            new DataToolKitServiceModule().Register(services);

            services.AddSingleton<IEqualityComparer<TestLiteDbEntity>>(
                new FallbackEqualsComparer<TestLiteDbEntity>());

            // Act
            services.AddLiteDbRepositoryInternal<TestLiteDbEntity>("TestApp", "test_litedb", "data", _sandbox.Root);
            var provider = services.BuildServiceProvider();

            // Assert
            var repoBase = provider.GetService<IRepositoryBase<TestLiteDbEntity>>();
            var repo = provider.GetService<IRepository<TestLiteDbEntity>>();

            Assert.NotNull(repoBase);
            Assert.NotNull(repo);
            Assert.IsType<LiteDbRepository<TestLiteDbEntity>>(repoBase);
            Assert.Same(repoBase, repo);
        }

        [Fact]
        public void AddLiteDbRepository_Returns_Singleton()
        {
            // Arrange
            var services = new ServiceCollection();
            new CommonBootstrapServiceModule().Register(services);
            new DataToolKitServiceModule().Register(services);

            services.AddSingleton<IEqualityComparer<TestLiteDbEntity>>(
                new FallbackEqualsComparer<TestLiteDbEntity>());

            services.AddLiteDbRepositoryInternal<TestLiteDbEntity>("TestApp", "test_litedb", "data", _sandbox.Root);
            var provider = services.BuildServiceProvider();

            // Act
            var repo1 = provider.GetRequiredService<IRepository<TestLiteDbEntity>>();
            var repo2 = provider.GetRequiredService<IRepository<TestLiteDbEntity>>();

            // Assert
            Assert.Same(repo1, repo2);
        }

        [Fact]
        public void AddLiteDbRepository_Registers_IStorageOptions_Automatically()
        {
            // Arrange
            var services = new ServiceCollection();
            new CommonBootstrapServiceModule().Register(services);
            new DataToolKitServiceModule().Register(services);

            services.AddSingleton<IEqualityComparer<TestLiteDbEntity>>(
                new FallbackEqualsComparer<TestLiteDbEntity>());

            // Act
            services.AddLiteDbRepositoryInternal<TestLiteDbEntity>("TestApp", "test_litedb", "data", _sandbox.Root);
            var provider = services.BuildServiceProvider();

            // Assert - StorageOptions wurden automatisch registriert
            var options = provider.GetService<IStorageOptions<TestLiteDbEntity>>();
            Assert.NotNull(options);
            Assert.IsType<LiteDbStorageOptions<TestLiteDbEntity>>(options);
            Assert.EndsWith(".db", options.FullPath);
            Assert.StartsWith(_sandbox.Root, options.FullPath);
        }

        [Fact]
        public void AddJsonRepository_Is_Idempotent()
        {
            // Arrange
            var services = new ServiceCollection();
            new CommonBootstrapServiceModule().Register(services);
            new DataToolKitServiceModule().Register(services);

            // Act - mehrfach aufrufen
            services.AddJsonRepositoryInternal<TestJsonEntity>("TestApp", "test_json", "data", _sandbox.Root);
            services.AddJsonRepositoryInternal<TestJsonEntity>("TestApp", "test_json", "data", _sandbox.Root);
            var provider = services.BuildServiceProvider();

            // Assert - sollte nicht crashen
            var repo = provider.GetRequiredService<IRepositoryBase<TestJsonEntity>>();
            Assert.NotNull(repo);
        }

        [Fact]
        public void AddLiteDbRepository_Is_Idempotent()
        {
            // Arrange
            var services = new ServiceCollection();
            new CommonBootstrapServiceModule().Register(services);
            new DataToolKitServiceModule().Register(services);

            services.AddSingleton<IEqualityComparer<TestLiteDbEntity>>(
                new FallbackEqualsComparer<TestLiteDbEntity>());

            // Act - mehrfach aufrufen
            services.AddLiteDbRepositoryInternal<TestLiteDbEntity>("TestApp", "test_litedb", "data", _sandbox.Root);
            services.AddLiteDbRepositoryInternal<TestLiteDbEntity>("TestApp", "test_litedb", "data", _sandbox.Root);
            var provider = services.BuildServiceProvider();

            // Assert - sollte nicht crashen
            var repo = provider.GetRequiredService<IRepository<TestLiteDbEntity>>();
            Assert.NotNull(repo);
        }

        [Fact]
        public void RepositoryFactory_GetJsonRepository_Returns_Correct_Instance()
        {
            // Arrange
            var services = new ServiceCollection();
            new CommonBootstrapServiceModule().Register(services);
            new DataToolKitServiceModule().Register(services);

            services.AddJsonRepositoryInternal<TestJsonEntity>("TestApp", "test_json", "data", _sandbox.Root);
            var provider = services.BuildServiceProvider();

            // Act
            var factory = provider.GetRequiredService<IRepositoryFactory>();
            var repo = factory.GetJsonRepository<TestJsonEntity>();

            // Assert
            Assert.NotNull(repo);
            Assert.IsType<JsonRepository<TestJsonEntity>>(repo);
        }

        [Fact]
        public void RepositoryFactory_GetLiteDbRepository_Returns_Correct_Instance()
        {
            // Arrange
            var services = new ServiceCollection();
            new CommonBootstrapServiceModule().Register(services);
            new DataToolKitServiceModule().Register(services);

            services.AddSingleton<IEqualityComparer<TestLiteDbEntity>>(
                new FallbackEqualsComparer<TestLiteDbEntity>());

            services.AddLiteDbRepositoryInternal<TestLiteDbEntity>("TestApp", "test_litedb", "data", _sandbox.Root);
            var provider = services.BuildServiceProvider();

            // Act
            var factory = provider.GetRequiredService<IRepositoryFactory>();
            var repo = factory.GetLiteDbRepository<TestLiteDbEntity>();

            // Assert
            Assert.NotNull(repo);
            Assert.IsType<LiteDbRepository<TestLiteDbEntity>>(repo);
        }

        [Fact]
        public void RepositoryFactory_Throws_When_Json_Repository_Not_Registered()
        {
            // Arrange
            var services = new ServiceCollection();
            new CommonBootstrapServiceModule().Register(services);
            new DataToolKitServiceModule().Register(services);

            var provider = services.BuildServiceProvider();
            var factory = provider.GetRequiredService<IRepositoryFactory>();

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() =>
                factory.GetJsonRepository<TestJsonEntity>());

            Assert.Contains("Kein JSON-Repository", ex.Message);
            Assert.Contains(nameof(TestJsonEntity), ex.Message);
        }

        [Fact]
        public void RepositoryFactory_Throws_When_LiteDb_Repository_Not_Registered()
        {
            // Arrange
            var services = new ServiceCollection();
            new CommonBootstrapServiceModule().Register(services);
            new DataToolKitServiceModule().Register(services);

            var provider = services.BuildServiceProvider();
            var factory = provider.GetRequiredService<IRepositoryFactory>();

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() =>
                factory.GetLiteDbRepository<TestLiteDbEntity>());

            Assert.Contains("Kein LiteDB-Repository", ex.Message);
            Assert.Contains(nameof(TestLiteDbEntity), ex.Message);
        }

        [Fact]
        public void AddJsonRepository_WithoutSubFolder_Works()
        {
            // Arrange
            var services = new ServiceCollection();
            new CommonBootstrapServiceModule().Register(services);
            new DataToolKitServiceModule().Register(services);

            // Act - ohne subFolder-Parameter
            services.AddJsonRepositoryInternal<TestJsonEntity>("TestApp", "test_json", null, _sandbox.Root);
            var provider = services.BuildServiceProvider();

            // Assert
            var repo = provider.GetRequiredService<IRepositoryBase<TestJsonEntity>>();
            var options = provider.GetRequiredService<IStorageOptions<TestJsonEntity>>();
            
            Assert.NotNull(repo);
            Assert.NotNull(options);
            Assert.EndsWith("test_json.json", options.FullPath);
        }

        [Fact]
        public void AddLiteDbRepository_WithoutSubFolder_Works()
        {
            // Arrange
            var services = new ServiceCollection();
            new CommonBootstrapServiceModule().Register(services);
            new DataToolKitServiceModule().Register(services);

            services.AddSingleton<IEqualityComparer<TestLiteDbEntity>>(
                new FallbackEqualsComparer<TestLiteDbEntity>());

            // Act - ohne subFolder-Parameter
            services.AddLiteDbRepositoryInternal<TestLiteDbEntity>("TestApp", "test_litedb", null, _sandbox.Root);
            var provider = services.BuildServiceProvider();

            // Assert
            var repo = provider.GetRequiredService<IRepository<TestLiteDbEntity>>();
            var options = provider.GetRequiredService<IStorageOptions<TestLiteDbEntity>>();
            
            Assert.NotNull(repo);
            Assert.NotNull(options);
            Assert.EndsWith("test_litedb.db", options.FullPath);
        }

        // ==================== END-TO-END BOOTSTRAP TESTS ====================

        [Fact]
        public void CompleteBootstrap_WithBothRepositoryTypes_Works()
        {
            // Arrange
            var services = new ServiceCollection();
            
            services.AddModulesFromAssemblies(
                typeof(CommonBootstrapServiceModule).Assembly,
                typeof(DataToolKitServiceModule).Assembly);
            
            services.AddSingleton<IEqualityComparer<TestLiteDbEntity>>(
                new FallbackEqualsComparer<TestLiteDbEntity>());
            
            services.AddJsonRepositoryInternal<TestJsonEntity>("TestApp", "json_data", "Data", _sandbox.Root);
            services.AddLiteDbRepositoryInternal<TestLiteDbEntity>("TestApp", "litedb_data", "Databases", _sandbox.Root);
            
            // Act
            var provider = services.BuildServiceProvider();
            
            // Assert
            var repositoryFactory = provider.GetRequiredService<IRepositoryFactory>();
            Assert.NotNull(repositoryFactory);
            
            var jsonRepo = repositoryFactory.GetJsonRepository<TestJsonEntity>();
            var liteDbRepo = repositoryFactory.GetLiteDbRepository<TestLiteDbEntity>();
            
            Assert.NotNull(jsonRepo);
            Assert.NotNull(liteDbRepo);
            Assert.IsType<JsonRepository<TestJsonEntity>>(jsonRepo);
            Assert.IsType<LiteDbRepository<TestLiteDbEntity>>(liteDbRepo);
        }

        [Fact]
        public void RepositoryFactory_GetJsonRepository_ReturnsSameSingleton_OnMultipleCalls()
        {
            // Arrange
            var services = new ServiceCollection();
            new CommonBootstrapServiceModule().Register(services);
            new DataToolKitServiceModule().Register(services);
            
            services.AddJsonRepositoryInternal<TestJsonEntity>("TestApp", "test_json", "data", _sandbox.Root);
            var provider = services.BuildServiceProvider();
            
            var factory = provider.GetRequiredService<IRepositoryFactory>();
            
            // Act
            var repo1 = factory.GetJsonRepository<TestJsonEntity>();
            var repo2 = factory.GetJsonRepository<TestJsonEntity>();
            var repo3 = factory.GetJsonRepository<TestJsonEntity>();
            
            // Assert
            Assert.Same(repo1, repo2);
            Assert.Same(repo2, repo3);
            Assert.Same(repo1, repo3);
        }

        [Fact]
        public void RepositoryFactory_GetLiteDbRepository_ReturnsSameSingleton_OnMultipleCalls()
        {
            // Arrange
            var services = new ServiceCollection();
            new CommonBootstrapServiceModule().Register(services);
            new DataToolKitServiceModule().Register(services);
            
            services.AddSingleton<IEqualityComparer<TestLiteDbEntity>>(
                new FallbackEqualsComparer<TestLiteDbEntity>());
            
            services.AddLiteDbRepositoryInternal<TestLiteDbEntity>("TestApp", "test_litedb", "data", _sandbox.Root);
            var provider = services.BuildServiceProvider();
            
            var factory = provider.GetRequiredService<IRepositoryFactory>();
            
            // Act
            var repo1 = factory.GetLiteDbRepository<TestLiteDbEntity>();
            var repo2 = factory.GetLiteDbRepository<TestLiteDbEntity>();
            var repo3 = factory.GetLiteDbRepository<TestLiteDbEntity>();
            
            // Assert
            Assert.Same(repo1, repo2);
            Assert.Same(repo2, repo3);
            Assert.Same(repo1, repo3);
        }

        [Fact]
        public void RepositoryFactory_And_DirectDI_ReturnSameInstance_ForJsonRepository()
        {
            // Arrange
            var services = new ServiceCollection();
            new CommonBootstrapServiceModule().Register(services);
            new DataToolKitServiceModule().Register(services);
            
            services.AddJsonRepositoryInternal<TestJsonEntity>("TestApp", "test_json", "data", _sandbox.Root);
            var provider = services.BuildServiceProvider();
            
            // Act
            var repoFromFactory = provider.GetRequiredService<IRepositoryFactory>()
                .GetJsonRepository<TestJsonEntity>();
            var repoFromDI = provider.GetRequiredService<IRepositoryBase<TestJsonEntity>>();
            
            // Assert
            Assert.Same(repoFromFactory, repoFromDI);
        }

        [Fact]
        public void RepositoryFactory_And_DirectDI_ReturnSameInstance_ForLiteDbRepository()
        {
            // Arrange
            var services = new ServiceCollection();
            new CommonBootstrapServiceModule().Register(services);
            new DataToolKitServiceModule().Register(services);
            
            services.AddSingleton<IEqualityComparer<TestLiteDbEntity>>(
                new FallbackEqualsComparer<TestLiteDbEntity>());
            
            services.AddLiteDbRepositoryInternal<TestLiteDbEntity>("TestApp", "test_litedb", "data", _sandbox.Root);
            var provider = services.BuildServiceProvider();
            
            // Act
            var repoFromFactory = provider.GetRequiredService<IRepositoryFactory>()
                .GetLiteDbRepository<TestLiteDbEntity>();
            var repoFromDI = provider.GetRequiredService<IRepository<TestLiteDbEntity>>();
            
            // Assert
            Assert.Same(repoFromFactory, repoFromDI);
        }

        [Fact]
        public void CompleteBootstrap_BothRepositories_FactorySingletonBehavior()
        {
            // Arrange
            var services = new ServiceCollection();
            
            services.AddModulesFromAssemblies(
                typeof(CommonBootstrapServiceModule).Assembly,
                typeof(DataToolKitServiceModule).Assembly);
            
            services.AddSingleton<IEqualityComparer<TestLiteDbEntity>>(
                new FallbackEqualsComparer<TestLiteDbEntity>());
            
            services.AddJsonRepositoryInternal<TestJsonEntity>("TestApp", "json_repo", "JsonData", _sandbox.Root);
            services.AddLiteDbRepositoryInternal<TestLiteDbEntity>("TestApp", "litedb_repo", "LiteDbData", _sandbox.Root);
            
            var provider = services.BuildServiceProvider();
            var factory = provider.GetRequiredService<IRepositoryFactory>();
            
            // Act
            var jsonRepo1 = factory.GetJsonRepository<TestJsonEntity>();
            var jsonRepo2 = factory.GetJsonRepository<TestJsonEntity>();
            var jsonRepoDI = provider.GetRequiredService<IRepositoryBase<TestJsonEntity>>();
            
            var liteDbRepo1 = factory.GetLiteDbRepository<TestLiteDbEntity>();
            var liteDbRepo2 = factory.GetLiteDbRepository<TestLiteDbEntity>();
            var liteDbRepoDI = provider.GetRequiredService<IRepository<TestLiteDbEntity>>();
            
            // Assert
            Assert.Same(jsonRepo1, jsonRepo2);
            Assert.Same(jsonRepo1, jsonRepoDI);
            
            Assert.Same(liteDbRepo1, liteDbRepo2);
            Assert.Same(liteDbRepo1, liteDbRepoDI);
            
            Assert.NotSame((object)jsonRepo1, (object)liteDbRepo1);
        }

        // ==================== FACTORY RETURN TYPE TESTS ====================

        [Fact]
        public void RepositoryFactory_GetJsonRepository_ReturnsConcreteJsonRepositoryType()
        {
            // Arrange
            var services = new ServiceCollection();
            new CommonBootstrapServiceModule().Register(services);
            new DataToolKitServiceModule().Register(services);
            
            services.AddJsonRepositoryInternal<TestJsonEntity>("TestApp", "test_json", "data", _sandbox.Root);
            var provider = services.BuildServiceProvider();
            
            var factory = provider.GetRequiredService<IRepositoryFactory>();
            
            // Act
            var repo = factory.GetJsonRepository<TestJsonEntity>();
            
            // Assert
            Assert.NotNull(repo);
            Assert.IsType<JsonRepository<TestJsonEntity>>(repo);
            Assert.IsAssignableFrom<IRepositoryBase<TestJsonEntity>>(repo);
        }

        [Fact]
        public void RepositoryFactory_GetLiteDbRepository_ReturnsConcreteLiteDbRepositoryType()
        {
            // Arrange
            var services = new ServiceCollection();
            new CommonBootstrapServiceModule().Register(services);
            new DataToolKitServiceModule().Register(services);
            
            services.AddSingleton<IEqualityComparer<TestLiteDbEntity>>(
                new FallbackEqualsComparer<TestLiteDbEntity>());
            
            services.AddLiteDbRepositoryInternal<TestLiteDbEntity>("TestApp", "test_litedb", "data", _sandbox.Root);
            var provider = services.BuildServiceProvider();
            
            var factory = provider.GetRequiredService<IRepositoryFactory>();
            
            // Act
            var repo = factory.GetLiteDbRepository<TestLiteDbEntity>();
            
            // Assert
            Assert.NotNull(repo);
            Assert.IsType<LiteDbRepository<TestLiteDbEntity>>(repo);
            Assert.IsAssignableFrom<IRepository<TestLiteDbEntity>>(repo);
            Assert.IsAssignableFrom<IRepositoryBase<TestLiteDbEntity>>(repo);
        }

        [Fact]
        public void RepositoryFactory_ReturnsConcretTypes_ForBothRepositoryTypes()
        {
            // Arrange
            var services = new ServiceCollection();
            
            services.AddModulesFromAssemblies(
                typeof(CommonBootstrapServiceModule).Assembly,
                typeof(DataToolKitServiceModule).Assembly);
            
            services.AddSingleton<IEqualityComparer<TestLiteDbEntity>>(
                new FallbackEqualsComparer<TestLiteDbEntity>());
            
            services.AddJsonRepositoryInternal<TestJsonEntity>("TestApp", "json_factory", "Data", _sandbox.Root);
            services.AddLiteDbRepositoryInternal<TestLiteDbEntity>("TestApp", "litedb_factory", "Databases", _sandbox.Root);
            
            var provider = services.BuildServiceProvider();
            var factory = provider.GetRequiredService<IRepositoryFactory>();
            
            // Act
            var jsonRepo = factory.GetJsonRepository<TestJsonEntity>();
            var liteDbRepo = factory.GetLiteDbRepository<TestLiteDbEntity>();
            
            // Assert
            Assert.IsType<JsonRepository<TestJsonEntity>>(jsonRepo);
            Assert.IsType<LiteDbRepository<TestLiteDbEntity>>(liteDbRepo);
            
            Assert.IsAssignableFrom<IRepositoryBase<TestJsonEntity>>(jsonRepo);
            Assert.IsAssignableFrom<IRepository<TestLiteDbEntity>>(liteDbRepo);
            Assert.IsAssignableFrom<IRepositoryBase<TestLiteDbEntity>>(liteDbRepo);
        }

        [Fact]
        public void RepositoryFactory_MultipleCallsReturnSameInstance_WithConcreteType()
        {
            // Arrange
            var services = new ServiceCollection();
            new CommonBootstrapServiceModule().Register(services);
            new DataToolKitServiceModule().Register(services);
            
            services.AddSingleton<IEqualityComparer<TestLiteDbEntity>>(
                new FallbackEqualsComparer<TestLiteDbEntity>());
            
            services.AddJsonRepositoryInternal<TestJsonEntity>("TestApp", "json_multi", "data", _sandbox.Root);
            services.AddLiteDbRepositoryInternal<TestLiteDbEntity>("TestApp", "litedb_multi", "data", _sandbox.Root);
            
            var provider = services.BuildServiceProvider();
            var factory = provider.GetRequiredService<IRepositoryFactory>();
            
            // Act
            var jsonRepo1 = factory.GetJsonRepository<TestJsonEntity>();
            var jsonRepo2 = factory.GetJsonRepository<TestJsonEntity>();
            
            var liteDbRepo1 = factory.GetLiteDbRepository<TestLiteDbEntity>();
            var liteDbRepo2 = factory.GetLiteDbRepository<TestLiteDbEntity>();
            
            // Assert
            Assert.Same(jsonRepo1, jsonRepo2);
            Assert.Same(liteDbRepo1, liteDbRepo2);
            
            Assert.IsType<JsonRepository<TestJsonEntity>>(jsonRepo1);
            Assert.IsType<JsonRepository<TestJsonEntity>>(jsonRepo2);
            Assert.IsType<LiteDbRepository<TestLiteDbEntity>>(liteDbRepo1);
            Assert.IsType<LiteDbRepository<TestLiteDbEntity>>(liteDbRepo2);
        }
    }
}
