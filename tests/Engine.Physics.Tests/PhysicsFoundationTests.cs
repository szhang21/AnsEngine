using Engine.Physics;
using Engine.SceneData;
using System.Numerics;

namespace Engine.Physics.Tests;

public sealed class PhysicsFoundationTests
{
    [Fact]
    public void PublicShape_ExposesWorldStepSnapshotAndQueryTypes()
    {
        var transform = PhysicsTransform.Identity;
        var collider = new PhysicsBoxColliderDefinition(Vector3.One, Vector3.Zero);
        var body = new PhysicsBodyDefinition("body-001", "Body 001", PhysicsBodyType.Dynamic, 1.0d, transform, collider);
        var definition = new PhysicsWorldDefinition(new[] { body });
        var step = new PhysicsStepContext(0.016d, 0.016d);
        var queryBounds = new PhysicsAabb(new Vector3(-1.0f), new Vector3(1.0f));
        var world = new PhysicsWorld();

        var snapshot = world.CreateSnapshot();
        var query = world.QueryAabb(queryBounds);

        Assert.Single(definition.Bodies);
        Assert.Equal(0.016d, step.FixedDeltaSeconds);
        Assert.Equal(Vector3.Zero, transform.Position);
        Assert.Equal(Vector3.One, collider.Size);
        Assert.Equal("body-001", body.BodyId);
        Assert.Equal(0, snapshot.BodyCount);
        Assert.False(query.HasAny);
    }

    [Fact]
    public void Load_ValidDefinitionCreatesStableBodySnapshots()
    {
        var world = PhysicsWorld.Load(
            new PhysicsWorldDefinition(
                new[]
                {
                    CreateBody("body-a", "Body A", PhysicsBodyType.Dynamic, new Vector3(1.0f, 2.0f, 3.0f), Vector3.One, new Vector3(2.0f, 2.0f, 2.0f), Vector3.Zero, 1.5d),
                    CreateBody("body-b", "Body B", PhysicsBodyType.Static, Vector3.Zero, Vector3.One, Vector3.One, Vector3.Zero, 0.0d)
                }));

        var snapshot = world.CreateSnapshot();

        Assert.Equal(2, world.BodyCount);
        Assert.Equal(2, snapshot.BodyCount);
        Assert.Equal(new[] { "body-a", "body-b" }, snapshot.Bodies.Select(body => body.BodyId).ToArray());
        Assert.Equal(new[] { PhysicsBodyType.Dynamic, PhysicsBodyType.Static }, snapshot.Bodies.Select(body => body.BodyType).ToArray());
        Assert.Equal(1.5d, snapshot.Bodies[0].Mass);
    }

    [Fact]
    public void Step_UpdatesFixedStepStatisticsWithoutMovingBodies()
    {
        var body = CreateBody("body-a", "Body A", PhysicsBodyType.Dynamic, new Vector3(0.0f, 2.0f, 0.0f), Vector3.One, Vector3.One, Vector3.Zero, 1.0d);
        var definition = new PhysicsWorldDefinition(new[] { body });
        var world = PhysicsWorld.Load(definition);
        var before = Assert.Single(world.CreateSnapshot().Bodies);

        world.Step(new PhysicsStepContext(0.02d, 0.02d));
        world.Step(new PhysicsStepContext(0.02d, 0.04d));

        var after = Assert.Single(world.CreateSnapshot().Bodies);
        Assert.Equal(2, world.StepCount);
        Assert.Equal(0.04d, world.AccumulatedFixedSeconds, 6);
        Assert.Equal(before.Transform, after.Transform);
        Assert.Equal(body.Transform, definition.Bodies[0].Transform);
    }

