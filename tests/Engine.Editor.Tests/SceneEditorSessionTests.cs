using System.Numerics;
using Engine.Editor;
using Engine.SceneData;
using Engine.SceneData.Abstractions;
using Xunit;

namespace Engine.Editor.Tests;

public sealed class SceneEditorSessionTests
{
    [Fact]
    public void Open_ValidScene_PopulatesSessionState()
    {
        var scenePath = WriteSceneFile(CreateValidSceneJson("scene-a", "cube-a"));
        var session = new SceneEditorSession();

        var result = session.Open(scenePath);

        Assert.True(result.IsSuccess);
        Assert.True(session.HasDocument);
        Assert.False(session.IsDirty);
        Assert.Null(session.SelectedObjectId);
        Assert.Null(session.SelectedObject);
        Assert.Equal(scenePath, session.SceneFilePath);
        Assert.NotNull(session.Document);
        Assert.NotNull(session.Scene);
        Assert.Equal("scene-a", session.Scene!.SceneId);
        var item = Assert.Single(session.Objects);
        Assert.Equal("cube-a", item.ObjectId);
    }

    [Fact]
    public void Close_OpenSession_ClearsState()
    {
        var scenePath = WriteSceneFile(CreateValidSceneJson("scene-a", "cube-a"));
        var session = new SceneEditorSession();
        Assert.True(session.Open(scenePath).IsSuccess);

        var result = session.Close();

        Assert.True(result.IsSuccess);
        Assert.False(session.HasDocument);
        Assert.False(session.IsDirty);
        Assert.Null(session.SceneFilePath);
        Assert.Null(session.SelectedObjectId);
        Assert.Null(session.Document);
        Assert.Null(session.Scene);
        Assert.Empty(session.Objects);
        Assert.Null(session.SelectedObject);
    }

    [Fact]
    public void Open_MissingFile_ReturnsFailureAndLeavesEmptySession()
    {
        var session = new SceneEditorSession();
        var missingPath = Path.Combine(CreateTemporaryDirectory(), "missing.scene.json");

        var result = session.Open(missingPath);

        Assert.False(result.IsSuccess);
        Assert.Equal(SceneEditorFailureKind.OpenFailed, result.Failure!.Kind);
        Assert.Equal(missingPath, result.Failure.Path);
        Assert.False(session.HasDocument);
        Assert.Null(session.Document);
        Assert.Null(session.Scene);
    }

    [Fact]
    public void Open_InvalidJson_ReturnsFailureAndKeepsExistingSession()
    {
        var validPath = WriteSceneFile(CreateValidSceneJson("scene-a", "cube-a"));
        var invalidPath = WriteSceneFile("{ invalid json");
        var session = new SceneEditorSession();
        Assert.True(session.Open(validPath).IsSuccess);

        var result = session.Open(invalidPath);

        Assert.False(result.IsSuccess);
        Assert.Equal(SceneEditorFailureKind.OpenFailed, result.Failure!.Kind);
        Assert.True(session.HasDocument);
        Assert.Equal(validPath, session.SceneFilePath);
        Assert.Equal("scene-a", session.Scene!.SceneId);
        Assert.Equal("cube-a", Assert.Single(session.Objects).ObjectId);
    }

    [Fact]
    public void Open_InvalidScene_ReturnsFailureAndKeepsExistingSession()
    {
        var validPath = WriteSceneFile(CreateValidSceneJson("scene-a", "cube-a"));
        var invalidScenePath = WriteSceneFile(
            """
            {
              "version": "1.0",
              "scene": {
                "id": "invalid-scene",
                "objects": [
                  {
                    "id": "missing-mesh"
                  }
                ]
              }
            }
            """);
        var session = new SceneEditorSession();
        Assert.True(session.Open(validPath).IsSuccess);

        var result = session.Open(invalidScenePath);

        Assert.False(result.IsSuccess);
        Assert.Equal(SceneEditorFailureKind.InvalidDocument, result.Failure!.Kind);
        Assert.True(session.HasDocument);
        Assert.Equal(validPath, session.SceneFilePath);
        Assert.Equal("scene-a", session.Scene!.SceneId);
    }

