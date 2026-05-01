using Xunit;

namespace Engine.Render.Tests;

public sealed class RenderBoundaryTests
{
    [Fact]
    public void EngineRender_Project_DoesNotReferenceSceneRuntimeTypes()
    {
        var projectFile = File.ReadAllText(FindRepositoryFile("src", "Engine.Render", "Engine.Render.csproj"));

        Assert.DoesNotContain("Engine.Scene", projectFile);
    }

    [Fact]
    public void EngineRender_Source_DoesNotReferenceSceneUpdateOrRuntimeSceneTypes()
    {
        var sourceText = string.Join(
            '\n',
            Directory.GetFiles(FindRepositoryDirectory("src", "Engine.Render"), "*.cs", SearchOption.AllDirectories)
                .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
                .Select(File.ReadAllText));

        Assert.DoesNotContain("SceneUpdateContext", sourceText);
        Assert.DoesNotContain("RuntimeScene", sourceText);
        Assert.DoesNotContain("SceneRuntimeObject", sourceText);
        Assert.DoesNotContain("SceneTransformComponent", sourceText);
        Assert.DoesNotContain("SceneMeshRendererComponent", sourceText);
    }

    private static string FindRepositoryFile(params string[] pathParts)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(new[] { directory.FullName }.Concat(pathParts).ToArray());
            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException($"Could not find repository file '{Path.Combine(pathParts)}'.");
    }

    private static string FindRepositoryDirectory(params string[] pathParts)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(new[] { directory.FullName }.Concat(pathParts).ToArray());
            if (Directory.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException($"Could not find repository directory '{Path.Combine(pathParts)}'.");
    }
}
