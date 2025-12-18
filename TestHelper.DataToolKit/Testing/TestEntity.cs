using System;
using System.Collections.Generic;
using DataToolKit.Abstractions;
using DataToolKit.Abstractions.Repositories;

namespace TestHelper.DataToolKit.Testing;

/// <summary>
/// Testentität für Integration-Tests mit LiteDB.
/// Implementiert EntityBase (hat bereits INotifyPropertyChanged via Fody).
/// </summary>
public class TestEntity : EntityBase
{
    public string Name { get; set; } = string.Empty;
    public int Index { get; set; }

    public override string ToString() => $"TestEntity[Id={Id}, Name={Name}, Index={Index}]";
}

/// <summary>
/// EqualityComparer für TestEntity (für Integration-Tests).
/// </summary>
public class TestEntityComparer : IEqualityComparer<TestEntity>
{
    public bool Equals(TestEntity? x, TestEntity? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x is null || y is null) return false;
        return x.Id == y.Id && x.Name == y.Name && x.Index == y.Index;
    }

    public int GetHashCode(TestEntity obj)
    {
        return HashCode.Combine(obj.Id, obj.Name, obj.Index);
    }
}
