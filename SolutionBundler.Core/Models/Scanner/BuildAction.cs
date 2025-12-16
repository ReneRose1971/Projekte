namespace SolutionBundler.Core.Models;

public enum BuildAction
{
    Unknown = 0,
    Compile,
    Page,
    Resource,
    Content,
    None
}