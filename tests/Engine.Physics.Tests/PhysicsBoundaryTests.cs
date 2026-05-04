namespace Engine.Physics.Tests;

public sealed class PhysicsBoundaryTests
{
    [Fact]
    public void EnginePhysics_ProjectDoesNotReferenceForbiddenEngineModulesOrNativeStacks()
    {
        var projectFile = File.ReadAllText(FindRepositoryFile("src", "Engine.Physics", "Engine.Physics.csproj"));

        Assert.DoesNotContain("ProjectReference", projectFile);
        Assert.DoesNotContain("PackageReference", projectFile);
        Assert.DoesNotContain("Engine.SceneData", projectFile);
        Assert.DoesNotContain("Engine.Contracts", projectFile);
        Assert.DoesNotContain("Engine.Core", projectFile);
        Assert.DoesNotContain("Engine.Scene", projectFile);
        Assert.DoesNotContain("Engine.App", projectFile);
        Assert.DoesNotContain("Engine.Render", projectFile);
        Assert.DoesNotContain("Engine.Scripting", projectFile);
        Assert.DoesNotContain("Engine.Editor", projectFile);
        Assert.DoesNotContain("OpenTK", projectFile);
        Assert.DoesNotContain("ImGui", projectFile);
    }

    [Fact]
    public void EnginePhysics_SourceDoesNotMentionForbiddenEngineModulesOrRuntimeStacks()
    {
        var sourceRoot = FindRepositoryDirectory("src", "Engine.Physics");
        var forbiddenTerms = new[]
        {
            "Engine.SceneData",
            "Engine.Contracts",
            "Engine.Core",
            "Engine.Scene",
            "Engine.App",
            "Engine.Render",
            "Engine.Scripting",
            "Engine.Editor",
            "OpenTK",
            "ImGui",
            "OpenGL"
        };

        foreach (var sourceFile in Directory.EnumerateFiles(sourceRoot, "*.cs", SearchOption.AllDirectories))
        {
            var source = File.ReadAllText(sourceFile);
            foreach (var forbiddenTerm in forbiddenTerms)
            {
                Assert.DoesNotContain(forbiddenTerm, source);
            }
        }
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

    private static string FindRepositoryDirectory(params string[] relativeSegments)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(new[] { directory.FullName }.Concat(relativeSegments).ToArray());
            if (Directory.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException(Path.Combine(relativeSegments));
    }
}