    [Fact]
    public void ReloadValidate_WithoutDocument_ReturnsNoDocumentOpen()
    {
        var session = new SceneEditorSession();

        var result = session.ReloadValidate();

        Assert.False(result.IsSuccess);
        Assert.Equal(SceneEditorFailureKind.NoDocumentOpen, result.Failure!.Kind);
    }

    [Fact]
    public void ReloadValidate_OpenSession_RefreshesNormalizedScene()
    {
        var scenePath = WriteSceneFile(CreateValidSceneJson("scene-a", "cube-a"));
        var session = new SceneEditorSession();
        Assert.True(session.Open(scenePath).IsSuccess);

        var result = session.ReloadValidate();

        Assert.True(result.IsSuccess);
        Assert.True(session.HasDocument);
        Assert.Equal("scene-a", session.Scene!.SceneId);
        Assert.Equal("cube-a", Assert.Single(session.Objects).ObjectId);
    }

    [Fact]
    public void SelectObject_WithoutDocument_ReturnsNoDocumentOpen()
    {
        var session = new SceneEditorSession();

        var result = session.SelectObject("cube-a");

        Assert.False(result.IsSuccess);
        Assert.Equal(SceneEditorFailureKind.NoDocumentOpen, result.Failure!.Kind);
    }

    [Fact]
    public void AddObject_WithoutDocument_ReturnsNoDocumentOpen()
    {
        var session = new SceneEditorSession();

        var result = session.AddObject("cube-b", "Cube B");

        Assert.False(result.IsSuccess);
        Assert.Equal(SceneEditorFailureKind.NoDocumentOpen, result.Failure!.Kind);
    }

    [Fact]
    public void SelectObject_ExistingObject_UpdatesSelectionWithoutDirtying()
    {
        var scenePath = WriteSceneFile(CreateValidSceneJson("scene-a", "cube-a"));
        var session = new SceneEditorSession();
        Assert.True(session.Open(scenePath).IsSuccess);

        var result = session.SelectObject("cube-a");

        Assert.True(result.IsSuccess);
        Assert.False(session.IsDirty);
        Assert.Equal("cube-a", session.SelectedObjectId);
        Assert.Equal("cube-a", session.SelectedObject!.ObjectId);
    }

    [Fact]
    public void SelectObject_MissingObject_ReturnsFailureAndKeepsSelection()
    {
        var scenePath = WriteSceneFile(CreateValidSceneJson("scene-a", "cube-a"));
        var session = new SceneEditorSession();
        Assert.True(session.Open(scenePath).IsSuccess);
        Assert.True(session.SelectObject("cube-a").IsSuccess);

        var result = session.SelectObject("missing");

        Assert.False(result.IsSuccess);
        Assert.Equal(SceneEditorFailureKind.ObjectNotFound, result.Failure!.Kind);
        Assert.Equal("cube-a", session.SelectedObjectId);
        Assert.False(session.IsDirty);
    }

    [Fact]
    public void AddObject_ValidObject_UpdatesDocumentSceneAndDirty()
    {
        var scenePath = WriteSceneFile(CreateValidSceneJson("scene-a", "cube-a"));
        var session = new SceneEditorSession();
        Assert.True(session.Open(scenePath).IsSuccess);

        var result = session.AddObject(CreateSceneObject("cube-b"));

        Assert.True(result.IsSuccess);
        Assert.True(session.IsDirty);
        Assert.Equal(2, session.Objects.Count);
        Assert.Contains(session.Objects, item => item.ObjectId == "cube-b");
        Assert.Contains(session.Document!.Scene.Objects, item => item.Id == "cube-b");
    }

    [Fact]
    public void AddObject_DefaultFactory_CreatesTransformAndMeshRendererComponents()
    {
        var scenePath = WriteSceneFile(CreateValidSceneJson("scene-a", "cube-a"));
        var session = new SceneEditorSession();
        Assert.True(session.Open(scenePath).IsSuccess);

        var result = session.AddObject("cube-b", "Cube B");

        Assert.True(result.IsSuccess);
        Assert.True(session.IsDirty);
        var item = Assert.Single(session.Objects, item => item.ObjectId == "cube-b");
        Assert.NotNull(item.TransformComponent);
        Assert.NotNull(item.MeshRendererComponent);
        Assert.Equal("mesh://cube", item.MeshRendererComponent!.Mesh.MeshId);
    }

