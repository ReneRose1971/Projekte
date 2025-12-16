using Common.Bootstrap;
using Common.Bootstrap.Defaults;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using Xunit;

namespace Common.BootStrap.Tests;

/// <summary>
/// Tests für das <see cref="CommonBootstrapServiceModule"/>.
/// </summary>
public sealed class CommonBootstrapServiceModuleTests
{
    [Fact]
    public void ServiceModule_Scans_Assembly_For_Concrete_Comparers()
    {
        // Arrange
        var services = new ServiceCollection();
        var module = new CommonBootstrapServiceModule();

        // Act
        module.Register(services);
        var provider = services.BuildServiceProvider();

        // Assert - Modul registriert nur konkrete Comparer, keine automatischen Fallbacks
        // Wenn keine konkreten Comparer in der Assembly sind, sollte GetService null zurückgeben
        var comparer = provider.GetService<IEqualityComparer<string>>();
        Assert.Null(comparer); // Kein automatischer Fallback mehr
    }

    [Fact]
    public void ServiceModule_Does_Not_Override_Existing_Comparer()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IEqualityComparer<string>>(StringComparer.OrdinalIgnoreCase);
        
        var module = new CommonBootstrapServiceModule();

        // Act
        module.Register(services);
        var provider = services.BuildServiceProvider();

        // Assert - der bereits registrierte StringComparer bleibt erhalten
        var comparer = provider.GetService<IEqualityComparer<string>>();
        Assert.Same(StringComparer.OrdinalIgnoreCase, comparer);
    }

    [Fact]
    public void FallbackEqualsComparer_Can_Be_Manually_Registered()
    {
        // Arrange
        var services = new ServiceCollection();
        
        // Manuelle Registrierung von FallbackEqualsComparer für benötigte Typen
        services.AddSingleton<IEqualityComparer<string>>(new FallbackEqualsComparer<string>());
        services.AddSingleton<IEqualityComparer<int>>(new FallbackEqualsComparer<int>());
        services.AddSingleton<IEqualityComparer<TestObject>>(new FallbackEqualsComparer<TestObject>());

        new CommonBootstrapServiceModule().Register(services);
        var provider = services.BuildServiceProvider();

        // Act
        var stringComparer = provider.GetRequiredService<IEqualityComparer<string>>();
        var intComparer = provider.GetRequiredService<IEqualityComparer<int>>();
        var objComparer = provider.GetRequiredService<IEqualityComparer<TestObject>>();

        // Assert
        Assert.IsType<FallbackEqualsComparer<string>>(stringComparer);
        Assert.IsType<FallbackEqualsComparer<int>>(intComparer);
        Assert.IsType<FallbackEqualsComparer<TestObject>>(objComparer);

        // Funktionsprüfung
        Assert.True(stringComparer.Equals("test", "test"));
        Assert.False(stringComparer.Equals("test", "TEST"));
        Assert.True(intComparer.Equals(42, 42));
        Assert.False(intComparer.Equals(42, 43));
    }

    [Fact]
    public void ServiceModule_Is_Idempotent()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IEqualityComparer<string>>(new FallbackEqualsComparer<string>());
        
        var module = new CommonBootstrapServiceModule();

        // Act - zweimal registrieren
        module.Register(services);
        module.Register(services);
        var provider = services.BuildServiceProvider();

        // Assert - sollte nicht crashen
        var comparer = provider.GetRequiredService<IEqualityComparer<string>>();
        Assert.NotNull(comparer);
    }

    private sealed class TestObject
    {
        public int Value { get; set; }

        public override bool Equals(object? obj)
            => obj is TestObject other && Value == other.Value;

        public override int GetHashCode() => Value.GetHashCode();
    }
}
