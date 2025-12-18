using System;
using System.Collections.Generic;
using DataToolKit.Abstractions.Repositories;

namespace TestHelper.DataToolKit.Testing;

/// <summary>
/// Einfaches DTO für Integration-Tests mit JSON-Repository.
/// Implementiert nur IEntity (ohne EntityBase).
/// </summary>
public class TestDto : IEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int Index { get; set; }

    public override string ToString() => $"TestDto[Id={Id}, Name={Name}, Index={Index}]";
}

/// <summary>
/// EqualityComparer für TestDto.
/// </summary>
public class TestDtoComparer : IEqualityComparer<TestDto>
{
    public bool Equals(TestDto? x, TestDto? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x is null || y is null) return false;
        return x.Id == y.Id && x.Name == y.Name && x.Index == y.Index;
    }

    public int GetHashCode(TestDto obj)
    {
        return HashCode.Combine(obj.Id, obj.Name, obj.Index);
    }
}