    [Fact]
    public void RemoveSelectedObject_SelectedObject_ClearsSelectionAndDirty()
    {
        var scenePath = WriteSceneFile(CreateValidSceneJson("scene-a", "cube-a"));
        var session = new SceneEditorSession();
        Assert.True(session.Open(scenePath).IsSuccess);
        Assert.True(session.SelectObject("cube-a").IsSuccess);

        var result = session.RemoveSelectedObject();

        Assert.True(result.IsSuccess);
        Assert.True(session.IsDirty);
        Assert.Null(session.SelectedObjectId);
        Assert.Empty(session.Objects);
    }

    [Fact]
    public void RemoveSelectedObject_WithoutSelection_ReturnsSelectionInvalid()
    {
        var scenePath = WriteSceneFile(CreateValidSceneJson("scene-a", "cube-a"));
        var session = new SceneEditorSession();
        Assert.True(session.Open(scenePath).IsSuccess);

        var result = session.RemoveSelectedObject();

        Assert.False(result.IsSuccess);
        Assert.Equal(SceneEditorFailureKind.SelectionInvalid, result.Failure!.Kind);
        Assert.False(session.IsDirty);
        Assert.Single(session.Objects);
    }

    [Fact]
    public void UpdateObjectId_SelectedObject_SelectionFollowsNewId()
    {
        var scenePath = WriteSceneFile(CreateValidSceneJson("scene-a", "cube-a"));
        var session = new SceneEditorSession();
        Assert.True(session.Open(scenePath).IsSuccess);
        Assert.True(session.SelectObject("cube-a").IsSuccess);

        var result = session.UpdateObjectId("cube-a", "cube-renamed");

        Assert.True(result.IsSuccess);
        Assert.True(session.IsDirty);
        Assert.Equal("cube-renamed", session.SelectedObjectId);
        Assert.Equal("cube-renamed", Assert.Single(session.Objects).ObjectId);
    }

    [Fact]
    public void UpdateObjectComponentProperties_ValidValues_UpdatesNormalizedScene()
    {
        var scenePath = WriteSceneFile(CreateValidSceneJson("scene-a", "cube-a"));
        var session = new SceneEditorSession();
        Assert.True(session.Open(scenePath).IsSuccess);

        Assert.True(session.UpdateObjectName("cube-a", "Renamed Cube").IsSuccess);
        var transform = new SceneFileTransformDefinition(
            new Vector3(1.0f, 2.0f, 3.0f),
            Quaternion.Identity,
            new Vector3(2.0f, 2.0f, 2.0f));
        Assert.True(
            session.UpdateObjectMeshRendererComponent(
                "cube-a",
                new SceneFileMeshRendererComponentDefinition("mesh://sphere", null)).IsSuccess);
        var result = session.UpdateObjectTransformComponent(
            "cube-a",
            new SceneFileTransformComponentDefinition(transform));

        Assert.True(result.IsSuccess);
        Assert.True(session.IsDirty);
        var item = Assert.Single(session.Objects);
        Assert.Equal("Renamed Cube", item.ObjectName);
        Assert.Equal("mesh://sphere", item.MeshRendererComponent!.Mesh.MeshId);
        Assert.Equal(new Vector3(1.0f, 2.0f, 3.0f), item.TransformComponent!.Transform.Position);
        Assert.Equal(new Vector3(2.0f, 2.0f, 2.0f), item.TransformComponent.Transform.Scale);
    }

    [Fact]
    public void RemoveObjectMeshRendererComponent_LeavesTransformOnlyObject()
    {
        var scenePath = WriteSceneFile(CreateValidSceneJson("scene-a", "cube-a"));
        var session = new SceneEditorSession();
        Assert.True(session.Open(scenePath).IsSuccess);

        var result = session.RemoveObjectMeshRendererComponent("cube-a");

        Assert.True(result.IsSuccess);
        Assert.True(session.IsDirty);
        var item = Assert.Single(session.Objects);
        Assert.NotNull(item.TransformComponent);
        Assert.Null(item.MeshRendererComponent);
        Assert.DoesNotContain(
            session.Document!.Scene.Objects.Single().Components,
            item => item.Type == SceneFileComponentTypes.MeshRenderer);
    }

