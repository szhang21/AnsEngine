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
    public void SceneDocumentStore_UsesStableDocumentStoreAbstraction()
    {
        ISceneDocumentStore store = new JsonSceneDocumentStore();
        var scenePath = GetTemporaryScenePath();

        var saveResult = store.Save(scenePath, CreateSampleDocument());
        var loadResult = store.Load(scenePath);

        Assert.True(saveResult.IsSuccess);
        Assert.True(loadResult.IsSuccess);
        Assert.Equal("sample-scene", loadResult.Document!.Scene.Id);
        Assert.Single(loadResult.Document.Scene.Objects);
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

    [Fact]
    public void JsonSceneDocumentStore_LoadValidScene_ReturnsFileDocument()
    {
        var store = new JsonSceneDocumentStore();

        var result = store.Load(GetSampleScenePath());

        Assert.True(result.IsSuccess);
        Assert.Equal("1.0", result.Document!.Version);
        Assert.Equal("sample-scene", result.Document.Scene.Id);
        Assert.Equal(2, result.Document.Scene.Objects.Count);
    }

    [Fact]
    public void JsonSceneDocumentStore_SaveAndLoad_RoundTripsFileDocument()
    {
        var store = new JsonSceneDocumentStore();
        var scenePath = GetTemporaryScenePath();
        var document = CreateSampleDocument();

        var saveResult = store.Save(scenePath, document);
        var loadResult = store.Load(scenePath);

        Assert.True(saveResult.IsSuccess);
        Assert.True(loadResult.IsSuccess);
        Assert.Equal(document.Version, loadResult.Document!.Version);
        Assert.Equal(document.Scene.Id, loadResult.Document.Scene.Id);
        Assert.Equal(document.Scene.Objects[0].Transform, loadResult.Document.Scene.Objects[0].Transform);
    }

    [Fact]
    public void JsonSceneDocumentStore_MissingFile_ReturnsNotFoundFailure()
    {
        var store = new JsonSceneDocumentStore();
        var missingPath = GetTemporaryScenePath();

        var result = store.Load(missingPath);

        Assert.False(result.IsSuccess);
        Assert.Equal(SceneDocumentStoreFailureKind.NotFound, result.Failure!.Kind);
        Assert.Equal(missingPath, result.Failure.Path);
    }

    [Fact]
    public void JsonSceneDocumentStore_InvalidJson_ReturnsInvalidJsonFailure()
    {
        var store = new JsonSceneDocumentStore();
        var scenePath = WriteSceneFile("{ invalid json");

        var result = store.Load(scenePath);

        Assert.False(result.IsSuccess);
        Assert.Equal(SceneDocumentStoreFailureKind.InvalidJson, result.Failure!.Kind);
    }

    [Fact]
    public void JsonSceneDocumentStore_SaveToDirectoryPath_ReturnsWriteFailed()
    {
        var store = new JsonSceneDocumentStore();
        var directoryPath = CreateTemporaryDirectory();

        var result = store.Save(directoryPath, CreateSampleDocument());

        Assert.False(result.IsSuccess);
        Assert.Equal(SceneDocumentStoreFailureKind.WriteFailed, result.Failure!.Kind);
    }

    [Fact]
    public void JsonSceneDocumentStore_LoadSaveLoad_PreservesNormalizedSceneDescription()
    {
        var store = new JsonSceneDocumentStore();
        var loader = new JsonSceneDescriptionLoader();
        var originalPath = GetSampleScenePath();
        var savedPath = GetTemporaryScenePath();

        var originalScene = loader.Load(originalPath).Scene!;
        var document = store.Load(originalPath).Document!;
        var saveResult = store.Save(savedPath, document);
        var savedScene = loader.Load(savedPath).Scene!;

        Assert.True(saveResult.IsSuccess);
        AssertSceneEquivalent(originalScene, savedScene);
    }

    [Fact]
    public void SceneFileDocumentNormalizer_DefaultValues_MatchLoaderDefaults()
    {
        var document = new SceneFileDocument(
            "1.0",
            new SceneFileDefinition(
                "scene-a",
                "",
                Camera: null,
                new[]
                {
                    new SceneFileObjectDefinition(
                        "cube-a",
                        "",
                        "mesh://cube",
                        Material: null,
                        Transform: null)
                }));

        var result = SceneFileDocumentNormalizer.Normalize(document, "memory.scene.json");

        Assert.True(result.IsSuccess);
        Assert.Equal("scene-a", result.Scene!.SceneName);
        var item = Assert.Single(result.Scene.Objects);
        Assert.Equal("cube-a", item.ObjectName);
        Assert.Equal("material://default", item.Material.MaterialId);
        Assert.Equal(SceneTransformDescription.Identity, item.LocalTransform);
        Assert.Equal(new Vector3(0.0f, 0.0f, 2.2f), result.Scene.Camera.Position);
    }

    [Fact]
    public void SceneFileDocumentNormalizer_MissingMesh_ReturnsMissingRequiredField()
    {
        var document = CreateSampleDocument() with
        {
            Scene = CreateSampleDocument().Scene with
            {
                Objects = new[]
                {
                    CreateSampleDocument().Scene.Objects[0] with
                    {
                        Mesh = "   "
                    }
                }
            }
        };

        var result = SceneFileDocumentNormalizer.Normalize(document, "memory.scene.json");

        Assert.False(result.IsSuccess);
        Assert.Equal(SceneDescriptionLoadFailureKind.MissingRequiredField, result.Failure!.Kind);
    }

    [Fact]
    public void SceneFileDocumentNormalizer_NonFiniteTransform_ReturnsInvalidValue()
    {
        var document = CreateSampleDocument() with
        {
            Scene = CreateSampleDocument().Scene with
            {
                Objects = new[]
                {
                    CreateSampleDocument().Scene.Objects[0] with
                    {
                        Transform = new SceneFileTransformDefinition(
                            new Vector3(float.NaN, 0.0f, 0.0f),
                            Quaternion.Identity,
                            Vector3.One)
                    }
                }
            }
        };

        var result = SceneFileDocumentNormalizer.Normalize(document, "memory.scene.json");

        Assert.False(result.IsSuccess);
        Assert.Equal(SceneDescriptionLoadFailureKind.InvalidValue, result.Failure!.Kind);
    }

    [Fact]
    public void SceneFileDocumentEditor_AddRemoveAndUpdateObject_ProducesReloadableDocument()
    {
        var editor = new SceneFileDocumentEditor();
        var store = new JsonSceneDocumentStore();
        var loader = new JsonSceneDescriptionLoader();
        var document = CreateSampleDocument();
        var addedObject = new SceneFileObjectDefinition(
            "sphere",
            "Sphere",
            "mesh://sphere",
            "material://default",
            CreateSceneFileTransformDefinition());

        var addResult = editor.AddObject(document, addedObject);
        var renameResult = editor.UpdateObjectId(addResult.Document!, "sphere", "sphere-renamed");
        var nameResult = editor.UpdateObjectName(renameResult.Document!, "sphere-renamed", "Sphere Renamed");
        var resourceResult = editor.UpdateObjectResources(nameResult.Document!, "sphere-renamed", "mesh://cube", "material://highlight");
        var transformResult = editor.UpdateObjectTransform(
            resourceResult.Document!,
            "sphere-renamed",
            new SceneFileTransformDefinition(new Vector3(1.0f, 2.0f, 3.0f), Quaternion.Identity, Vector3.One));
        var removeResult = editor.RemoveObject(transformResult.Document!, "cube");
        var scenePath = GetTemporaryScenePath();
        var saveResult = store.Save(scenePath, removeResult.Document!);
        var loadResult = loader.Load(scenePath);

        Assert.True(addResult.IsSuccess);
        Assert.True(renameResult.IsSuccess);
        Assert.True(nameResult.IsSuccess);
        Assert.True(resourceResult.IsSuccess);
        Assert.True(transformResult.IsSuccess);
        Assert.True(removeResult.IsSuccess);
        Assert.True(saveResult.IsSuccess);
        Assert.True(loadResult.IsSuccess);
        var item = Assert.Single(loadResult.Scene!.Objects);
        Assert.Equal("sphere-renamed", item.ObjectId);
        Assert.Equal("Sphere Renamed", item.ObjectName);
        Assert.Equal("mesh://cube", item.Mesh.MeshId);
        Assert.Equal("material://highlight", item.Material.MaterialId);
        Assert.Equal(new Vector3(1.0f, 2.0f, 3.0f), item.LocalTransform.Position);
    }

    [Fact]
    public void SceneFileDocumentEditor_DuplicateObjectId_ReturnsFailure()
    {
        var editor = new SceneFileDocumentEditor();
        var duplicateObject = CreateSampleDocument().Scene.Objects[0];

        var result = editor.AddObject(CreateSampleDocument(), duplicateObject);

        Assert.False(result.IsSuccess);
        Assert.Equal(SceneDocumentEditFailureKind.DuplicateObjectId, result.Failure!.Kind);
    }

    [Fact]
    public void SceneFileDocumentEditor_MissingObject_ReturnsFailure()
    {
        var editor = new SceneFileDocumentEditor();

        var result = editor.RemoveObject(CreateSampleDocument(), "missing-object");

        Assert.False(result.IsSuccess);
        Assert.Equal(SceneDocumentEditFailureKind.ObjectNotFound, result.Failure!.Kind);
    }

    [Fact]
    public void SceneFileDocumentEditor_EmptyMesh_ReturnsMissingRequiredField()
    {
        var editor = new SceneFileDocumentEditor();

        var result = editor.UpdateObjectResources(CreateSampleDocument(), "cube", "", "material://default");

        Assert.False(result.IsSuccess);
        Assert.Equal(SceneDocumentEditFailureKind.MissingRequiredField, result.Failure!.Kind);
    }

    [Fact]
    public void SceneFileDocumentEditor_InvalidMaterial_ReturnsInvalidReference()
    {
        var editor = new SceneFileDocumentEditor();

        var result = editor.UpdateObjectResources(CreateSampleDocument(), "cube", "mesh://cube", "invalid-material");

        Assert.False(result.IsSuccess);
        Assert.Equal(SceneDocumentEditFailureKind.InvalidReference, result.Failure!.Kind);
    }

    [Fact]
    public void SceneFileDocumentEditor_NonFiniteTransform_ReturnsInvalidTransform()
    {
        var editor = new SceneFileDocumentEditor();
        var transform = new SceneFileTransformDefinition(
            Vector3.Zero,
            new Quaternion(0.0f, float.PositiveInfinity, 0.0f, 1.0f),
            Vector3.One);

        var result = editor.UpdateObjectTransform(CreateSampleDocument(), "cube", transform);

        Assert.False(result.IsSuccess);
        Assert.Equal(SceneDocumentEditFailureKind.InvalidTransform, result.Failure!.Kind);
    }

    private static string GetSampleScenePath()
    {
        return Path.Combine(AppContext.BaseDirectory, "SampleScenes", "sample.scene.json");
    }

    private static string WriteSceneFile(string content)
    {
        var directoryPath = CreateTemporaryDirectory();
        var scenePath = Path.Combine(directoryPath, "sample.scene.json");
        File.WriteAllText(scenePath, content);
        return scenePath;
    }

    private static string GetTemporaryScenePath()
    {
        return Path.Combine(CreateTemporaryDirectory(), "sample.scene.json");
    }

    private static string CreateTemporaryDirectory()
    {
        var directoryPath = Path.Combine(Path.GetTempPath(), "AnsEngine.SceneData.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directoryPath);
        return directoryPath;
    }

    private static SceneFileDocument CreateSampleDocument()
    {
        return new SceneFileDocument(
            "1.0",
            new SceneFileDefinition(
                "sample-scene",
                "Sample Scene",
                new SceneFileCameraDefinition(new Vector3(0.0f, 0.25f, 2.2f), Vector3.Zero, 1.0471976f),
                new[]
                {
                    new SceneFileObjectDefinition(
                        "cube",
                        "Cube",
                        "mesh://cube",
                        "material://highlight",
                        new SceneFileTransformDefinition(Vector3.Zero, Quaternion.Identity, Vector3.One))
                }));
    }

    private static SceneFileTransformDefinition CreateSceneFileTransformDefinition()
    {
        return new SceneFileTransformDefinition(Vector3.Zero, Quaternion.Identity, Vector3.One);
    }

    private static void AssertSceneEquivalent(SceneDescription expected, SceneDescription actual)
    {
        Assert.Equal(expected.SceneId, actual.SceneId);
        Assert.Equal(expected.SceneName, actual.SceneName);
        Assert.Equal(expected.Camera, actual.Camera);
        Assert.Equal(expected.Objects.Count, actual.Objects.Count);

        for (var index = 0; index < expected.Objects.Count; index += 1)
        {
            Assert.Equal(expected.Objects[index], actual.Objects[index]);
        }
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
