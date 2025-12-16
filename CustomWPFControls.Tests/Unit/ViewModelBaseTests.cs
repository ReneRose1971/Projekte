using System;
using CustomWPFControls.Tests.Testing;
using Xunit;

namespace CustomWPFControls.Tests.Unit
{
    /// <summary>
    /// Unit-Tests für ViewModelBase&lt;T&gt;.
    /// </summary>
    public class ViewModelBaseTests
    {
        #region Constructor Tests

        [Fact]
        public void Constructor_WithValidModel_SetsModelProperty()
        {
            // Arrange
            var model = WPFCOntrolsTestHelpers.CreateTestModel();

            // Act
            var viewModel = new TestViewModel(model);

            // Assert
            Assert.Same(model, viewModel.Model);
        }

        [Fact]
        public void Constructor_WithNullModel_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new TestViewModel(null!));
        }

        #endregion

        #region Equals Tests

        [Fact]
        public void Equals_SameModelReference_ReturnsTrue()
        {
            // Arrange
            var model = WPFCOntrolsTestHelpers.CreateTestModel();
            var vm1 = new TestViewModel(model);
            var vm2 = new TestViewModel(model);

            // Act & Assert
            Assert.True(vm1.Equals(vm2));
            Assert.True(vm2.Equals(vm1)); // Symmetrie
        }

        [Fact]
        public void Equals_DifferentModelInstances_ReturnsFalse()
        {
            // Arrange
            var model1 = WPFCOntrolsTestHelpers.CreateTestModel(1, "Test");
            var model2 = WPFCOntrolsTestHelpers.CreateTestModel(1, "Test"); // Gleiche Werte, andere Instanz
            var vm1 = new TestViewModel(model1);
            var vm2 = new TestViewModel(model2);

            // Act & Assert
            Assert.False(vm1.Equals(vm2));
        }

        [Fact]
        public void Equals_SameInstance_ReturnsTrue()
        {
            // Arrange
            var model = WPFCOntrolsTestHelpers.CreateTestModel();
            var vm = new TestViewModel(model);

            // Act & Assert
            Assert.True(vm.Equals(vm));
        }

        [Fact]
        public void Equals_Null_ReturnsFalse()
        {
            // Arrange
            var model = WPFCOntrolsTestHelpers.CreateTestModel();
            var vm = new TestViewModel(model);

            // Act & Assert
            Assert.False(vm.Equals(null));
        }

        [Fact]
        public void Equals_DifferentType_ReturnsFalse()
        {
            // Arrange
            var model = WPFCOntrolsTestHelpers.CreateTestModel();
            var vm = new TestViewModel(model);

            // Act & Assert
            Assert.False(vm.Equals("not a viewmodel"));
        }

        #endregion

        #region GetHashCode Tests

        [Fact]
        public void GetHashCode_SameModelReference_ReturnsSameHash()
        {
            // Arrange
            var model = WPFCOntrolsTestHelpers.CreateTestModel();
            var vm1 = new TestViewModel(model);
            var vm2 = new TestViewModel(model);

            // Act & Assert
            Assert.Equal(vm1.GetHashCode(), vm2.GetHashCode());
        }

        [Fact]
        public void GetHashCode_DifferentModelInstances_ReturnsDifferentHash()
        {
            // Arrange
            var model1 = WPFCOntrolsTestHelpers.CreateTestModel(1, "Test");
            var model2 = WPFCOntrolsTestHelpers.CreateTestModel(1, "Test");
            var vm1 = new TestViewModel(model1);
            var vm2 = new TestViewModel(model2);

            // Act & Assert
            Assert.NotEqual(vm1.GetHashCode(), vm2.GetHashCode());
        }

        [Fact]
        public void GetHashCode_ConsistentAcrossMultipleCalls()
        {
            // Arrange
            var model = WPFCOntrolsTestHelpers.CreateTestModel();
            var vm = new TestViewModel(model);

            // Act
            var hash1 = vm.GetHashCode();
            var hash2 = vm.GetHashCode();

            // Assert
            Assert.Equal(hash1, hash2);
        }

        #endregion

        #region ToString Tests

        [Fact]
        public void ToString_DelegatesToModel()
        {
            // Arrange
            var model = WPFCOntrolsTestHelpers.CreateTestModel(42, "TestModel");
            var vm = new TestViewModel(model);

            // Act
            var result = vm.ToString();

            // Assert
            Assert.Equal(model.ToString(), result);
        }

        #endregion

        #region PropertyChanged Tests

        [Fact]
        public void PropertyChanged_UiProperty_RaisesEvent()
        {
            // Arrange
            var model = WPFCOntrolsTestHelpers.CreateTestModel();
            var vm = new TestViewModel(model);
            var eventRaised = false;
            vm.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(TestViewModel.IsSelected))
                    eventRaised = true;
            };

            // Act
            vm.IsSelected = true;

            // Assert
            Assert.True(eventRaised);
        }

        #endregion
    }
}
