using SolutionBundler.Core.Implementations;
using Xunit;

namespace SolutionBundler.Tests;

public class HashAndClassifierTests
{
    [Fact]
    public void Sha1HashCalculator_Produces_ExpectedLength()
    {
        var hasher = new Sha1HashCalculator();
        var hash = hasher.Sha1(System.Text.Encoding.UTF8.GetBytes("hello"));
        Assert.Equal(40, hash.Length);
    }

    [Theory]
    [InlineData("file.cs", "csharp")]
    [InlineData("view.xaml", "xaml")]
    [InlineData("project.csproj", "xml")]
    [InlineData("data.json", "json")]
    [InlineData("solution.sln", "")]
    public void Classifier_Returns_Expected(string path, string expected)
    {
        var classifier = new SimpleContentClassifier();
        Assert.Equal(expected, classifier.Classify(path));
    }
}