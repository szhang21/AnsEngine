namespace Engine.Runtime.Tests;

using Engine.Contracts;
using Engine.Runtime;
using Engine.Runtime.Abstractions;
using Engine.SceneData;
using System.Numerics;
using Xunit;

public sealed class EngineRuntimeSessionTests
{
    [Fact]
    public void Initialize_LoadsSceneAndCreatesInitialSnapshot()
    {
        var session = new EngineRuntimeSession();

        var result = session.Initialize(CreateScriptScene("RotateSelf"));

        Assert.True(result.IsSuccess, result.Failure?.Message);
        var snapshot = session.CreateRuntimeSnapshot();
        var item = Assert.Single(snapshot.Objects);
        Assert.Equal("mover", item.ObjectId);
        Assert.True(item.HasTransform);
        Assert.True(item.HasMeshRenderer);
    }

    [Fact]
    public void Tick_RunsSceneStatisticsThenScriptUpdateBeforeSnapshot()
    {
        var session = new EngineRuntimeSession();
        Assert.True(session.Initialize(CreateScriptScene("RotateSelf")).IsSuccess);

        var result = session.Tick(new RuntimeTickContext(0.5d, 0.5d, RuntimeInputSnapshot.Empty));

        Assert.True(result.IsSuccess, result.Failure?.Message);
        var item = Assert.Single(result.Snapshot!.Objects);
        Assert.Equal(1, result.Snapshot.UpdateFrameCount);
        Assert.Equal(0.5d, result.Snapshot.AccumulatedUpdateSeconds);
        AssertQuaternionNearlyEqual(
            Quaternion.CreateFromAxisAngle(Vector3.UnitY, 0.5f),
            item.LocalTransform!.Value.Rotation);
    }

    [Fact]
    public void Tick_MoveOnInputThenPhysicsWriteback_ResolvesBeforeRenderSnapshot()
    {
        var session = new EngineRuntimeSession();
        Assert.True(session.Initialize(CreatePhysicsMovementScene()).IsSuccess);

        var result = session.Tick(
            new RuntimeTickContext(
                0.5d,
                0.5d,
                RuntimeInputSnapshot.FromKeys(RuntimeKey.D)));

        Assert.True(result.IsSuccess, result.Failure?.Message);
        var mover = result.Snapshot!.Objects.Single(item => item.ObjectId == "mover");
        AssertVectorNearlyEqual(Vector3.Zero, mover.LocalTransform!.Value.Position);
    }

    [Fact]
    public void Tick_ScriptFailureFailsBeforeSuccessfulSnapshot()
    {
        var registry = new Engine.Scripting.ScriptRegistry();
        Assert.Null(registry.Register("FailOnUpdate", static () => new FailingRuntimeScript()));
        var session = new EngineRuntimeSession(
            new Engine.Core.EngineRuntimeInfo("AnsEngine", "0.1.0"),
            new Engine.Scripting.ScriptRuntime(registry));
        Assert.True(session.Initialize(CreateScriptScene("FailOnUpdate")).IsSuccess);

        var result = session.Tick(new RuntimeTickContext(1.0d, 1.0d, RuntimeInputSnapshot.Empty));

        Assert.False(result.IsSuccess);
        Assert.Null(result.Snapshot);
        Assert.Equal("ScriptUpdate", result.Failure!.Stage);
        Assert.Equal("mover", result.Failure.ObjectId);
        Assert.Equal("FailOnUpdate", result.Failure.ComponentType);
        Assert.Contains("synthetic runtime update failure", result.Failure.Message);
    }

    [Fact]
    public void Initialize_TransformlessScriptObjectFailsBeforeTick()
    {
        var session = new EngineRuntimeSession();

        var result = session.Initialize(CreateTransformlessScriptScene());

        Assert.False(result.IsSuccess);
        Assert.Equal("ScriptBinding", result.Failure!.Stage);
        Assert.Equal("empty", result.Failure.ObjectId);
        Assert.Equal("RotateSelf", result.Failure.ComponentType);
    }

    [Fact]
    public void Initialize_UnknownScriptFailsWithScriptBindingDiagnostic()
    {
        var session = new EngineRuntimeSession();

        var result = session.Initialize(CreateScriptScene("MissingScript"));

        Assert.False(result.IsSuccess);
        Assert.Equal("ScriptBinding", result.Failure!.Stage);
        Assert.Equal("mover", result.Failure.ObjectId);
        Assert.Equal("MissingScript", result.Failure.ComponentType);
    }

    [Fact]
    public void EngineRuntime_ProjectReferencesOnlyAllowedRuntimeDependencies()
    {
        var projectFile = File.ReadAllText(FindRepositoryFile("src", "Engine.Runtime", "Engine.Runtime.csproj"));

        Assert.Contains("Engine.Scene", projectFile);
        Assert.Contains("Engine.Scripting", projectFile);
        Assert.Contains("Engine.Physics", projectFile);
        Assert.Contains("Engine.SceneData", projectFile);
        Assert.Contains("Engine.Runtime.Abstractions", projectFile);
        Assert.Contains("Engine.Contracts", projectFile);
        Assert.DoesNotContain("Engine.App", projectFile);
        Assert.DoesNotContain("Engine.Platform", projectFile);
        Assert.DoesNotContain("Engine.Render", projectFile);
        Assert.DoesNotContain("Engine.Editor", projectFile);
        Assert.DoesNotContain("Engine.Editor.App", projectFile);
        Assert.DoesNotContain("Engine.Asset", projectFile);
    }

