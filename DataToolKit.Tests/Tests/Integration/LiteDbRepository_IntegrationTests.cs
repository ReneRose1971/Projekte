using System;
using System.IO;
using System.Linq;
using DataToolKit.Abstractions.Repositories;
using DataToolKit.Storage.Repositories;
using DataToolKit.Tests.Testing;
using DataToolKit.Tests.Common;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DataToolKit.Tests.Integration
{
    /// <summary>
    /// Integration-Tests für LiteDbRepository mit DI-Container.
    /// Testet: Repository-Erzeugung, Dateipfad-Validierung, Roundtrip-Persistierung, Update/Delete.
    /// </summary>
    public class LiteDbRepository_IntegrationTests : IDisposable
    {
        private readonly ServiceProvider _serviceProvider;
        private readonly string _testDataPath;

        public LiteDbRepository_IntegrationTests()
        {
            _testDataPath = Path.Combine(Path.GetTempPath(), $"DataToolKit_LiteDbIntTest_{Guid.NewGuid():N}");
            
            var services = new ServiceCollection();
            new IntegrationTestModule(_testDataPath).Register(services);
            _serviceProvider = services.BuildServiceProvider();
        }

        [Fact]
        public void LiteDbRepository_CanBeResolvedFromDI()
        {
            // Act
            var repository = _serviceProvider.GetRequiredService<IRepository<TestEntity>>();

            // Assert
            Assert.NotNull(repository);
            Assert.IsType<LiteDbRepository<TestEntity>>(repository);
        }

        [Fact]
        public void LiteDbRepository_StorageOptions_AreCorrectlyConfigured()
        {
            // Arrange
            var options = _serviceProvider.GetRequiredService<IStorageOptions<TestEntity>>();

            // Assert
            Assert.NotNull(options);
            Assert.Contains("LiteDb", options.FullPath);
            Assert.EndsWith("TestEntity.db", options.FullPath);
        }

        [Fact]
        public void LiteDbRepository_CreatesCorrectFilePath_AfterWrite()
        {
            // Arrange
            var repository = _serviceProvider.GetRequiredService<IRepository<TestEntity>>();
            var options = _serviceProvider.GetRequiredService<IStorageOptions<TestEntity>>();

            var testData = new[]
            {
                new TestEntity { Id = 0, Name = "Alice", Index = 100 },
                new TestEntity { Id = 0, Name = "Bob", Index = 200 }
            };

            // Act
            repository.Write(testData);

            // Assert
            Assert.True(File.Exists(options.FullPath), $"File not found: {options.FullPath}");
            
            // Verify directory structure
            var directory = Path.GetDirectoryName(options.FullPath);
            Assert.True(Directory.Exists(directory), $"Directory not found: {directory}");
        }

        [Fact]
        public void LiteDbRepository_LoadAndWrite_Roundtrip_WithIdAssignment()
        {
            // Arrange
            var repository = _serviceProvider.GetRequiredService<IRepository<TestEntity>>();
            var testData = new[]
            {
                new TestEntity { Id = 0, Name = "Alice", Index = 10 },
                new TestEntity { Id = 0, Name = "Bob", Index = 20 },
                new TestEntity { Id = 0, Name = "Charlie", Index = 30 }
            };

            // Act
            repository.Write(testData);
            var loaded = repository.Load().OrderBy(e => e.Name).ToList();

            // Assert
            Assert.Equal(3, loaded.Count);
            
            // IDs wurden von LiteDB zugewiesen
            Assert.All(loaded, e => Assert.True(e.Id > 0));
            
            Assert.Equal("Alice", loaded[0].Name);
            Assert.Equal(10, loaded[0].Index);
            Assert.Equal("Bob", loaded[1].Name);
            Assert.Equal(20, loaded[1].Index);
            Assert.Equal("Charlie", loaded[2].Name);
            Assert.Equal(30, loaded[2].Index);
        }

        [Fact]
        public void LiteDbRepository_Update_ModifiesExistingEntity()
        {
            // Arrange
            var repository = _serviceProvider.GetRequiredService<IRepository<TestEntity>>();
            repository.Write(new[] { new TestEntity { Id = 0, Name = "Alice", Index = 10 } });
            
            var entity = repository.Load().First();
            entity.Name = "Alice Updated";
            entity.Index = 99;

            // Act
            repository.Update(entity);
            var loaded = repository.Load().First();

            // Assert
            Assert.Equal("Alice Updated", loaded.Name);
            Assert.Equal(99, loaded.Index);
        }

        [Fact]
        public void LiteDbRepository_Delete_RemovesEntity()
        {
            // Arrange
            var repository = _serviceProvider.GetRequiredService<IRepository<TestEntity>>();
            repository.Write(new[] 
            { 
                new TestEntity { Id = 0, Name = "Alice", Index = 10 },
                new TestEntity { Id = 0, Name = "Bob", Index = 20 }
            });
            
            var alice = repository.Load().First(e => e.Name == "Alice");

            // Act
            repository.Delete(alice);
            var remaining = repository.Load();

            // Assert
            Assert.Single(remaining);
            Assert.Equal("Bob", remaining[0].Name);
        }

        [Fact]
        public void LiteDbRepository_Clear_RemovesAllData()
        {
            // Arrange
            var repository = _serviceProvider.GetRequiredService<IRepository<TestEntity>>();
            repository.Write(new[] 
            { 
                new TestEntity { Id = 0, Name = "Alice", Index = 10 },
                new TestEntity { Id = 0, Name = "Bob", Index = 20 }
            });

            // Act
            repository.Clear();
            var loaded = repository.Load();

            // Assert
            Assert.Empty(loaded);
        }

        [Fact]
        public void LiteDbRepository_DeltaWrite_InsertsUpdatesDeletes()
        {
            // Arrange
            var repository = _serviceProvider.GetRequiredService<IRepository<TestEntity>>();
            
            // Initial data
            repository.Write(new[] 
            { 
                new TestEntity { Id = 0, Name = "Alice", Index = 10 },
                new TestEntity { Id = 0, Name = "Bob", Index = 20 }
            });
            
            var loaded = repository.Load().OrderBy(e => e.Name).ToList();
            var alice = loaded[0];
            var bob = loaded[1];

            // Act: Update Alice, Delete Bob, Insert Charlie
            alice.Index = 99;
            repository.Write(new[] 
            {
                alice,  // Update
                new TestEntity { Id = 0, Name = "Charlie", Index = 30 }  // Insert
                // Bob missing -> Delete
            });

            var final = repository.Load().OrderBy(e => e.Name).ToList();

            // Assert
            Assert.Equal(2, final.Count);
            Assert.Equal("Alice", final[0].Name);
            Assert.Equal(99, final[0].Index);
            Assert.Equal("Charlie", final[1].Name);
            Assert.Equal(30, final[1].Index);
        }

        public void Dispose()
        {
            _serviceProvider?.Dispose();
            
            if (Directory.Exists(_testDataPath))
            {
                try
                {
                    Directory.Delete(_testDataPath, recursive: true);
                }
                catch
                {
                    // Best-effort cleanup
                }
            }
        }
    }
}
