using System.Reflection;
using Engine.Contracts;
using Engine.Runtime.Abstractions;
using Xunit;

namespace Engine.Runtime.Abstractions.Tests;

public sealed class RuntimeAbstractionsApiShapeTests
{
    [Fact]
    public void RuntimeObject_PublicSurface_ExposesOnlyObjectIdentityAndTypedComponentLookup()
    {
        var properties = typeof(IRuntimeObject).GetProperties().Select(property => property.Name).Order().ToArray();
        var methods = typeof(IRuntimeObject).GetMethods().Select(method => method.Name).Order().ToArray();

        Assert.Equal(new[] { "ObjectId", "ObjectName" }, properties);
        Assert.Equal(
            new[] { "get_ObjectId", "get_ObjectName", "GetComponent", "HasComponent" },
            methods);
    }

    [Fact]
    public void RuntimeTransform_PublicSurface_UsesContractsSceneTransform()
    {
        var property = Assert.Single(typeof(IRuntimeTransformComponent).GetProperties());
        var method = Assert.Single(
            typeof(IRuntimeTransformComponent).GetMethods(),
            method => method.Name == "SetLocalTransform");

        Assert.Equal(nameof(IRuntimeTransformComponent.LocalTransform), property.Name);
        Assert.Equal(typeof(SceneTransform), property.PropertyType);
        Assert.Equal(typeof(void), method.ReturnType);
        Assert.Equal(typeof(SceneTransform), Assert.Single(method.GetParameters()).ParameterType);
    }

    [Fact]
    public void RuntimeUpdateComponent_PublicSurface_ExposesOnlyUpdateLifecycleEntry()
    {
        var method = Assert.Single(
            typeof(IRuntimeUpdateComponent).GetMethods(),
            item => item.Name == nameof(IRuntimeUpdateComponent.Update));

        Assert.True(typeof(IRuntimeComponent).IsAssignableFrom(typeof(IRuntimeUpdateComponent)));
        Assert.Equal(typeof(RuntimeUpdateResult), method.ReturnType);
        Assert.Equal(typeof(RuntimeUpdateContext), Assert.Single(method.GetParameters()).ParameterType);
    }

    [Fact]
    public void RuntimeUpdateContext_PublicSurface_UsesRuntimeObjectAndRuntimeInput()
    {
        var properties = typeof(RuntimeUpdateContext)
            .GetProperties()
            .Select(property => (property.Name, property.PropertyType))
            .OrderBy(item => item.Name)
            .ToArray();

        Assert.Equal(
            new[]
            {
                (nameof(RuntimeUpdateContext.DeltaSeconds), typeof(double)),
                (nameof(RuntimeUpdateContext.Input), typeof(RuntimeInputSnapshot)),
                (nameof(RuntimeUpdateContext.Owner), typeof(IRuntimeObject)),
                (nameof(RuntimeUpdateContext.TotalSeconds), typeof(double))
            },
            properties);
    }

    [Fact]
    public void RuntimeInputSnapshot_TracksOnlyRuntimeKeys()
    {
        var empty = RuntimeInputSnapshot.Empty;
        var input = RuntimeInputSnapshot.FromKeys(RuntimeKey.W, RuntimeKey.A, RuntimeKey.W);

        Assert.False(empty.AnyInputDetected);
        Assert.False(empty.IsKeyDown(RuntimeKey.W));
        Assert.True(input.AnyInputDetected);
        Assert.True(input.IsKeyDown(RuntimeKey.W));
        Assert.True(input.IsKeyDown(RuntimeKey.A));
        Assert.False(input.IsKeyDown(RuntimeKey.S));
        Assert.False(input.IsKeyDown(RuntimeKey.D));
        Assert.Equal(new[] { "W", "A", "S", "D" }, Enum.GetNames<RuntimeKey>());
    }

    [Fact]
    public void RuntimeUpdateResult_RepresentsSuccessAndFailure()
    {
        var success = RuntimeUpdateResult.Success();
        var failure = RuntimeUpdateResult.FailureResult(
            new RuntimeUpdateFailure("Missing Transform.", "cube", "RotateSelf"));

        Assert.True(success.IsSuccess);
        Assert.Null(success.Failure);
        Assert.False(failure.IsSuccess);
        Assert.Equal("Missing Transform.", failure.Failure?.Message);
        Assert.Equal("cube", failure.Failure?.ObjectId);
        Assert.Equal("RotateSelf", failure.Failure?.ComponentType);
    }

    [Fact]
    public void RuntimeAbstractions_AssemblyReferencesOnlyContractsAndFrameworkAssemblies()
    {
        var referencedAssemblies = typeof(IRuntimeObject).Assembly
            .GetReferencedAssemblies()
            .Select(assembly => assembly.Name)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .ToArray();

        Assert.Contains("Engine.Contracts", referencedAssemblies);
        Assert.DoesNotContain("Engine.Scene", referencedAssemblies);
        Assert.DoesNotContain("Engine.Scripting", referencedAssemblies);
        Assert.DoesNotContain("Engine.App", referencedAssemblies);
        Assert.DoesNotContain("Engine.Render", referencedAssemblies);
        Assert.DoesNotContain("Engine.Physics", referencedAssemblies);
        Assert.DoesNotContain("Engine.SceneData", referencedAssemblies);
        Assert.DoesNotContain("Engine.Editor", referencedAssemblies);
        Assert.DoesNotContain("Engine.Editor.App", referencedAssemblies);
    }

    [Fact]
    public void RuntimeAbstractions_SourceDoesNotExposeSchedulerTraversalOrMutationApiNames()
    {
        var sourceText = string.Join(
            '\n',
            Directory.GetFiles(FindRepositoryDirectory("src", "Engine.Runtime.Abstractions"), "*.cs", SearchOption.AllDirectories)
                .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
                .Select(File.ReadAllText));

        Assert.DoesNotContain("FixedUpdate", sourceText);
        Assert.DoesNotContain("Schedule", sourceText);
        Assert.DoesNotContain("Scheduler", sourceText);
        Assert.DoesNotContain("Traverse", sourceText);
        Assert.DoesNotContain("FindObject", sourceText);
        Assert.DoesNotContain("AddComponent", sourceText);
        Assert.DoesNotContain("RemoveComponent", sourceText);
        Assert.DoesNotContain("Parent", sourceText);
        Assert.DoesNotContain("Children", sourceText);
        Assert.DoesNotContain("IRuntimeScene", sourceText);
        Assert.DoesNotContain("SceneGraph", sourceText);
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
