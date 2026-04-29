using Engine.Editor;
using Xunit;

namespace Engine.Editor.Tests;

public sealed class EditorModuleBoundaryTests
{
    [Fact]
    public void SceneEditorSession_TypeExistsAsEditorCoreEntryPoint()
    {
        var session = new SceneEditorSession();

        Assert.NotNull(session);
    }

    [Fact]
    public void SceneEditorSessionResult_UsesExplicitFailureSemantics()
    {
        var success = SceneEditorSessionResult.Success();
        var failure = SceneEditorSessionResult.FailureResult(
            new SceneEditorFailure(
                SceneEditorFailureKind.OpenFailed,
                "Open failed.",
                "sample.scene.json",
                "cube"));

        Assert.True(success.IsSuccess);
        Assert.Null(success.Failure);
        Assert.False(failure.IsSuccess);
        Assert.Equal(SceneEditorFailureKind.OpenFailed, failure.Failure!.Kind);
        Assert.Equal("sample.scene.json", failure.Failure.Path);
        Assert.Equal("cube", failure.Failure.ObjectId);
    }

    [Fact]
    public void EngineEditorProject_DeclaresOnlyAllowedEngineProjectReferences()
    {
        var projectFile = File.ReadAllText(FindRepositoryFile("src", "Engine.Editor", "Engine.Editor.csproj"));

        Assert.Contains(@"..\Engine.Core\Engine.Core.csproj", projectFile);
        Assert.Contains(@"..\Engine.Contracts\Engine.Contracts.csproj", projectFile);
        Assert.Contains(@"..\Engine.SceneData\Engine.SceneData.csproj", projectFile);
        Assert.DoesNotContain("Engine.App.csproj", projectFile);
        Assert.DoesNotContain("Engine.Render.csproj", projectFile);
        Assert.DoesNotContain("Engine.Platform.csproj", projectFile);
        Assert.DoesNotContain("Engine.Asset.csproj", projectFile);
        Assert.DoesNotContain("OpenTK", projectFile);
    }

    [Fact]
    public void EngineEditorAssembly_DoesNotLoadForbiddenModules()
    {
        var referencedAssemblyNames = typeof(SceneEditorSession).Assembly
            .GetReferencedAssemblies()
            .Select(assemblyName => assemblyName.Name)
            .Where(name => name is not null)
            .ToArray();

        Assert.DoesNotContain("Engine.App", referencedAssemblyNames);
        Assert.DoesNotContain("Engine.Render", referencedAssemblyNames);
        Assert.DoesNotContain("Engine.Platform", referencedAssemblyNames);
        Assert.DoesNotContain("Engine.Asset", referencedAssemblyNames);
        Assert.DoesNotContain("OpenTK", referencedAssemblyNames);
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
