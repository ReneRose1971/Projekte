using Xunit;

namespace Scriptum.Persistence.Tests;

/// <summary>
/// Collection-Definition für Tests, die auf die gleiche LiteDB-Datei zugreifen.
/// Stellt sicher, dass diese Tests sequenziell ausgeführt werden, um Dateikonflikte zu vermeiden.
/// </summary>
[CollectionDefinition("LiteDB Tests", DisableParallelization = true)]
public class LiteDbTestCollection
{
}
