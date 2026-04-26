using Engine.Contracts;
using Engine.SceneData.Abstractions;
using System.Numerics;
using Xunit;

namespace Engine.SceneData.Tests;

public sealed class SceneDataContractsTests
{
    [Fact]
    public void SceneDescription_StoresNormalizedScenePayload()
    {
        var camera = new SceneCameraDescription(new Vector3(0.0f, 1.0f, 5.0f), Vector3.Zero, 1.2f);
        var transform = SceneTransformDescription.Identity;
        var objects = new[]
        {
            new SceneObjectDescription(
                "cube",
                "Cube",
                new SceneMeshRef("mesh://cube"),
                new SceneMaterialRef("material://default"),
                transform)
        };

        var scene = new SceneDescription("sample-scene", "Sample Scene", camera, objects);

        Assert.Equal("sample-scene", scene.SceneId);
        Assert.Equal("Sample Scene", scene.SceneName);
        Assert.Same(camera, scene.Camera);
        Assert.Equal(objects, scene.Objects);
    }

    [Fact]
    public void SceneFileDocument_ReservesFileModelShape()
    {
        var document = new SceneFileDocument(
            "1.0",
            new SceneFileDefinition(
                "sample-scene",
                "Sample Scene",
                new SceneFileCameraDefinition(new Vector3(0.0f, 1.0f, 5.0f), Vector3.Zero, 1.2f),
                new[]
                {
                    new SceneFileObjectDefinition(
                        "cube",
                        "Cube",
                        "mesh://cube",
                        "material://default",
                        new SceneFileTransformDefinition(Vector3.Zero, Quaternion.Identity, Vector3.One))
                }));

        Assert.Equal("1.0", document.Version);
        Assert.Equal("sample-scene", document.Scene.Id);
        Assert.Single(document.Scene.Objects);
    }

    [Fact]
    public void SceneDescriptionLoadResult_Success_PreservesScene()
    {
        var scene = new SceneDescription(
            "sample-scene",
            "Sample Scene",
            new SceneCameraDescription(new Vector3(0.0f, 1.0f, 5.0f), Vector3.Zero, 1.2f),
            Array.Empty<SceneObjectDescription>());

        var result = SceneDescriptionLoadResult.Success(scene);

        Assert.True(result.IsSuccess);
        Assert.Same(scene, result.Scene);
        Assert.Null(result.Failure);
    }

