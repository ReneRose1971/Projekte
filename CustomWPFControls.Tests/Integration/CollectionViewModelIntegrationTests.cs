using System;
using System.Collections.Generic;
using System.Linq;
using CustomWPFControls.Factories;
using CustomWPFControls.Tests.Testing;
using CustomWPFControls.ViewModels;
using DataToolKit.Abstractions.DataStores;
using Xunit;

namespace CustomWPFControls.Tests.Integration
{
    /// <summary>
    /// Integration-Tests für CollectionViewModel mit DataStore-Integration.
    /// </summary>
    public class CollectionViewModelIntegrationTests
    {
        #region Setup Helpers

        private CollectionViewModel<TestModel, TestViewModel> CreateCollectionViewModel(
            IDataStore<TestModel>? dataStore = null,
            IViewModelFactory<TestModel, TestViewModel>? factory = null,
            IEqualityComparer<TestModel>? comparer = null)
        {
            dataStore ??= WPFCOntrolsTestHelpers.CreateDataStore();
            
            var serviceProvider = WPFCOntrolsTestHelpers.CreateServiceProvider();
            factory ??= serviceProvider.GetService(typeof(IViewModelFactory<TestModel, TestViewModel>)) 
                as IViewModelFactory<TestModel, TestViewModel>;
            comparer ??= serviceProvider.GetService(typeof(IEqualityComparer<TestModel>)) 
                as IEqualityComparer<TestModel>;

            return new CollectionViewModel<TestModel, TestViewModel>(
                dataStore!, factory!, comparer!);
        }

        #endregion

        #region Constructor & Initialization Tests

        [Fact]
        public void Constructor_WithValidDependencies_CreatesInstance()
        {
            // Arrange & Act
            var viewModel = CreateCollectionViewModel();

            // Assert
            Assert.NotNull(viewModel);
            Assert.NotNull(viewModel.Items);
            Assert.Equal(0, viewModel.Count);
        }

        [Fact]
        public void Constructor_WithNullDataStore_ThrowsArgumentNullException()
        {
            // Arrange
            var serviceProvider = WPFCOntrolsTestHelpers.CreateServiceProvider();
            var factory = serviceProvider.GetService(typeof(IViewModelFactory<TestModel, TestViewModel>)) 
                as IViewModelFactory<TestModel, TestViewModel>;
            var comparer = serviceProvider.GetService(typeof(IEqualityComparer<TestModel>)) 
                as IEqualityComparer<TestModel>;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new CollectionViewModel<TestModel, TestViewModel>(null!, factory!, comparer!));
        }