    [Fact]
    public void UpdateObjectScriptComponent_AddsUpdatesRemovesAndPreservesProperties()
    {
        var scenePath = WriteSceneFile(CreateValidSceneJson("scene-a", "cube-a"));
        var session = new SceneEditorSession();
        Assert.True(session.Open(scenePath).IsSuccess);
        var script = new SceneFileScriptComponentDefinition(
            "MoveOnInput",
            new Dictionary<string, SceneFileScriptPropertyValue>
            {
                ["speed"] = SceneFileScriptPropertyValue.FromNumber(3.5d),
                ["enabled"] = SceneFileScriptPropertyValue.FromBoolean(true),
                ["label"] = SceneFileScriptPropertyValue.FromString("primary")
            });

        var addResult = session.UpdateObjectScriptComponent("cube-a", script);
        var updateResult = session.UpdateObjectScriptComponent(
            "cube-a",
            new SceneFileScriptComponentDefinition(
                "MoveOnInput",
                new Dictionary<string, SceneFileScriptPropertyValue>
                {
                    ["speed"] = SceneFileScriptPropertyValue.FromNumber(4.0d)
                }));
        var removeResult = session.RemoveObjectScriptComponent("cube-a", "MoveOnInput");

        Assert.True(addResult.IsSuccess);
        Assert.True(updateResult.IsSuccess);
        Assert.True(removeResult.IsSuccess);
        Assert.True(session.IsDirty);
        Assert.Empty(Assert.Single(session.Objects).ScriptComponents);
        Assert.DoesNotContain(
            session.Document!.Scene.Objects.Single().Components,
            item => item.Type == SceneFileComponentTypes.Script);
    }

    [Fact]
    public void Save_ScriptRigidBodyAndBoxCollider_RoundTripsThroughSession()
    {
        var scenePath = WriteSceneFile(CreateValidSceneJson("scene-a", "cube-a"));
        var session = new SceneEditorSession();
        Assert.True(session.Open(scenePath).IsSuccess);

        Assert.True(
            session.UpdateObjectScriptComponent(
                "cube-a",
                new SceneFileScriptComponentDefinition(
                    "MoveOnInput",
                    new Dictionary<string, SceneFileScriptPropertyValue>
                    {
                        ["speed"] = SceneFileScriptPropertyValue.FromNumber(2.0d),
                        ["enabled"] = SceneFileScriptPropertyValue.FromBoolean(true),
                        ["label"] = SceneFileScriptPropertyValue.FromString("mover")
                    })).IsSuccess);
        Assert.True(
            session.UpdateObjectRigidBodyComponent(
                "cube-a",
                new SceneFileRigidBodyComponentDefinition("Dynamic", 2.5d)).IsSuccess);
        Assert.True(
            session.UpdateObjectBoxColliderComponent(
                "cube-a",
                new SceneFileBoxColliderComponentDefinition(
                    new Vector3(2.0f, 3.0f, 4.0f),
                    new Vector3(0.5f, 1.0f, -0.5f))).IsSuccess);

        var result = session.Save();

        Assert.True(result.IsSuccess);
        Assert.False(session.IsDirty);
        var item = Assert.Single(session.Objects);
        var script = Assert.Single(item.ScriptComponents);
        Assert.Equal("MoveOnInput", script.ScriptId);
        Assert.Equal(2.0d, script.Properties["speed"].Number);
        Assert.True(script.Properties["enabled"].Boolean);
        Assert.Equal("mover", script.Properties["label"].Text);
        Assert.Equal(SceneRigidBodyType.Dynamic, item.RigidBodyComponent!.BodyType);
        Assert.Equal(2.5d, item.RigidBodyComponent.Mass);
        Assert.Equal(new Vector3(2.0f, 3.0f, 4.0f), item.BoxColliderComponent!.Size);
        Assert.Equal(new Vector3(0.5f, 1.0f, -0.5f), item.BoxColliderComponent.Center);
    }

