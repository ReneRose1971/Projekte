using System;
using System.IO;
using System.Linq;
using DataToolKit.Abstractions.Repositories;
using DataToolKit.Storage.Repositories;
using DataToolKit.Tests.Common;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DataToolKit.Tests.Integration
{
    /// <summary>
    /// Integration-Tests für JsonRepository mit DI-Container.
    /// Testet: Repository-Erzeugung, Dateipfad-Validierung, Roundtrip-Persistierung.
    /// </summary>
    public class JsonRepository_IntegrationTests : IDisposable
    {
        private readonly ServiceProvider _serviceProvider;
        private readonly string _testDataPath;

        public JsonRepository_IntegrationTests()
        {
            _testDataPath = Path.Combine(Path.GetTempPath(), $"DataToolKit_JsonIntTest_{Guid.NewGuid():N}");
            
            var services = new ServiceCollection();
            new IntegrationTestModule(_testDataPath).Register(services);
            _serviceProvider = services.BuildServiceProvider();
        }

        [Fact]
        public void JsonRepository_CanBeResolvedFromDI()
        {
            // Act
            var repository = _serviceProvider.GetRequiredService<IRepositoryBase<TestDto>>();

            // Assert
            Assert.NotNull(repository);
            Assert.IsType<JsonRepository<TestDto>>(repository);
        }

        [Fact]
        public void JsonRepository_StorageOptions_AreCorrectlyConfigured()
        {
            // Arrange
            var options = _serviceProvider.GetRequiredService<IStorageOptions<TestDto>>();

            // Assert
            Assert.NotNull(options);
            Assert.Contains("Json", options.FullPath);
            Assert.EndsWith("TestDto.json", options.FullPath);
        }

        [Fact]
        public void JsonRepository_CreatesCorrectFilePath_AfterWrite()
        {
            // Arrange
            var repository = _serviceProvider.GetRequiredService<IRepositoryBase<TestDto>>();
            var options = _serviceProvider.GetRequiredService<IStorageOptions<TestDto>>();

            var testData = new[]
            {
                new TestDto { Id = 1, Name = "Alice", Index = 100 },
                new TestDto { Id = 2, Name = "Bob", Index = 200 }
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
        public void JsonRepository_LoadAndWrite_Roundtrip()
        {
            // Arrange
            var repository = _serviceProvider.GetRequiredService<IRepositoryBase<TestDto>>();
            var testData = new[]
            {
                new TestDto { Id = 1, Name = "Alice", Index = 10 },
                new TestDto { Id = 2, Name = "Bob", Index = 20 },
                new TestDto { Id = 3, Name = "Charlie", Index = 30 }
            };

            // Act
            repository.Write(testData);
            var loaded = repository.Load();

            // Assert
            Assert.Equal(3, loaded.Count);
            Assert.Equal("Alice", loaded[0].Name);
            Assert.Equal(10, loaded[0].Index);
            Assert.Equal("Bob", loaded[1].Name);
            Assert.Equal(20, loaded[1].Index);
            Assert.Equal("Charlie", loaded[2].Name);
            Assert.Equal(30, loaded[2].Index);
        }

        [Fact]
        public void JsonRepository_Clear_RemovesAllData()
        {
            // Arrange
            var repository = _serviceProvider.GetRequiredService<IRepositoryBase<TestDto>>();
            repository.Write(new[] { new TestDto { Id = 1, Name = "Test", Index = 1 } });

            // Act
            repository.Clear();
            var loaded = repository.Load();

            // Assert
            Assert.Empty(loaded);
        }

        [Fact]
        public void JsonRepository_MultipleWrites_CreateBackupFile()
        {
            // Arrange
            var repository = _serviceProvider.GetRequiredService<IRepositoryBase<TestDto>>();
            var options = _serviceProvider.GetRequiredService<IStorageOptions<TestDto>>();
            var backupPath = options.FullPath + ".bak";

            // Act
            repository.Write(new[] { new TestDto { Id = 1, Name = "First", Index = 1 } });
            repository.Write(new[] { new TestDto { Id = 2, Name = "Second", Index = 2 } });

            // Assert
            Assert.True(File.Exists(backupPath), $"Backup file not found: {backupPath}");
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
