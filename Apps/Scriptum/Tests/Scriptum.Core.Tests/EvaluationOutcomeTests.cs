using FluentAssertions;
using Xunit;

namespace Scriptum.Core.Tests;

public class EvaluationOutcomeTests
{
    [Fact]
    public void Richtig_Is_Defined()
    {
        EvaluationOutcome.Richtig.Should().BeDefined();
    }
    
    [Fact]
    public void Falsch_Is_Defined()
    {
        EvaluationOutcome.Falsch.Should().BeDefined();
    }
    
    [Fact]
    public void Korrigiert_Is_Defined()
    {
        EvaluationOutcome.Korrigiert.Should().BeDefined();
    }
    
    [Fact]
    public void All_Values_Are_Different()
    {
        EvaluationOutcome.Richtig.Should().NotBe(EvaluationOutcome.Falsch);
    }
    
    [Fact]
    public void Falsch_Is_Different_From_Korrigiert()
    {
        EvaluationOutcome.Falsch.Should().NotBe(EvaluationOutcome.Korrigiert);
    }
    
    [Fact]
    public void Richtig_Is_Different_From_Korrigiert()
    {
        EvaluationOutcome.Richtig.Should().NotBe(EvaluationOutcome.Korrigiert);
    }
}