    [Fact]
    public void UpdateObjectPhysicsComponents_StaticRigidBodyNormalizesMassAndComponentsCanExistIndependently()
    {
        var scenePath = WriteSceneFile(CreateValidSceneJson("scene-a", "cube-a"));
        var session = new SceneEditorSession();
        Assert.True(session.Open(scenePath).IsSuccess);

        Assert.True(
            session.UpdateObjectRigidBodyComponent(
                "cube-a",
                new SceneFileRigidBodyComponentDefinition("Static", 99.0d)).IsSuccess);
        Assert.Equal(SceneRigidBodyType.Static, Assert.Single(session.Objects).RigidBodyComponent!.BodyType);
        Assert.Equal(0.0d, Assert.Single(session.Objects).RigidBodyComponent!.Mass);
        Assert.Null(Assert.Single(session.Objects).BoxColliderComponent);

        Assert.True(session.RemoveObjectRigidBodyComponent("cube-a").IsSuccess);
        Assert.True(
            session.UpdateObjectBoxColliderComponent(
                "cube-a",
                new SceneFileBoxColliderComponentDefinition(Vector3.One, Vector3.Zero)).IsSuccess);
        Assert.Null(Assert.Single(session.Objects).RigidBodyComponent);
        Assert.NotNull(Assert.Single(session.Objects).BoxColliderComponent);
    }

    [Fact]
    public void UpdateObjectPhysicsComponents_InvalidValuesFailWithoutChangingSession()
    {
        var scenePath = WriteSceneFile(CreateValidSceneJson("scene-a", "cube-a"));
        var session = new SceneEditorSession();
        Assert.True(session.Open(scenePath).IsSuccess);
        Assert.True(session.SelectObject("cube-a").IsSuccess);
        var originalDocument = session.Document;
        var originalScene = session.Scene;

        var rigidBodyResult = session.UpdateObjectRigidBodyComponent(
            "cube-a",
            new SceneFileRigidBodyComponentDefinition("Dynamic", 0.0d));
        var boxColliderResult = session.UpdateObjectBoxColliderComponent(
            "cube-a",
            new SceneFileBoxColliderComponentDefinition(Vector3.Zero, Vector3.Zero));

        Assert.False(rigidBodyResult.IsSuccess);
        Assert.Equal(SceneEditorFailureKind.InvalidTransform, rigidBodyResult.Failure!.Kind);
        Assert.False(boxColliderResult.IsSuccess);
        Assert.Equal(SceneEditorFailureKind.InvalidTransform, boxColliderResult.Failure!.Kind);
        Assert.Same(originalDocument, session.Document);
        Assert.Same(originalScene, session.Scene);
        Assert.Equal("cube-a", session.SelectedObjectId);
        Assert.False(session.IsDirty);
    }

    [Fact]
    public void Open_TransformOnlyObject_CanSelectWithoutMeshRenderer()
    {
        var scenePath = WriteSceneFile(CreateTransformOnlySceneJson("scene-a", "empty-a"));
        var session = new SceneEditorSession();

        var result = session.Open(scenePath);

        Assert.True(result.IsSuccess);
        Assert.True(session.SelectObject("empty-a").IsSuccess);
        var item = Assert.Single(session.Objects);
        Assert.Equal("empty-a", item.ObjectId);
        Assert.NotNull(item.TransformComponent);
        Assert.Null(item.MeshRendererComponent);
        Assert.Equal("empty-a", session.SelectedObject!.ObjectId);
    }

    [Fact]
    public void Save_TransformOnlyObject_DoesNotAddMeshRenderer()
    {
        var scenePath = WriteSceneFile(CreateTransformOnlySceneJson("scene-a", "empty-a"));
        var session = new SceneEditorSession();
        Assert.True(session.Open(scenePath).IsSuccess);
        Assert.True(
            session.UpdateObjectTransformComponent(
                "empty-a",
                new SceneFileTransformComponentDefinition(
                    new SceneFileTransformDefinition(
                        new Vector3(4.0f, 5.0f, 6.0f),
                        null,
                        null))).IsSuccess);

        var result = session.Save();

        Assert.True(result.IsSuccess);
        Assert.False(session.IsDirty);
        var item = Assert.Single(session.Objects);
        Assert.Null(item.MeshRendererComponent);
        Assert.DoesNotContain(
            session.Document!.Scene.Objects.Single().Components,
            item => item.Type == SceneFileComponentTypes.MeshRenderer);
    }

