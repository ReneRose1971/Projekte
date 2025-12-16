using System.Collections.Generic;
using Common.Bootstrap.Defaults;
using CustomWPFControls.Factories;
using DataToolKit.Abstractions.DataStores;
using DataToolKit.Storage.DataStores;
using Microsoft.Extensions.DependencyInjection;

namespace CustomWPFControls.Tests.Testing
{
    /// <summary>
    /// Test-Helper-Klasse mit Factory-Methods für Test-Objekte.
    /// </summary>
    public static class WPFCOntrolsTestHelpers
    {
        /// <summary>
        /// Erstellt einen InMemoryDataStore mit TestModel.
        /// </summary>
        public static IDataStore<TestModel> CreateDataStore(IEqualityComparer<TestModel>? comparer = null)
        {
            return new InMemoryDataStore<TestModel>(comparer ?? EqualityComparer<TestModel>.Default);
        }

        /// <summary>
        /// Erstellt einen ServiceProvider mit registrierten Test-Dependencies.
        /// </summary>
        public static ServiceProvider CreateServiceProvider()
        {
            var services = new ServiceCollection();

            // EqualityComparer
            services.AddSingleton<IEqualityComparer<TestModel>>(
                new FallbackEqualsComparer<TestModel>());

            // DataStore
            services.AddSingleton<IDataStore<TestModel>>(
                provider => CreateDataStore(provider.GetRequiredService<IEqualityComparer<TestModel>>()));

            // ViewModelFactory
            services.AddViewModelFactory<TestModel, TestViewModel>();

            return services.BuildServiceProvider();
        }

        /// <summary>
        /// Erstellt ein TestModel mit Default-Werten.
        /// </summary>
        public static TestModel CreateTestModel(int id = 1, string name = "Test")
        {
            return new TestModel
            {
                Id = id,
                Name = name,
                Description = $"Description for {name}"
            };
        }

        /// <summary>
        /// Erstellt mehrere TestModels.
        /// </summary>
        public static List<TestModel> CreateTestModels(int count)
        {
            var models = new List<TestModel>();
            for (int i = 1; i <= count; i++)
            {
                models.Add(CreateTestModel(i, $"Model{i}"));
            }
            return models;
        }
    }
}