    [Fact]
    public void Load_ComputesAabbFromPositionCenterSizeAndAbsoluteScaleIgnoringRotation()
    {
        var world = PhysicsWorld.Load(
            new PhysicsWorldDefinition(
                new[]
                {
                    new PhysicsBodyDefinition(
                        "body-a",
                        "Body A",
                        PhysicsBodyType.Dynamic,
                        1.0d,
                        new PhysicsTransform(
                            new Vector3(1.0f, 2.0f, 3.0f),
                            Quaternion.CreateFromAxisAngle(Vector3.UnitY, 1.2f),
                            new Vector3(-2.0f, 3.0f, 1.0f)),
                        new PhysicsBoxColliderDefinition(
                            new Vector3(2.0f, 4.0f, 6.0f),
                            new Vector3(0.5f, -0.5f, 1.0f)))
                }));

        var body = Assert.Single(world.CreateSnapshot().Bodies);

        AssertVectorNearlyEqual(new Vector3(-0.5f, -4.5f, 1.0f), body.Aabb.Min);
        AssertVectorNearlyEqual(new Vector3(3.5f, 7.5f, 7.0f), body.Aabb.Max);
    }

    [Fact]
    public void QueryAabb_ReturnsOnlyOverlappingBodies()
    {
        var world = PhysicsWorld.Load(
            new PhysicsWorldDefinition(
                new[]
                {
                    CreateBody("near", "Near", PhysicsBodyType.Static, Vector3.Zero, Vector3.One, Vector3.One, Vector3.Zero, 0.0d),
                    CreateBody("far", "Far", PhysicsBodyType.Static, new Vector3(10.0f, 0.0f, 0.0f), Vector3.One, Vector3.One, Vector3.Zero, 0.0d)
                }));

        var result = world.QueryAabb(new PhysicsAabb(new Vector3(-1.0f), new Vector3(1.0f)));

        Assert.True(result.HasAny);
        var body = Assert.Single(result.Bodies);
        Assert.Equal("near", body.BodyId);
    }

    [Fact]
    public void QueryGround_ClassifiesAboveIntersectingAndBelowWithoutMutatingWorld()
    {
        var world = PhysicsWorld.Load(
            new PhysicsWorldDefinition(
                new[]
                {
                    CreateBody("above", "Above", PhysicsBodyType.Static, new Vector3(0.0f, 2.0f, 0.0f), Vector3.One, Vector3.One, Vector3.Zero, 0.0d),
                    CreateBody("intersecting", "Intersecting", PhysicsBodyType.Static, Vector3.Zero, Vector3.One, new Vector3(1.0f, 2.0f, 1.0f), Vector3.Zero, 0.0d),
                    CreateBody("below", "Below", PhysicsBodyType.Static, new Vector3(0.0f, -2.0f, 0.0f), Vector3.One, Vector3.One, Vector3.Zero, 0.0d)
                }));
        var before = world.CreateSnapshot().Bodies.Select(body => body.Aabb).ToArray();

        var above = world.QueryGround("above");
        var intersecting = world.QueryGround("intersecting");
        var below = world.QueryGround("below");

        Assert.True(above.IsAboveGround);
        Assert.True(intersecting.IsIntersectingGround);
        Assert.True(below.IsBelowGround);
        Assert.Equal(before, world.CreateSnapshot().Bodies.Select(body => body.Aabb).ToArray());
    }