    [Fact]
    public void SceneDescriptionLoadFailure_RequiresConcreteFailureKind()
    {
        _ = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new SceneDescriptionLoadFailure(SceneDescriptionLoadFailureKind.None, "invalid", "scene.json"));
    }

    [Fact]
    public void SceneDescriptionAssembly_OnlyDependsOnContractsOutsideFramework()
    {
        var referencedAssemblyNames = typeof(SceneDescription).Assembly
            .GetReferencedAssemblies()
            .Select(assemblyName => assemblyName.Name)
            .Where(name => name is not null)
            .ToArray();

        Assert.Contains("Engine.Contracts", referencedAssemblyNames);
        Assert.DoesNotContain("Engine.Scene", referencedAssemblyNames);
        Assert.DoesNotContain("Engine.Asset", referencedAssemblyNames);
        Assert.DoesNotContain("Engine.Render", referencedAssemblyNames);
        Assert.DoesNotContain("Engine.App", referencedAssemblyNames);
    }

    [Fact]
    public void SceneDescriptionLoader_UsesStableLoaderAbstraction()
    {
        ISceneDescriptionLoader loader = new StubSceneDescriptionLoader();

        var result = loader.Load("sample.scene.json");

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Scene);
    }

    [Fact]
    public void JsonLoader_ValidScene_LoadsNormalizedDescription()
    {
        var loader = new JsonSceneDescriptionLoader();

        var result = loader.Load(GetSampleScenePath());

        Assert.True(result.IsSuccess);
        var scene = Assert.IsType<SceneDescription>(result.Scene);
        Assert.Equal("sample-scene", scene.SceneId);
        Assert.Equal("Sample Scene", scene.SceneName);
        Assert.Equal(2, scene.Objects.Count);
        Assert.Equal("mesh://cube", scene.Objects[0].Mesh.MeshId);
        Assert.Equal("material://highlight", scene.Objects[0].Material.MaterialId);
        Assert.Equal("material://default", scene.Objects[1].Material.MaterialId);
        Assert.Equal(SceneTransformDescription.Identity, scene.Objects[1].LocalTransform);
        Assert.Equal(new Vector3(0.0f, 0.25f, 2.2f), scene.Camera.Position);
        Assert.Equal(1.0471976f, scene.Camera.FieldOfViewRadians);
    }

    [Fact]
    public void JsonLoader_InvalidJson_ReturnsInvalidJsonFailure()
    {
        var scenePath = WriteSceneFile("{ invalid json");
        var loader = new JsonSceneDescriptionLoader();

        var result = loader.Load(scenePath);

        Assert.False(result.IsSuccess);
        Assert.Equal(SceneDescriptionLoadFailureKind.InvalidJson, result.Failure!.Kind);
        Assert.Equal(scenePath, result.Failure.Path);
    }

    [Fact]
    public void JsonLoader_MissingMesh_ReturnsMissingRequiredFieldFailure()
    {
        var scenePath = WriteSceneFile(
            """
            {
              "version": "1.0",
              "scene": {
                "id": "scene-a",
                "objects": [
                  {
                    "id": "cube-a"
                  }
                ]
              }
            }
            """);
        var loader = new JsonSceneDescriptionLoader();

        var result = loader.Load(scenePath);

        Assert.False(result.IsSuccess);
        Assert.Equal(SceneDescriptionLoadFailureKind.MissingRequiredField, result.Failure!.Kind);
    }

    [Fact]
    public void JsonLoader_DuplicateObjectId_ReturnsDuplicateObjectIdFailure()
    {
        var scenePath = WriteSceneFile(
            """
            {
              "version": "1.0",
              "scene": {
                "id": "scene-a",
                "objects": [
                  {
                    "id": "cube-a",
                    "mesh": "mesh://cube"
                  },
                  {
                    "id": "cube-a",
                    "mesh": "mesh://cube"
                  }
                ]
              }
            }
            """);
        var loader = new JsonSceneDescriptionLoader();

        var result = loader.Load(scenePath);

        Assert.False(result.IsSuccess);
        Assert.Equal(SceneDescriptionLoadFailureKind.DuplicateObjectId, result.Failure!.Kind);
    }

    [Fact]
    public void JsonLoader_MissingFile_ReturnsNotFoundFailure()
    {
        var loader = new JsonSceneDescriptionLoader();
        var missingPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.scene.json");

        var result = loader.Load(missingPath);

        Assert.False(result.IsSuccess);
        Assert.Equal(SceneDescriptionLoadFailureKind.NotFound, result.Failure!.Kind);
    }

    [Fact]
    public void JsonLoader_MissingCameraAndTransforms_FillsDefaults()
    {
        var scenePath = WriteSceneFile(
            """
            {
              "version": "1.0",
              "scene": {
                "id": "scene-a",
                "objects": [
                  {
                    "id": "cube-a",
                    "mesh": "mesh://cube"
                  }
                ]
              }
            }
            """);
        var loader = new JsonSceneDescriptionLoader();

        var result = loader.Load(scenePath);

        Assert.True(result.IsSuccess);
        var scene = Assert.IsType<SceneDescription>(result.Scene);
        var item = Assert.Single(scene.Objects);
        Assert.Equal("cube-a", item.ObjectName);
        Assert.Equal("material://default", item.Material.MaterialId);
        Assert.Equal(SceneTransformDescription.Identity, item.LocalTransform);
        Assert.Equal(new Vector3(0.0f, 0.0f, 2.2f), scene.Camera.Position);
        Assert.Equal(Vector3.Zero, scene.Camera.Target);
    }

    private static string GetSampleScenePath()
    {
        return Path.Combine(AppContext.BaseDirectory, "SampleScenes", "sample.scene.json");
    }

    private static string WriteSceneFile(string content)
    {
        var directoryPath = Path.Combine(Path.GetTempPath(), "AnsEngine.SceneData.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directoryPath);
        var scenePath = Path.Combine(directoryPath, "sample.scene.json");
        File.WriteAllText(scenePath, content);
        return scenePath;
    }

    private sealed class StubSceneDescriptionLoader : ISceneDescriptionLoader
    {
        public SceneDescriptionLoadResult Load(string sceneFilePath)
        {
            return SceneDescriptionLoadResult.Success(
                new SceneDescription(
                    "sample-scene",
                    sceneFilePath,
                    new SceneCameraDescription(new Vector3(0.0f, 1.0f, 5.0f), Vector3.Zero, 1.2f),
                    Array.Empty<SceneObjectDescription>()));
        }
    }
}
