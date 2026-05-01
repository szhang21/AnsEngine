using System.Reflection;
using Engine.Scene;
using Xunit;

namespace Engine.Scene.Tests;

public sealed class SceneBoundaryTests
{
    [Fact]
    public void EngineScene_Project_DoesNotReferenceForbiddenModulesOrGuiLibraries()
    {
        var projectFile = File.ReadAllText(FindRepositoryFile("src", "Engine.Scene", "Engine.Scene.csproj"));

        Assert.DoesNotContain("Engine.Render", projectFile);
        Assert.DoesNotContain("Engine.Asset", projectFile);
        Assert.DoesNotContain("Engine.App", projectFile);
        Assert.DoesNotContain("Engine.Editor", projectFile);
        Assert.DoesNotContain("Engine.Editor.App", projectFile);
        Assert.DoesNotContain("OpenTK", projectFile);
        Assert.DoesNotContain("ImGui", projectFile);
        Assert.DoesNotContain("OpenGL", projectFile);
    }

    [Fact]
    public void EngineRenderAndSceneData_DoNotReferenceRuntimeSceneTypes()
    {
        var sceneAssemblyName = typeof(SceneGraphService).Assembly.GetName().Name;
        Assert.DoesNotContain(sceneAssemblyName, GetReferencedAssemblyNames("src", "Engine.Render", "Engine.Render.csproj"));
        Assert.DoesNotContain(sceneAssemblyName, GetReferencedAssemblyNames("src", "Engine.SceneData", "Engine.SceneData.csproj"));
    }

    [Fact]
    public void RuntimeSnapshot_PublicSurface_DoesNotExposeMutableRuntimeTypes()
    {
        var snapshotProperties = typeof(RuntimeSceneSnapshot).GetProperties();
        Assert.DoesNotContain(snapshotProperties, item => item.PropertyType == typeof(RuntimeScene));
        Assert.DoesNotContain(snapshotProperties, item => item.PropertyType == typeof(SceneRuntimeObject));
        Assert.DoesNotContain(snapshotProperties, item => item.PropertyType == typeof(SceneTransformComponent));
        Assert.DoesNotContain(snapshotProperties, item => item.PropertyType == typeof(SceneMeshRendererComponent));

        var objectSnapshotProperties = typeof(SceneRuntimeObjectSnapshot).GetProperties();
        Assert.DoesNotContain(objectSnapshotProperties, item => item.PropertyType == typeof(SceneRuntimeObject));
        Assert.DoesNotContain(objectSnapshotProperties, item => item.PropertyType == typeof(SceneTransformComponent));
        Assert.DoesNotContain(objectSnapshotProperties, item => item.PropertyType == typeof(SceneMeshRendererComponent));
    }

    private static IReadOnlyList<string> GetReferencedAssemblyNames(params string[] projectPathParts)
    {
        var projectFile = File.ReadAllText(FindRepositoryFile(projectPathParts));
        return projectFile
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Where(line => line.Contains("ProjectReference", StringComparison.Ordinal))
            .Select(Path.GetFileNameWithoutExtension)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Select(name => name!)
            .ToArray();
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
}
