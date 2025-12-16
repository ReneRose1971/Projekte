using SolutionBundler.Core.Implementations;
using Xunit;

namespace SolutionBundler.Tests;

public class RegexSecretMaskerTests
{
    [Fact]
    public void MasksJsonSecrets()
    {
        var masker = new RegexSecretMasker();
        var input = "{\"apiKey\": \"12345\", \"other\": \"x\"}\n";
        var outp = masker.Process("appsettings.json", input);
        Assert.DoesNotContain("12345", outp);
        Assert.Contains("***MASKED***", outp);
    }

    [Fact]
    public void LeavesNonJsonUnchanged()
    {
        var masker = new RegexSecretMasker();
        var input = "password=secretvalue";
        var outp = masker.Process("secrets.txt", input);
        Assert.Equal(input, outp);
    }
}