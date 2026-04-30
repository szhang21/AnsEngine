using Engine.Editor;
using Engine.Editor.App;
using Xunit;

namespace Engine.Editor.App.Tests;

public sealed class EditorAppBoundaryTests
{
    [Fact]
    public void EngineEditorAppProject_DeclaresExpectedDependenciesOnly()
    {
        var projectFile = File.ReadAllText(FindRepositoryFile("src", "Engine.Editor.App", "Engine.Editor.App.csproj"));

        Assert.Contains(@"..\Engine.Editor\Engine.Editor.csproj", projectFile);
        Assert.Contains(@"..\Engine.SceneData\Engine.SceneData.csproj", projectFile);
        Assert.Contains(@"..\Engine.Contracts\Engine.Contracts.csproj", projectFile);
        Assert.Contains(@"..\Engine.Platform\Engine.Platform.csproj", projectFile);
        Assert.Contains("OpenTK", projectFile);
        Assert.Contains("ImGui.NET", projectFile);
        Assert.DoesNotContain("Engine.App.csproj", projectFile);
        Assert.DoesNotContain("Engine.Render.csproj", projectFile);
        Assert.DoesNotContain("Engine.Asset.csproj", projectFile);
    }

    [Fact]
    public void EngineEditor_RemainsHeadlessWithoutGuiDependencies()
    {
        var projectFile = File.ReadAllText(FindRepositoryFile("src", "Engine.Editor", "Engine.Editor.csproj"));
        var referencedAssemblyNames = typeof(SceneEditorSession).Assembly
            .GetReferencedAssemblies()
            .Select(assemblyName => assemblyName.Name)
            .Where(name => name is not null)
            .ToArray();

        Assert.DoesNotContain("OpenTK", projectFile);
        Assert.DoesNotContain("ImGui", projectFile);
        Assert.DoesNotContain("OpenTK", referencedAssemblyNames);
        Assert.DoesNotContain("ImGui.NET", referencedAssemblyNames);
        Assert.DoesNotContain("Engine.Editor.App", referencedAssemblyNames);
    }

    [Fact]
    public void EngineApp_DoesNotReferenceEditorApp()
    {
        var projectFile = File.ReadAllText(FindRepositoryFile("src", "Engine.App", "Engine.App.csproj"));

        Assert.DoesNotContain("Engine.Editor.App", projectFile);
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
