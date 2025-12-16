using System;

namespace CustomWPFControls.Tests.Testing
{
    /// <summary>
    /// Test-Model für Unit/Integration-Tests.
    /// </summary>
    public class TestModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";

        public override bool Equals(object? obj)
        {
            if (obj is not TestModel other) return false;
            return Id == other.Id && Name == other.Name;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Name);
        }

        public override string ToString()
        {
            return $"TestModel{{Id={Id}, Name={Name}}}";
        }
    }
}