    [Fact]
    public void Load_MalformedDefinitionFailsFastWithStableDiagnostics()
    {
        var missingTransform = new PhysicsWorldDefinition(
            new[]
            {
                new PhysicsBodyDefinition(
                    "body-a",
                    "Body A",
                    PhysicsBodyType.Dynamic,
                    1.0d,
                    Transform: null,
                    new PhysicsBoxColliderDefinition(Vector3.One, Vector3.Zero))
            });
        var invalidSize = new PhysicsWorldDefinition(
            new[]
            {
                CreateBody("body-b", "Body B", PhysicsBodyType.Dynamic, Vector3.Zero, Vector3.One, new Vector3(0.0f, 1.0f, 1.0f), Vector3.Zero, 1.0d)
            });

        var transformFailure = Assert.Throws<ArgumentException>(() => PhysicsWorld.Load(missingTransform));
        var sizeFailure = Assert.Throws<ArgumentException>(() => PhysicsWorld.Load(invalidSize));
        var stepFailure = Assert.Throws<ArgumentException>(() => new PhysicsWorld().Step(new PhysicsStepContext(0.0d, 0.0d)));

        Assert.Contains("requires a transform definition", transformFailure.Message, StringComparison.Ordinal);
        Assert.Contains("size must contain positive finite values", sizeFailure.Message, StringComparison.Ordinal);
        Assert.Contains("fixed delta seconds must be positive and finite", stepFailure.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void TestOnlySceneDataAdapter_MapsRealisticFixtureToPhysicsWorldDefinition()
    {
        var scenePath = WriteSceneFile(
            """
            {
              "version": "2.0",
              "scene": {
                "id": "physics-scene",
                "objects": [
                  {
                    "id": "crate",
                    "name": "Crate",
                    "components": [
                      {
                        "type": "Transform",
                        "position": { "x": 0, "y": 1, "z": 0 },
                        "scale": { "x": 2, "y": 1, "z": 1 }
                      },
                      {
                        "type": "RigidBody",
                        "bodyType": "Dynamic",
                        "mass": 2
                      },
                      {
                        "type": "BoxCollider",
                        "size": { "x": 1, "y": 1, "z": 1 }
                      }
                    ]
                  }
                ]
              }
            }
            """);
        var scene = new JsonSceneDescriptionLoader().Load(scenePath).Scene!;

        var definition = TestOnlySceneDataPhysicsAdapter.ToPhysicsWorldDefinition(scene);
        var world = PhysicsWorld.Load(definition);
        world.Step(new PhysicsStepContext(0.016d, 0.016d));
        var snapshot = world.CreateSnapshot();

        var body = Assert.Single(snapshot.Bodies);
        Assert.Equal("crate", body.BodyId);
        Assert.Equal(1, snapshot.StepCount);
        Assert.Equal(0.016d, snapshot.AccumulatedFixedSeconds);
        AssertVectorNearlyEqual(new Vector3(-1.0f, 0.5f, -0.5f), body.Aabb.Min);
        AssertVectorNearlyEqual(new Vector3(1.0f, 1.5f, 0.5f), body.Aabb.Max);
    }

    private static PhysicsBodyDefinition CreateBody(
        string bodyId,
        string bodyName,
        PhysicsBodyType bodyType,
        Vector3 position,
        Vector3 scale,
        Vector3 size,
        Vector3 center,
        double mass)
    {
        return new PhysicsBodyDefinition(
            bodyId,
            bodyName,
            bodyType,
            mass,
            new PhysicsTransform(position, Quaternion.Identity, scale),
            new PhysicsBoxColliderDefinition(size, center));
    }

    private static string WriteSceneFile(string content)
    {
        var directoryPath = Path.Combine(Path.GetTempPath(), "AnsEngine.Physics.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directoryPath);
        var scenePath = Path.Combine(directoryPath, "physics.scene.json");
        File.WriteAllText(scenePath, content);
        return scenePath;
    }

    private static void AssertVectorNearlyEqual(Vector3 expected, Vector3 actual)
    {
        Assert.Equal(expected.X, actual.X, 5);
        Assert.Equal(expected.Y, actual.Y, 5);
        Assert.Equal(expected.Z, actual.Z, 5);
    }

    private static class TestOnlySceneDataPhysicsAdapter
    {
        public static PhysicsWorldDefinition ToPhysicsWorldDefinition(SceneDescription scene)
        {
            var bodies = new List<PhysicsBodyDefinition>();
            foreach (var sceneObject in scene.Objects)
            {
                if (sceneObject.RigidBodyComponent is null || sceneObject.BoxColliderComponent is null)
                {
                    continue;
                }

                bodies.Add(
                    new PhysicsBodyDefinition(
                        sceneObject.ObjectId,
                        sceneObject.ObjectName,
                        sceneObject.RigidBodyComponent.BodyType == SceneRigidBodyType.Dynamic
                            ? PhysicsBodyType.Dynamic
                            : PhysicsBodyType.Static,
                        sceneObject.RigidBodyComponent.Mass,
                        new PhysicsTransform(
                            sceneObject.LocalTransform.Position,
                            sceneObject.LocalTransform.Rotation,
                            sceneObject.LocalTransform.Scale),
                        new PhysicsBoxColliderDefinition(
                            sceneObject.BoxColliderComponent.Size,
                            sceneObject.BoxColliderComponent.Center)));
            }

            return new PhysicsWorldDefinition(bodies);
        }
    }
}