    [Fact]
    public void EditFailure_DoesNotChangeDocumentSceneSelectionOrDirty()
    {
        var scenePath = WriteSceneFile(CreateValidSceneJson("scene-a", "cube-a"));
        var session = new SceneEditorSession();
        Assert.True(session.Open(scenePath).IsSuccess);
        Assert.True(session.SelectObject("cube-a").IsSuccess);
        var originalDocument = session.Document;
        var originalScene = session.Scene;
        var originalSelection = session.SelectedObjectId;
        var originalDirty = session.IsDirty;

        var result = session.AddObject(CreateSceneObject("cube-a"));

        Assert.False(result.IsSuccess);
        Assert.Equal(SceneEditorFailureKind.DuplicateObjectId, result.Failure!.Kind);
        Assert.Same(originalDocument, session.Document);
        Assert.Same(originalScene, session.Scene);
        Assert.Equal(originalSelection, session.SelectedObjectId);
        Assert.Equal(originalDirty, session.IsDirty);
    }

    [Fact]
    public void Save_WithoutDocument_ReturnsNoDocumentOpen()
    {
        var session = new SceneEditorSession();

        var result = session.Save();

        Assert.False(result.IsSuccess);
        Assert.Equal(SceneEditorFailureKind.NoDocumentOpen, result.Failure!.Kind);
    }

    [Fact]
    public void Save_OpenEditedSession_WritesFileReloadsAndClearsDirty()
    {
        var scenePath = WriteSceneFile(CreateValidSceneJson("scene-a", "cube-a"));
        var session = new SceneEditorSession();
        Assert.True(session.Open(scenePath).IsSuccess);
        Assert.True(session.AddObject(CreateSceneObject("cube-b")).IsSuccess);

        var result = session.Save();

        Assert.True(result.IsSuccess);
        Assert.False(session.IsDirty);
        Assert.Equal(scenePath, session.SceneFilePath);
        Assert.Equal(2, session.Objects.Count);
        var loadResult = new JsonSceneDescriptionLoader().Load(scenePath);
        Assert.True(loadResult.IsSuccess);
        Assert.Contains(loadResult.Scene!.Objects, item => item.ObjectId == "cube-b");
    }

    [Fact]
    public void SaveAs_OpenEditedSession_WritesNewPathSwitchesPathAndClearsDirty()
    {
        var scenePath = WriteSceneFile(CreateValidSceneJson("scene-a", "cube-a"));
        var saveAsPath = Path.Combine(CreateTemporaryDirectory(), "saved-as.scene.json");
        var session = new SceneEditorSession();
        Assert.True(session.Open(scenePath).IsSuccess);
        Assert.True(session.UpdateObjectName("cube-a", "Saved As Cube").IsSuccess);

        var result = session.SaveAs(saveAsPath);

        Assert.True(result.IsSuccess);
        Assert.False(session.IsDirty);
        Assert.Equal(saveAsPath, session.SceneFilePath);
        var loadResult = new JsonSceneDescriptionLoader().Load(saveAsPath);
        Assert.True(loadResult.IsSuccess);
        Assert.Equal("Saved As Cube", Assert.Single(loadResult.Scene!.Objects).ObjectName);
    }

    [Fact]
    public void SaveAs_WriteFailure_PreservesMemoryPathAndDirty()
    {
        var scenePath = WriteSceneFile(CreateValidSceneJson("scene-a", "cube-a"));
        var invalidSavePath = CreateTemporaryDirectory();
        var session = new SceneEditorSession();
        Assert.True(session.Open(scenePath).IsSuccess);
        Assert.True(session.AddObject(CreateSceneObject("cube-b")).IsSuccess);
        var originalDocument = session.Document;
        var originalScene = session.Scene;

        var result = session.SaveAs(invalidSavePath);

        Assert.False(result.IsSuccess);
        Assert.Equal(SceneEditorFailureKind.SaveFailed, result.Failure!.Kind);
        Assert.True(session.IsDirty);
        Assert.Equal(scenePath, session.SceneFilePath);
        Assert.Same(originalDocument, session.Document);
        Assert.Same(originalScene, session.Scene);
        Assert.Equal(2, session.Objects.Count);
    }

