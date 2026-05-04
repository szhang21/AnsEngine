using Engine.Contracts;
using Engine.SceneData.Abstractions;
using System.Numerics;
using System.Text.Json;
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
            "2.0",
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

        Assert.Equal("2.0", document.Version);
        Assert.Equal("sample-scene", document.Scene.Id);
        Assert.Single(document.Scene.Objects);
        Assert.Equal(2, document.Scene.Objects[0].Components.Count);
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
        Assert.DoesNotContain("Engine.Scripting", referencedAssemblyNames);
        Assert.DoesNotContain("Engine.Physics", referencedAssemblyNames);
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
              "version": "2.0",
              "scene": {
                "id": "scene-a",
                "objects": [
                  {
                    "id": "cube-a",
                    "components": [
                      {
                        "type": "Transform"
                      },
                      {
                        "type": "MeshRenderer"
                      }
                    ]
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
              "version": "2.0",
              "scene": {
                "id": "scene-a",
                "objects": [
                  {
                    "id": "cube-a",
                    "components": [
                      {
                        "type": "Transform"
                      },
                      {
                        "type": "MeshRenderer",
                        "mesh": "mesh://cube"
                      }
                    ]
                  },
                  {
                    "id": "cube-a",
                    "components": [
                      {
                        "type": "Transform"
                      },
                      {
                        "type": "MeshRenderer",
                        "mesh": "mesh://cube"
                      }
                    ]
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
    public void JsonLoader_VersionOneDocument_ReturnsInvalidValue()
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

        Assert.False(result.IsSuccess);
        Assert.Equal(SceneDescriptionLoadFailureKind.InvalidValue, result.Failure!.Kind);
    }

    [Fact]
    public void JsonSceneDocumentStore_LoadValidScene_ReturnsFileDocument()
    {
        var store = new JsonSceneDocumentStore();

        var result = store.Load(GetSampleScenePath());

        Assert.True(result.IsSuccess);
        Assert.Equal("2.0", result.Document!.Version);
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
    public void JsonSceneDocumentStore_Save_WritesComponentArrayWithoutLegacyObjectFields()
    {
        var store = new JsonSceneDocumentStore();
        var scenePath = GetTemporaryScenePath();

        var saveResult = store.Save(scenePath, CreateSampleDocument());
        var json = File.ReadAllText(scenePath);

        Assert.True(saveResult.IsSuccess);
        Assert.Contains("\"version\": \"2.0\"", json, StringComparison.Ordinal);
        Assert.Contains("\"components\"", json, StringComparison.Ordinal);
        Assert.Contains("\"type\": \"Transform\"", json, StringComparison.Ordinal);
        Assert.Contains("\"type\": \"MeshRenderer\"", json, StringComparison.Ordinal);
        using var jsonDocument = JsonDocument.Parse(json);
        var sceneObject = jsonDocument.RootElement.GetProperty("scene").GetProperty("objects")[0];
        Assert.False(sceneObject.TryGetProperty("mesh", out _));
        Assert.False(sceneObject.TryGetProperty("material", out _));
        Assert.False(sceneObject.TryGetProperty("transform", out _));
    }

    [Fact]
    public void JsonLoader_UnknownComponentType_ReturnsInvalidJsonFailure()
    {
        var scenePath = WriteSceneFile(
            """
            {
              "version": "2.0",
              "scene": {
                "id": "scene-a",
                "objects": [
                  {
                    "id": "cube-a",
                    "components": [
                      {
                        "type": "Transform"
                      },
                      {
                        "type": "Unknown"
                      }
                    ]
                  }
                ]
              }
            }
            """);
        var loader = new JsonSceneDescriptionLoader();

        var result = loader.Load(scenePath);

        Assert.False(result.IsSuccess);
        Assert.Equal(SceneDescriptionLoadFailureKind.InvalidJson, result.Failure!.Kind);
    }

    [Fact]
    public void JsonLoader_ScriptComponents_NormalizesAndPreservesOrder()
    {
        var scenePath = WriteSceneFile(
            """
            {
              "version": "2.0",
              "scene": {
                "id": "scene-a",
                "objects": [
                  {
                    "id": "cube-a",
                    "components": [
                      {
                        "type": "Transform"
                      },
                      {
                        "type": "Script",
                        "scriptId": "FirstScript",
                        "properties": {
                          "speed": 1.5,
                          "enabled": true,
                          "label": "primary"
                        }
                      },
                      {
                        "type": "Script",
                        "scriptId": "SecondScript"
                      }
                    ]
                  }
                ]
              }
            }
            """);
        var loader = new JsonSceneDescriptionLoader();

        var result = loader.Load(scenePath);

        Assert.True(result.IsSuccess, result.Failure?.Message);
        var item = Assert.Single(result.Scene!.Objects);
        Assert.Equal(
            new[]
            {
                SceneComponentDescriptionTypes.Transform,
                SceneComponentDescriptionTypes.Script,
                SceneComponentDescriptionTypes.Script
            },
            item.Components.Select(component => component.Type).ToArray());
        Assert.Equal(new[] { "FirstScript", "SecondScript" }, item.ScriptComponents.Select(component => component.ScriptId).ToArray());
        Assert.Equal(1.5d, item.ScriptComponents[0].Properties["speed"].Number);
        Assert.True(item.ScriptComponents[0].Properties["enabled"].Boolean);
        Assert.Equal("primary", item.ScriptComponents[0].Properties["label"].Text);
        Assert.Empty(item.ScriptComponents[1].Properties);
    }

    [Fact]
    public void JsonLoader_PhysicsComponents_NormalizesAndPreservesOrder()
    {
        var scenePath = WriteSceneFile(
            """
            {
              "version": "2.0",
              "scene": {
                "id": "scene-a",
                "objects": [
                  {
                    "id": "cube-a",
                    "components": [
                      {
                        "type": "Transform"
                      },
                      {
                        "type": "RigidBody",
                        "bodyType": "Dynamic",
                        "mass": 2.5
                      },
                      {
                        "type": "BoxCollider",
                        "size": { "x": 2, "y": 3, "z": 4 },
                        "center": { "x": 0.5, "y": 1, "z": -0.5 }
                      }
                    ]
                  }
                ]
              }
            }
            """);
        var loader = new JsonSceneDescriptionLoader();

        var result = loader.Load(scenePath);

        Assert.True(result.IsSuccess, result.Failure?.Message);
        var item = Assert.Single(result.Scene!.Objects);
        Assert.Equal(
            new[]
            {
                SceneComponentDescriptionTypes.Transform,
                SceneComponentDescriptionTypes.RigidBody,
                SceneComponentDescriptionTypes.BoxCollider
            },
            item.Components.Select(component => component.Type).ToArray());
        Assert.NotNull(item.RigidBodyComponent);
        Assert.NotNull(item.BoxColliderComponent);
        Assert.Equal(SceneRigidBodyType.Dynamic, item.RigidBodyComponent!.BodyType);
        Assert.Equal(2.5d, item.RigidBodyComponent.Mass);
        Assert.Equal(new Vector3(2.0f, 3.0f, 4.0f), item.BoxColliderComponent!.Size);
        Assert.Equal(new Vector3(0.5f, 1.0f, -0.5f), item.BoxColliderComponent.Center);
    }

    [Fact]
    public void JsonLoader_StaticRigidBodyAndBoxColliderDefaultValues_NormalizesMassAndCenter()
    {
        var scenePath = WriteSceneFile(
            """
            {
              "version": "2.0",
              "scene": {
                "id": "scene-a",
                "objects": [
                  {
                    "id": "floor",
                    "components": [
                      {
                        "type": "Transform"
                      },
                      {
                        "type": "RigidBody",
                        "bodyType": "Static",
                        "mass": 99
                      },
                      {
                        "type": "BoxCollider",
                        "size": { "x": 8, "y": 0.5, "z": 8 }
                      }
                    ]
                  }
                ]
              }
            }
            """);
        var loader = new JsonSceneDescriptionLoader();

        var result = loader.Load(scenePath);

        Assert.True(result.IsSuccess, result.Failure?.Message);
        var item = Assert.Single(result.Scene!.Objects);
        Assert.Equal(SceneRigidBodyType.Static, item.RigidBodyComponent!.BodyType);
        Assert.Equal(0.0d, item.RigidBodyComponent.Mass);
        Assert.Equal(Vector3.Zero, item.BoxColliderComponent!.Center);
    }

    [Fact]
    public void JsonLoader_InvalidRigidBodyBodyType_ReturnsInvalidValue()
    {
        var scenePath = WriteSceneFile(CreatePhysicsSceneJson(
            """
            {
              "type": "RigidBody",
              "bodyType": "Kinematic"
            },
            {
              "type": "BoxCollider",
              "size": { "x": 1, "y": 1, "z": 1 }
            }
            """));
        var loader = new JsonSceneDescriptionLoader();

        var result = loader.Load(scenePath);

        Assert.False(result.IsSuccess);
        Assert.Equal(SceneDescriptionLoadFailureKind.InvalidValue, result.Failure!.Kind);
    }

    [Fact]
    public void JsonLoader_InvalidRigidBodyMass_ReturnsInvalidValue()
    {
        var scenePath = WriteSceneFile(CreatePhysicsSceneJson(
            """
            {
              "type": "RigidBody",
              "bodyType": "Dynamic",
              "mass": 0
            },
            {
              "type": "BoxCollider",
              "size": { "x": 1, "y": 1, "z": 1 }
            }
            """));
        var loader = new JsonSceneDescriptionLoader();

        var result = loader.Load(scenePath);

        Assert.False(result.IsSuccess);
        Assert.Equal(SceneDescriptionLoadFailureKind.InvalidValue, result.Failure!.Kind);
    }

    [Fact]
    public void JsonLoader_InvalidBoxColliderSize_ReturnsInvalidValue()
    {
        var scenePath = WriteSceneFile(CreatePhysicsSceneJson(
            """
            {
              "type": "RigidBody",
              "bodyType": "Dynamic",
              "mass": 1
            },
            {
              "type": "BoxCollider",
              "size": { "x": 1, "y": 0, "z": 1 }
            }
            """));
        var loader = new JsonSceneDescriptionLoader();

        var result = loader.Load(scenePath);

        Assert.False(result.IsSuccess);
        Assert.Equal(SceneDescriptionLoadFailureKind.InvalidValue, result.Failure!.Kind);
    }

    [Fact]
    public void JsonLoader_MissingBoxColliderSize_ReturnsMissingRequiredField()
    {
        var scenePath = WriteSceneFile(CreatePhysicsSceneJson(
            """
            {
              "type": "RigidBody",
              "bodyType": "Dynamic",
              "mass": 1
            },
            {
              "type": "BoxCollider"
            }
            """));
        var loader = new JsonSceneDescriptionLoader();

        var result = loader.Load(scenePath);

        Assert.False(result.IsSuccess);
        Assert.Equal(SceneDescriptionLoadFailureKind.MissingRequiredField, result.Failure!.Kind);
    }

    [Fact]
    public void JsonSceneDocumentStore_SaveAndLoad_PreservesPhysicsComponents()
    {
        var store = new JsonSceneDocumentStore();
        var scenePath = GetTemporaryScenePath();
        var document = CreatePhysicsDocument();

        var saveResult = store.Save(scenePath, document);
        var loadResult = store.Load(scenePath);

        Assert.True(saveResult.IsSuccess);
        Assert.True(loadResult.IsSuccess);
        var components = loadResult.Document!.Scene.Objects[0].Components;
        var rigidBody = Assert.IsType<SceneFileRigidBodyComponentDefinition>(components[1]);
        var boxCollider = Assert.IsType<SceneFileBoxColliderComponentDefinition>(components[2]);
        Assert.Equal("Dynamic", rigidBody.BodyType);
        Assert.Equal(3.0d, rigidBody.Mass);
        Assert.Equal(new Vector3(2.0f, 3.0f, 4.0f), boxCollider.Size);
        Assert.Equal(new Vector3(0.25f, 0.5f, 0.75f), boxCollider.Center);
    }

    [Fact]
    public void JsonLoader_BlankScriptId_ReturnsMissingRequiredField()
    {
        var scenePath = WriteSceneFile(
            """
            {
              "version": "2.0",
              "scene": {
                "id": "scene-a",
                "objects": [
                  {
                    "id": "cube-a",
                    "components": [
                      {
                        "type": "Transform"
                      },
                      {
                        "type": "Script",
                        "scriptId": "   "
                      }
                    ]
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
    public void JsonLoader_UnsupportedScriptPropertyType_ReturnsInvalidJsonFailure()
    {
        var scenePath = WriteSceneFile(
            """
            {
              "version": "2.0",
              "scene": {
                "id": "scene-a",
                "objects": [
                  {
                    "id": "cube-a",
                    "components": [
                      {
                        "type": "Transform"
                      },
                      {
                        "type": "Script",
                        "scriptId": "RotateSelf",
                        "properties": {
                          "unsupported": {
                            "x": 1
                          }
                        }
                      }
                    ]
                  }
                ]
              }
            }
            """);
        var loader = new JsonSceneDescriptionLoader();

        var result = loader.Load(scenePath);

        Assert.False(result.IsSuccess);
        Assert.Equal(SceneDescriptionLoadFailureKind.InvalidJson, result.Failure!.Kind);
    }

    [Fact]
    public void JsonSceneDocumentStore_SaveAndLoad_PreservesScriptComponentsAndProperties()
    {
        var store = new JsonSceneDocumentStore();
        var scenePath = GetTemporaryScenePath();
        var document = CreateScriptDocument();

        var saveResult = store.Save(scenePath, document);
        var loadResult = store.Load(scenePath);

        Assert.True(saveResult.IsSuccess);
        Assert.True(loadResult.IsSuccess);
        var components = loadResult.Document!.Scene.Objects[0].Components;
        Assert.Equal(SceneFileComponentTypes.Script, components[1].Type);
        Assert.Equal(SceneFileComponentTypes.Script, components[2].Type);
        var firstScript = Assert.IsType<SceneFileScriptComponentDefinition>(components[1]);
        var secondScript = Assert.IsType<SceneFileScriptComponentDefinition>(components[2]);
        Assert.Equal("FirstScript", firstScript.ScriptId);
        Assert.Equal("SecondScript", secondScript.ScriptId);
        Assert.Equal(1.5d, firstScript.Properties["speed"].Number);
        Assert.True(firstScript.Properties["enabled"].Boolean);
        Assert.Equal("primary", firstScript.Properties["label"].Text);
    }

    [Fact]
    public void JsonLoader_DuplicateComponentType_ReturnsInvalidValueFailure()
    {
        var scenePath = WriteSceneFile(
            """
            {
              "version": "2.0",
              "scene": {
                "id": "scene-a",
                "objects": [
                  {
                    "id": "cube-a",
                    "components": [
                      {
                        "type": "Transform"
                      },
                      {
                        "type": "Transform"
                      },
                      {
                        "type": "MeshRenderer",
                        "mesh": "mesh://cube"
                      }
                    ]
                  }
                ]
              }
            }
            """);
        var loader = new JsonSceneDescriptionLoader();

        var result = loader.Load(scenePath);

        Assert.False(result.IsSuccess);
        Assert.Equal(SceneDescriptionLoadFailureKind.InvalidValue, result.Failure!.Kind);
    }

    [Fact]
    public void JsonLoader_TransformOnlyObject_NormalizesWithoutMeshRenderer()
    {
        var scenePath = WriteSceneFile(
            """
            {
              "version": "2.0",
              "scene": {
                "id": "scene-a",
                "objects": [
                  {
                    "id": "empty-a",
                    "components": [
                      {
                        "type": "Transform"
                      }
                    ]
                  }
                ]
              }
            }
            """);
        var loader = new JsonSceneDescriptionLoader();

        var result = loader.Load(scenePath);

        Assert.True(result.IsSuccess);
        var item = Assert.Single(result.Scene!.Objects);
        Assert.NotNull(item.TransformComponent);
        Assert.Null(item.MeshRendererComponent);
        Assert.Equal(SceneTransformDescription.Identity, item.LocalTransform);
    }

    [Fact]
    public void JsonLoader_MissingTransform_ReturnsMissingRequiredField()
    {
        var scenePath = WriteSceneFile(
            """
            {
              "version": "2.0",
              "scene": {
                "id": "scene-a",
                "objects": [
                  {
                    "id": "cube-a",
                    "components": [
                      {
                        "type": "MeshRenderer",
                        "mesh": "mesh://cube"
                      }
                    ]
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
            "2.0",
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

    private static string CreatePhysicsSceneJson(string physicsComponentsJson)
    {
        return $$"""
                {
                  "version": "2.0",
                  "scene": {
                    "id": "physics-scene",
                    "objects": [
                      {
                        "id": "physics-body",
                        "components": [
                          {
                            "type": "Transform"
                          },
                          {{physicsComponentsJson}}
                        ]
                      }
                    ]
                  }
                }
                """;
    }

    private static SceneFileDocument CreateSampleDocument()
    {
        return new SceneFileDocument(
            "2.0",
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

    private static SceneFileDocument CreatePhysicsDocument()
    {
        return new SceneFileDocument(
            "2.0",
            new SceneFileDefinition(
                "physics-scene",
                "Physics Scene",
                Camera: null,
                new[]
                {
                    new SceneFileObjectDefinition(
                        "physics-body",
                        "Physics Body",
                        new SceneFileComponentDefinition[]
                        {
                            new SceneFileTransformComponentDefinition(new SceneFileTransformDefinition(Vector3.Zero, Quaternion.Identity, Vector3.One)),
                            new SceneFileRigidBodyComponentDefinition("Dynamic", 3.0d),
                            new SceneFileBoxColliderComponentDefinition(
                                new Vector3(2.0f, 3.0f, 4.0f),
                                new Vector3(0.25f, 0.5f, 0.75f))
                        })
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
            Assert.Equal(expected.Objects[index].ObjectId, actual.Objects[index].ObjectId);
            Assert.Equal(expected.Objects[index].ObjectName, actual.Objects[index].ObjectName);
            Assert.Equal(expected.Objects[index].LocalTransform, actual.Objects[index].LocalTransform);
            Assert.Equal(expected.Objects[index].MeshRendererComponent, actual.Objects[index].MeshRendererComponent);
            Assert.Equal(
                expected.Objects[index].ScriptComponents.Select(component => component.ScriptId).ToArray(),
                actual.Objects[index].ScriptComponents.Select(component => component.ScriptId).ToArray());
        }
    }

    private static SceneFileDocument CreateScriptDocument()
    {
        return new SceneFileDocument(
            "2.0",
            new SceneFileDefinition(
                "script-scene",
                "Script Scene",
                Camera: null,
                new[]
                {
                    new SceneFileObjectDefinition(
                        "cube",
                        "Cube",
                        new SceneFileComponentDefinition[]
                        {
                            new SceneFileTransformComponentDefinition(new SceneFileTransformDefinition(Vector3.Zero, Quaternion.Identity, Vector3.One)),
                            new SceneFileScriptComponentDefinition(
                                "FirstScript",
                                new Dictionary<string, SceneFileScriptPropertyValue>
                                {
                                    ["speed"] = SceneFileScriptPropertyValue.FromNumber(1.5d),
                                    ["enabled"] = SceneFileScriptPropertyValue.FromBoolean(true),
                                    ["label"] = SceneFileScriptPropertyValue.FromString("primary")
                                }),
                            new SceneFileScriptComponentDefinition("SecondScript", null)
                        })
                }));
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
