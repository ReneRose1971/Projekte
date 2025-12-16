using System.ComponentModel;
using CustomWPFControls.Services;
using Xunit;
using FluentAssertions;

namespace CustomWPFControls.Tests.Unit;

/// <summary>
/// Unit-Tests für <see cref="WindowLayoutData"/>.
/// Ziel: Validierung der Fody-basierten PropertyChanged-Implementierung und IEntity-Konformität.
/// </summary>
public sealed class WindowLayoutDataTests
{
    [Fact]
    public void Constructor_ShouldInitializeWithDefaultValues()
    {
        // Arrange & Act
        var data = new WindowLayoutData();

        // Assert
        data.Id.Should().Be(0);
        data.WindowKey.Should().BeEmpty();
        data.Left.Should().Be(0);
        data.Top.Should().Be(0);
        data.Width.Should().Be(0);
        data.Height.Should().Be(0);
        data.WindowState.Should().Be(0);
    }

    [Fact]
    public void ImplementsIEntity_ShouldHaveIdProperty()
    {
        // Arrange
        var data = new WindowLayoutData();

        // Act
        data.Id = 42;

        // Assert
        data.Id.Should().Be(42);
        data.Should().BeAssignableTo<DataToolKit.Abstractions.Repositories.IEntity>();
    }

    [Fact]
    public void ImplementsINotifyPropertyChanged()
    {
        // Arrange
        var data = new WindowLayoutData();

        // Assert
        data.Should().BeAssignableTo<INotifyPropertyChanged>();
    }

    [Theory]
    [InlineData(nameof(WindowLayoutData.WindowKey), "TestWindow")]
    [InlineData(nameof(WindowLayoutData.Left), 100.0)]
    [InlineData(nameof(WindowLayoutData.Top), 200.0)]
    [InlineData(nameof(WindowLayoutData.Width), 800.0)]
    [InlineData(nameof(WindowLayoutData.Height), 600.0)]
    [InlineData(nameof(WindowLayoutData.WindowState), 2)]
    public void PropertyChanged_ShouldBeRaisedForAllProperties(string propertyName, object value)
    {
        // Arrange
        var data = new WindowLayoutData();
        var eventRaised = false;
        string? raisedPropertyName = null;

        data.PropertyChanged += (sender, e) =>
        {
            eventRaised = true;
            raisedPropertyName = e.PropertyName;
        };

        // Act
        typeof(WindowLayoutData).GetProperty(propertyName)!.SetValue(data, value);

        // Assert
        eventRaised.Should().BeTrue($"PropertyChanged should be raised for {propertyName}");
        raisedPropertyName.Should().Be(propertyName);
    }

    [Fact]
    public void PropertyChanged_Id_ShouldNotBeRaised()
    {
        // Arrange
        var data = new WindowLayoutData();
        var eventRaised = false;

        data.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == nameof(WindowLayoutData.Id))
                eventRaised = true;
        };

        // Act
        data.Id = 1;
        data.Id = 2;
        data.Id = 3;

        // Assert - [DoNotNotify] sollte PropertyChanged für Id unterdrücken
        eventRaised.Should().BeFalse("Id property should not raise PropertyChanged due to [DoNotNotify]");
    }

    [Fact]
    public void AllProperties_ShouldBeSettableAndGettable()
    {
        // Arrange
        var data = new WindowLayoutData();

        // Act
        data.Id = 123;
        data.WindowKey = "MainWindow";
        data.Left = 100.5;
        data.Top = 200.5;
        data.Width = 1024.0;
        data.Height = 768.0;
        data.WindowState = 2;

        // Assert
        data.Id.Should().Be(123);
        data.WindowKey.Should().Be("MainWindow");
        data.Left.Should().Be(100.5);
        data.Top.Should().Be(200.5);
        data.Width.Should().Be(1024.0);
        data.Height.Should().Be(768.0);
        data.WindowState.Should().Be(2);
    }

    [Fact]
    public void PropertyChanged_MultipleChanges_ShouldRaiseMultipleEvents()
    {
        // Arrange
        var data = new WindowLayoutData();
        var eventCount = 0;
        var raisedProperties = new List<string?>();

        data.PropertyChanged += (sender, e) =>
        {
            eventCount++;
            raisedProperties.Add(e.PropertyName);
        };

        // Act
        data.Left = 100;
        data.Top = 200;
        data.Width = 800;

        // Assert
        eventCount.Should().Be(3);
        raisedProperties.Should().Contain(nameof(WindowLayoutData.Left));
        raisedProperties.Should().Contain(nameof(WindowLayoutData.Top));
        raisedProperties.Should().Contain(nameof(WindowLayoutData.Width));
    }

    [Fact]
    public void WindowState_ShouldAcceptValidValues()
    {
        // Arrange
        var data = new WindowLayoutData();

        // Act & Assert - Normale Window-States
        data.WindowState = 0; // Normal
        data.WindowState.Should().Be(0);

        data.WindowState = 1; // Minimized
        data.WindowState.Should().Be(1);

        data.WindowState = 2; // Maximized
        data.WindowState.Should().Be(2);
    }
}