    private static SceneDescription CreateScriptScene(string scriptId)
    {
        return new SceneDescription(
            "script-scene",
            "Script Scene",
            null!,
            new[]
            {
                new SceneObjectDescription(
                    "mover",
                    "Mover",
                    new SceneComponentDescription[]
                    {
                        new SceneTransformComponentDescription(SceneTransformDescription.Identity),
                        new SceneMeshRendererComponentDescription(
                            new SceneMeshRef("mesh://cube"),
                            new SceneMaterialRef("material://default")),
                        new SceneScriptComponentDescription(
                            scriptId,
                            new Dictionary<string, SceneScriptPropertyValue>
                            {
                                ["speedRadiansPerSecond"] = SceneScriptPropertyValue.FromNumber(1.0d)
                            })
                    })
            });
    }

    private static SceneDescription CreateTransformlessScriptScene()
    {
        return new SceneDescription(
            "script-scene",
            "Script Scene",
            null!,
            new[]
            {
                new SceneObjectDescription(
                    "empty",
                    "Empty",
                    new SceneComponentDescription[]
                    {
                        new SceneScriptComponentDescription(
                            "RotateSelf",
                            new Dictionary<string, SceneScriptPropertyValue>
                            {
                                ["speedRadiansPerSecond"] = SceneScriptPropertyValue.FromNumber(1.0d)
                            })
                    })
            });
    }

    private static SceneDescription CreatePhysicsMovementScene()
    {
        return new SceneDescription(
            "physics-movement-scene",
            "Physics Movement Scene",
            null!,
            new[]
            {
                new SceneObjectDescription(
                    "mover",
                    "Mover",
                    new SceneComponentDescription[]
                    {
                        new SceneTransformComponentDescription(SceneTransformDescription.Identity),
                        new SceneMeshRendererComponentDescription(
                            new SceneMeshRef("mesh://cube"),
                            new SceneMaterialRef("material://highlight")),
                        new SceneScriptComponentDescription(
                            "MoveOnInput",
                            new Dictionary<string, SceneScriptPropertyValue>
                            {
                                ["speedUnitsPerSecond"] = SceneScriptPropertyValue.FromNumber(2.0d)
                            }),
                        new SceneRigidBodyComponentDescription(SceneRigidBodyType.Dynamic, 1.0d),
                        new SceneBoxColliderComponentDescription(Vector3.One, Vector3.Zero)
                    }),
                new SceneObjectDescription(
                    "wall",
                    "Wall",
                    new SceneComponentDescription[]
                    {
                        new SceneTransformComponentDescription(
                            new SceneTransformDescription(
                                new Vector3(1.0f, 0.0f, 0.0f),
                                Quaternion.Identity,
                                Vector3.One)),
                        new SceneMeshRendererComponentDescription(
                            new SceneMeshRef("mesh://cube"),
                            new SceneMaterialRef("material://default")),
                        new SceneRigidBodyComponentDescription(SceneRigidBodyType.Static, 0.0d),
                        new SceneBoxColliderComponentDescription(Vector3.One, Vector3.Zero)
                    })
            });
    }

    private static void AssertQuaternionNearlyEqual(Quaternion expected, Quaternion actual)
    {
        Assert.InRange(MathF.Abs(expected.X - actual.X), 0.0f, 0.0001f);
        Assert.InRange(MathF.Abs(expected.Y - actual.Y), 0.0f, 0.0001f);
        Assert.InRange(MathF.Abs(expected.Z - actual.Z), 0.0f, 0.0001f);
        Assert.InRange(MathF.Abs(expected.W - actual.W), 0.0f, 0.0001f);
    }

    private static void AssertVectorNearlyEqual(Vector3 expected, Vector3 actual)
    {
        Assert.InRange(MathF.Abs(expected.X - actual.X), 0.0f, 0.0001f);
        Assert.InRange(MathF.Abs(expected.Y - actual.Y), 0.0f, 0.0001f);
        Assert.InRange(MathF.Abs(expected.Z - actual.Z), 0.0f, 0.0001f);
    }

    private sealed class FailingRuntimeScript : Engine.Scripting.IScriptBehavior, IRuntimeUpdateComponent
    {
        public void Initialize(Engine.Scripting.ScriptContext context)
        {
        }

        public void Update(Engine.Scripting.ScriptContext context)
        {
        }

        public RuntimeUpdateResult Update(RuntimeUpdateContext context)
        {
            return RuntimeUpdateResult.FailureResult(
                new RuntimeUpdateFailure(
                    "synthetic runtime update failure",
                    context.Owner.ObjectId,
                    "FailOnUpdate"));
        }
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