    [Fact]
    public void Save_ReloadFailureAfterWrite_PreservesMemoryPathAndDirty()
    {
        var scenePath = "memory.scene.json";
        var store = new ReloadFailingDocumentStore(CreateDocument("scene-a", "cube-a"));
        var session = new SceneEditorSession(store);
        Assert.True(session.Open(scenePath).IsSuccess);
        Assert.True(session.AddObject(CreateSceneObject("cube-b")).IsSuccess);
        var originalDocument = session.Document;
        var originalScene = session.Scene;

        store.FailNextLoad = true;
        var result = session.Save();

        Assert.False(result.IsSuccess);
        Assert.Equal(SceneEditorFailureKind.ReloadValidationFailed, result.Failure!.Kind);
        Assert.True(session.IsDirty);
        Assert.Equal(scenePath, session.SceneFilePath);
        Assert.Same(originalDocument, session.Document);
        Assert.Same(originalScene, session.Scene);
        Assert.Equal(2, session.Objects.Count);
    }

    private static string CreateValidSceneJson(string sceneId, string objectId)
    {
        return $$"""
            {
              "version": "2.0",
              "scene": {
                "id": "{{sceneId}}",
                "name": "Sample Scene",
                "objects": [
                  {
                    "id": "{{objectId}}",
                    "name": "Cube",
                    "components": [
                      {
                        "type": "Transform"
                      },
                      {
                        "type": "MeshRenderer",
                        "mesh": "mesh://cube",
                        "material": "material://default"
                      }
                    ]
                  }
                ]
              }
            }
            """;
    }

    private static string CreateTransformOnlySceneJson(string sceneId, string objectId)
    {
        return $$"""
            {
              "version": "2.0",
              "scene": {
                "id": "{{sceneId}}",
                "name": "Sample Scene",
                "objects": [
                  {
                    "id": "{{objectId}}",
                    "name": "Empty",
                    "components": [
                      {
                        "type": "Transform"
                      }
                    ]
                  }
                ]
              }
            }
            """;
    }

    private static SceneFileObjectDefinition CreateSceneObject(string objectId)
    {
        return new SceneFileObjectDefinition(
            objectId,
            "Cube",
            "mesh://cube",
            "material://default",
            null);
    }

    private static SceneFileDocument CreateDocument(string sceneId, string objectId)
    {
        return new SceneFileDocument(
            "2.0",
            new SceneFileDefinition(
                sceneId,
                "Sample Scene",
                null,
                new[]
                {
                    CreateSceneObject(objectId)
                }));
    }

    private static string WriteSceneFile(string content)
    {
        var scenePath = Path.Combine(CreateTemporaryDirectory(), "sample.scene.json");
        File.WriteAllText(scenePath, content);
        return scenePath;
    }

    private static string CreateTemporaryDirectory()
    {
        var directoryPath = Path.Combine(Path.GetTempPath(), "AnsEngine.Editor.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directoryPath);
        return directoryPath;
    }

    private sealed class ReloadFailingDocumentStore : ISceneDocumentStore
    {
        private SceneFileDocument mDocument;

        public ReloadFailingDocumentStore(SceneFileDocument document)
        {
            mDocument = document;
        }

        public bool FailNextLoad { get; set; }

        public SceneDocumentLoadResult Load(string sceneFilePath)
        {
            if (FailNextLoad)
            {
                FailNextLoad = false;
                return SceneDocumentLoadResult.FailureResult(
                    new SceneDocumentStoreFailure(
                        SceneDocumentStoreFailureKind.ReadFailed,
                        "Simulated reload failure.",
                        sceneFilePath));
            }

            return SceneDocumentLoadResult.Success(mDocument);
        }

        public SceneDocumentSaveResult Save(string sceneFilePath, SceneFileDocument document)
        {
            mDocument = document;
            return SceneDocumentSaveResult.Success();
        }
    }
}