        [Fact]
        public void Constructor_WithNullFactory_ThrowsArgumentNullException()
        {
            // Arrange
            var dataStore = WPFCOntrolsTestHelpers.CreateDataStore();
            var serviceProvider = WPFCOntrolsTestHelpers.CreateServiceProvider();
            var comparer = serviceProvider.GetService(typeof(IEqualityComparer<TestModel>)) 
                as IEqualityComparer<TestModel>;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new CollectionViewModel<TestModel, TestViewModel>(dataStore, null!, comparer!));
        }

        [Fact]
        public void Constructor_WithNullComparer_ThrowsArgumentNullException()
        {
            // Arrange
            var dataStore = WPFCOntrolsTestHelpers.CreateDataStore();
            var serviceProvider = WPFCOntrolsTestHelpers.CreateServiceProvider();
            var factory = serviceProvider.GetService(typeof(IViewModelFactory<TestModel, TestViewModel>)) 
                as IViewModelFactory<TestModel, TestViewModel>;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new CollectionViewModel<TestModel, TestViewModel>(dataStore, factory!, null!));
        }

        [Fact]
        public void Constructor_WithPrePopulatedDataStore_CreatesInitialViewModels()
        {
            // Arrange
            var dataStore = WPFCOntrolsTestHelpers.CreateDataStore();
            var models = WPFCOntrolsTestHelpers.CreateTestModels(3);
            foreach (var model in models)
            {
                dataStore.Add(model);
            }

            // Act
            var viewModel = CreateCollectionViewModel(dataStore);

            // Assert
            Assert.Equal(3, viewModel.Count);
            Assert.Equal(3, viewModel.Items.Count);
            Assert.All(viewModel.Items, vm => Assert.NotNull(vm.Model));
        }

        #endregion

        #region DataStore ? ViewModels Synchronization Tests

        [Fact]
        public void DataStoreAdd_CreatesNewViewModel()
        {
            // Arrange
            var dataStore = WPFCOntrolsTestHelpers.CreateDataStore();
            var viewModel = CreateCollectionViewModel(dataStore);
            var model = WPFCOntrolsTestHelpers.CreateTestModel(1, "Model1");

            // Act
            dataStore.Add(model);

            // Assert
            Assert.Equal(1, viewModel.Count);
            Assert.Single(viewModel.Items);
            Assert.Same(model, viewModel.Items[0].Model);
        }

        [Fact]
        public void DataStoreAddMultiple_CreatesMultipleViewModels()
        {
            // Arrange
            var dataStore = WPFCOntrolsTestHelpers.CreateDataStore();
            var viewModel = CreateCollectionViewModel(dataStore);
            var models = WPFCOntrolsTestHelpers.CreateTestModels(5);

            // Act
            foreach (var model in models)
            {
                dataStore.Add(model);
            }

            // Assert
            Assert.Equal(5, viewModel.Count);
            Assert.Equal(5, viewModel.Items.Count);
        }

        [Fact]
        public void DataStoreRemove_RemovesViewModel()
        {
            // Arrange
            var dataStore = WPFCOntrolsTestHelpers.CreateDataStore();
            var model = WPFCOntrolsTestHelpers.CreateTestModel(1, "Model1");
            dataStore.Add(model);
            var viewModel = CreateCollectionViewModel(dataStore);

            // Act
            dataStore.Remove(model);

            // Assert
            Assert.Equal(0, viewModel.Count);
            Assert.Empty(viewModel.Items);
        }

        [Fact]
        public void DataStoreRemoveMultiple_RemovesMultipleViewModels()
        {
            // Arrange
            var dataStore = WPFCOntrolsTestHelpers.CreateDataStore();
            var models = WPFCOntrolsTestHelpers.CreateTestModels(5);
            foreach (var model in models)
            {
                dataStore.Add(model);
            }
            var viewModel = CreateCollectionViewModel(dataStore);

            // Act
            dataStore.Remove(models[0]);
            dataStore.Remove(models[2]);
            dataStore.Remove(models[4]);

            // Assert
            Assert.Equal(2, viewModel.Count);
            Assert.Contains(viewModel.Items, vm => vm.Model == models[1]);
            Assert.Contains(viewModel.Items, vm => vm.Model == models[3]);
        }

        [Fact]
        public void DataStoreClear_RemovesAllViewModels()
        {
            // Arrange
            var dataStore = WPFCOntrolsTestHelpers.CreateDataStore();
            var models = WPFCOntrolsTestHelpers.CreateTestModels(5);
            foreach (var model in models)
            {
                dataStore.Add(model);
            }
            var viewModel = CreateCollectionViewModel(dataStore);

            // Act
            dataStore.Clear();

            // Assert
            Assert.Equal(0, viewModel.Count);
            Assert.Empty(viewModel.Items);
        }

        #endregion

        #region ViewModels ? DataStore Synchronization Tests

        [Fact]
        public void AddModel_AddsToDataStore()
        {
            // Arrange
            var dataStore = WPFCOntrolsTestHelpers.CreateDataStore();
            var viewModel = CreateCollectionViewModel(dataStore);
            var model = WPFCOntrolsTestHelpers.CreateTestModel(1, "Model1");

            // Act
            var result = viewModel.AddModel(model);

            // Assert
            Assert.True(result);
            Assert.Equal(1, dataStore.Count);
            Assert.Equal(1, viewModel.Count);
        }

        [Fact]
        public void AddModel_DuplicateModel_ReturnsFalse()
        {
            // Arrange
            var dataStore = WPFCOntrolsTestHelpers.CreateDataStore();
            var viewModel = CreateCollectionViewModel(dataStore);
            var model = WPFCOntrolsTestHelpers.CreateTestModel(1, "Model1");
            viewModel.AddModel(model);

            // Act
            var result = viewModel.AddModel(model);

            // Assert
            Assert.False(result);
            Assert.Equal(1, dataStore.Count);
            Assert.Equal(1, viewModel.Count);
        }

        [Fact]
        public void RemoveModel_RemovesFromDataStore()
        {
            // Arrange
            var dataStore = WPFCOntrolsTestHelpers.CreateDataStore();
            var viewModel = CreateCollectionViewModel(dataStore);
            var model = WPFCOntrolsTestHelpers.CreateTestModel(1, "Model1");
            viewModel.AddModel(model);

            // Act
            var result = viewModel.RemoveModel(model);

            // Assert
            Assert.True(result);
            Assert.Equal(0, dataStore.Count);
            Assert.Equal(0, viewModel.Count);
        }

        [Fact]
        public void RemoveModel_NonExistentModel_ReturnsFalse()
        {
            // Arrange
            var dataStore = WPFCOntrolsTestHelpers.CreateDataStore();
            var viewModel = CreateCollectionViewModel(dataStore);
            var model1 = WPFCOntrolsTestHelpers.CreateTestModel(1, "Model1");
            var model2 = WPFCOntrolsTestHelpers.CreateTestModel(2, "Model2");
            viewModel.AddModel(model1);

            // Act
            var result = viewModel.RemoveModel(model2);

            // Assert
            Assert.False(result);
            Assert.Equal(1, dataStore.Count);
            Assert.Equal(1, viewModel.Count);
        }

        [Fact]
        public void RemoveViewModel_RemovesFromDataStore()
        {
            // Arrange
            var dataStore = WPFCOntrolsTestHelpers.CreateDataStore();
            var viewModel = CreateCollectionViewModel(dataStore);
            var model = WPFCOntrolsTestHelpers.CreateTestModel(1, "Model1");
            viewModel.AddModel(model);
            var vm = viewModel.Items.First();

            // Act
            var result = viewModel.RemoveViewModel(vm);

            // Assert
            Assert.True(result);
            Assert.Equal(0, dataStore.Count);
            Assert.Equal(0, viewModel.Count);
        }

        [Fact]
        public void Clear_ClearsDataStore()
        {
            // Arrange
            var dataStore = WPFCOntrolsTestHelpers.CreateDataStore();
            var viewModel = CreateCollectionViewModel(dataStore);
            var models = WPFCOntrolsTestHelpers.CreateTestModels(5);
            foreach (var model in models)
            {
                viewModel.AddModel(model);
            }

            // Act
            viewModel.Clear();

            // Assert
            Assert.Equal(0, dataStore.Count);
            Assert.Equal(0, viewModel.Count);
        }

        #endregion

        #region SelectedItem Tests

        [Fact]
        public void SelectedItem_CanBeSetAndGet()
        {
            // Arrange
            var dataStore = WPFCOntrolsTestHelpers.CreateDataStore();
            var viewModel = CreateCollectionViewModel(dataStore);
            var model = WPFCOntrolsTestHelpers.CreateTestModel(1, "Model1");
            viewModel.AddModel(model);
            var vm = viewModel.Items.First();

            // Act
            viewModel.SelectedItem = vm;

            // Assert
            Assert.Same(vm, viewModel.SelectedItem);
        }

        [Fact]
        public void SelectedItem_CanBeSetToNull()
        {
            // Arrange
            var dataStore = WPFCOntrolsTestHelpers.CreateDataStore();
            var viewModel = CreateCollectionViewModel(dataStore);
            var model = WPFCOntrolsTestHelpers.CreateTestModel(1, "Model1");
            viewModel.AddModel(model);
            viewModel.SelectedItem = viewModel.Items.First();

            // Act
            viewModel.SelectedItem = null;

            // Assert
            Assert.Null(viewModel.SelectedItem);
        }

        [Fact]
        public void SelectedItem_RaisesPropertyChanged()
        {
            // Arrange
            var dataStore = WPFCOntrolsTestHelpers.CreateDataStore();
            var viewModel = CreateCollectionViewModel(dataStore);
            var model = WPFCOntrolsTestHelpers.CreateTestModel(1, "Model1");
            viewModel.AddModel(model);
            var vm = viewModel.Items.First();

            var propertyChangedRaised = false;
            viewModel.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(viewModel.SelectedItem))
                    propertyChangedRaised = true;
            };

            // Act
            viewModel.SelectedItem = vm;

            // Assert
            Assert.True(propertyChangedRaised);
        }

        #endregion

        #region Count & PropertyChanged Tests

        [Fact]
        public void Count_UpdatesWhenItemsAdded()
        {
            // Arrange
            var dataStore = WPFCOntrolsTestHelpers.CreateDataStore();
            var viewModel = CreateCollectionViewModel(dataStore);

            // Act & Assert
            Assert.Equal(0, viewModel.Count);
            
            viewModel.AddModel(WPFCOntrolsTestHelpers.CreateTestModel(1));
            Assert.Equal(1, viewModel.Count);
            
            viewModel.AddModel(WPFCOntrolsTestHelpers.CreateTestModel(2));
            Assert.Equal(2, viewModel.Count);
        }

        [Fact]
        public void Count_RaisesPropertyChangedWhenItemsAdded()
        {
            // Arrange
            var dataStore = WPFCOntrolsTestHelpers.CreateDataStore();
            var viewModel = CreateCollectionViewModel(dataStore);
            var propertyChangedRaised = false;
            
            viewModel.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(viewModel.Count))
                    propertyChangedRaised = true;
            };

            // Act
            dataStore.Add(WPFCOntrolsTestHelpers.CreateTestModel(1));

            // Assert
            Assert.True(propertyChangedRaised);
        }

        #endregion

        #region IEqualityComparer Tests

        [Fact]
        public void AddModel_UsesComparerForDuplicateDetection()
        {
            // Arrange
            var comparer = new TestModelIdComparer();
            var dataStore = WPFCOntrolsTestHelpers.CreateDataStore(comparer);
            var viewModel = CreateCollectionViewModel(dataStore, comparer: comparer);

            var model1 = new TestModel { Id = 1, Name = "Name1" };
            var model2 = new TestModel { Id = 1, Name = "Name2" }; // Same Id, different Name

            // Act
            var result1 = viewModel.AddModel(model1);
            var result2 = viewModel.AddModel(model2);

            // Assert
            Assert.True(result1);
            Assert.False(result2); // Duplicate detected by comparer
            Assert.Equal(1, viewModel.Count);
        }

        #endregion

        #region Dispose Tests

        [Fact]
        public void Dispose_UnsubscribesFromDataStore()
        {
            // Arrange
            var dataStore = WPFCOntrolsTestHelpers.CreateDataStore();
            var viewModel = CreateCollectionViewModel(dataStore);
            var model = WPFCOntrolsTestHelpers.CreateTestModel(1);

            // Act
            viewModel.Dispose();
            dataStore.Add(model); // Should not update viewModel

            // Assert
            Assert.Equal(0, viewModel.Count); // Still 0 because unsubscribed
        }

        [Fact]
        public void Dispose_ClearsViewModels()
        {
            // Arrange
            var dataStore = WPFCOntrolsTestHelpers.CreateDataStore();
            var models = WPFCOntrolsTestHelpers.CreateTestModels(3);
            foreach (var model in models)
            {
                dataStore.Add(model);
            }
            var viewModel = CreateCollectionViewModel(dataStore);

            // Act
            viewModel.Dispose();

            // Assert
            Assert.Equal(0, viewModel.Count);
            Assert.Empty(viewModel.Items);
        }

        #endregion

        #region Helper Classes

        private class TestModelIdComparer : IEqualityComparer<TestModel>
        {
            public bool Equals(TestModel? x, TestModel? y)
            {
                if (x == null || y == null) return false;
                return x.Id == y.Id;
            }

            public int GetHashCode(TestModel obj)
            {
                return obj.Id.GetHashCode();
            }
        }

        #endregion
    }
}
