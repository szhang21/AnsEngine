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
    public void RuntimeAbstractions_SourceDoesNotExposeForbiddenApiNames()
    {
        var sourceText = string.Join(
            '\n',
            Directory.GetFiles(FindRepositoryDirectory("src", "Engine.Runtime.Abstractions"), "*.cs", SearchOption.AllDirectories)
                .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
                .Select(File.ReadAllText));

        Assert.DoesNotContain("Update", sourceText);
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
