namespace Engine.Platform.Tests;

public sealed class PlatformBoundaryTests
{
    [Fact]
    public void EnginePlatform_ProjectDoesNotReferenceForbiddenEngineModules()
    {
        var projectFile = File.ReadAllText(FindRepositoryFile("src", "Engine.Platform", "Engine.Platform.csproj"));

        Assert.DoesNotContain("Engine.Scene", projectFile);
        Assert.DoesNotContain("Engine.Scripting", projectFile);
        Assert.DoesNotContain("Engine.App", projectFile);
        Assert.DoesNotContain("Engine.Render", projectFile);
    }

    private static string FindRepositoryFile(params string[] relativeSegments)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(new[] { directory.FullName }.Concat(relativeSegments).ToArray());
            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException("Could not locate repository file.", Path.Combine(relativeSegments));
    }
}
