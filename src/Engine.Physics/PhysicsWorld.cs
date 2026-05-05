using System.Numerics;

namespace Engine.Physics;

public sealed class PhysicsWorld
{
    private readonly List<PhysicsBodySnapshot> mBodies = new();
    private long mStepCount;
    private double mAccumulatedFixedSeconds;

    public int BodyCount => mBodies.Count;

    public long StepCount => mStepCount;

    public double AccumulatedFixedSeconds => mAccumulatedFixedSeconds;

    public static PhysicsWorld Load(PhysicsWorldDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);
        if (definition.Bodies is null)
        {
            throw new ArgumentException("Physics world definition requires a bodies collection.", nameof(definition));
        }

        var world = new PhysicsWorld();
        foreach (var body in definition.Bodies)
        {
            world.mBodies.Add(CreateBodySnapshot(body));
        }

        return world;
    }

    public void Step(PhysicsStepContext context)
    {
        if (!double.IsFinite(context.FixedDeltaSeconds) || context.FixedDeltaSeconds <= 0.0d)
        {
            throw new ArgumentException("Physics step fixed delta seconds must be positive and finite.", nameof(context));
        }

        if (!double.IsFinite(context.TotalSeconds) || context.TotalSeconds < 0.0d)
        {
            throw new ArgumentException("Physics step total seconds must be non-negative and finite.", nameof(context));
        }

        mStepCount += 1;
        mAccumulatedFixedSeconds += context.FixedDeltaSeconds;
    }

    public PhysicsWorldSnapshot CreateSnapshot()
    {
        return new PhysicsWorldSnapshot(mBodies.ToArray(), StepCount, AccumulatedFixedSeconds);
    }

    public PhysicsQueryResult QueryAabb(PhysicsAabb aabb)
    {
        if (!IsValidAabb(aabb))
        {
            throw new ArgumentException("Physics AABB query requires finite min/max values with min <= max.", nameof(aabb));
        }

        return new PhysicsQueryResult(mBodies.Where(body => Overlaps(body.Aabb, aabb)).ToArray());
    }

    public PhysicsGroundQueryResult QueryGround(string bodyId)
    {
        if (string.IsNullOrWhiteSpace(bodyId))
        {
            throw new ArgumentException("Physics ground query requires a body id.", nameof(bodyId));
        }

        var body = mBodies.FirstOrDefault(item => string.Equals(item.BodyId, bodyId, StringComparison.Ordinal));
        if (body is null)
        {
            throw new ArgumentException($"Physics body '{bodyId}' was not found.", nameof(bodyId));
        }

        return PhysicsGroundQueryResult.FromAabb(body.BodyId, body.Aabb);
    }

    public PhysicsKinematicMoveResult ResolveKinematicMove(string bodyId, PhysicsTransform desiredTransform)
    {
        if (string.IsNullOrWhiteSpace(bodyId))
        {
            throw new ArgumentException("Physics kinematic move requires a body id.", nameof(bodyId));
        }

        if (!IsFinite(desiredTransform.Position) ||
            !IsFinite(desiredTransform.Rotation) ||
            !IsFinite(desiredTransform.Scale))
        {
            throw new ArgumentException("Physics kinematic move desired transform values must be finite.", nameof(desiredTransform));
        }

        var mover = mBodies.FirstOrDefault(body => string.Equals(body.BodyId, bodyId, StringComparison.Ordinal));
        if (mover is null)
        {
            throw new ArgumentException($"Physics body '{bodyId}' was not found.", nameof(bodyId));
        }

        if (mover.BodyType != PhysicsBodyType.Dynamic)
        {
            throw new ArgumentException($"Physics body '{bodyId}' must be Dynamic to resolve a kinematic move.", nameof(bodyId));
        }

        var currentTransform = mover.Transform;
        var resolvedPosition = currentTransform.Position;
        var delta = desiredTransform.Position - currentTransform.Position;
        string? firstBlockingBodyId = null;

        TryApplyAxis(
            mover,
            desiredTransform,
            delta.X,
            new Vector3(1.0f, 0.0f, 0.0f),
            ref resolvedPosition,
            ref firstBlockingBodyId);
        TryApplyAxis(
            mover,
            desiredTransform,
            delta.Y,
            new Vector3(0.0f, 1.0f, 0.0f),
            ref resolvedPosition,
            ref firstBlockingBodyId);
        TryApplyAxis(
            mover,
            desiredTransform,
            delta.Z,
            new Vector3(0.0f, 0.0f, 1.0f),
            ref resolvedPosition,
            ref firstBlockingBodyId);

        var resolvedTransform = new PhysicsTransform(
            resolvedPosition,
            desiredTransform.Rotation,
            desiredTransform.Scale);
        return new PhysicsKinematicMoveResult(
            bodyId,
            desiredTransform,
            resolvedTransform,
            firstBlockingBodyId is not null,
            firstBlockingBodyId);
    }

    private static PhysicsBodySnapshot CreateBodySnapshot(PhysicsBodyDefinition body)
    {
        ArgumentNullException.ThrowIfNull(body);

        if (string.IsNullOrWhiteSpace(body.BodyId))
        {
            throw new ArgumentException("Physics body definition requires a body id.", nameof(body));
        }

        if (!Enum.IsDefined(typeof(PhysicsBodyType), body.BodyType))
        {
            throw new ArgumentException($"Physics body '{body.BodyId}' has unsupported body type.", nameof(body));
        }

        if (body.Transform is null)
        {
            throw new ArgumentException($"Physics body '{body.BodyId}' requires a transform definition.", nameof(body));
        }

        if (body.BoxCollider is null)
        {
            throw new ArgumentException($"Physics body '{body.BodyId}' requires a box collider definition.", nameof(body));
        }

        if (!double.IsFinite(body.Mass) || body.Mass < 0.0d)
        {
            throw new ArgumentException($"Physics body '{body.BodyId}' mass must be non-negative and finite.", nameof(body));
        }

        if (body.BodyType == PhysicsBodyType.Dynamic && body.Mass <= 0.0d)
        {
            throw new ArgumentException($"Dynamic physics body '{body.BodyId}' mass must be positive.", nameof(body));
        }

        var transform = body.Transform.Value;
        var collider = body.BoxCollider;
        if (!IsFinite(transform.Position) || !IsFinite(transform.Rotation) || !IsFinite(transform.Scale))
        {
            throw new ArgumentException($"Physics body '{body.BodyId}' transform values must be finite.", nameof(body));
        }

        if (!IsFinite(collider.Size) || collider.Size.X <= 0.0f || collider.Size.Y <= 0.0f || collider.Size.Z <= 0.0f)
        {
            throw new ArgumentException($"Physics body '{body.BodyId}' box collider size must contain positive finite values.", nameof(body));
        }

        if (!IsFinite(collider.Center))
        {
            throw new ArgumentException($"Physics body '{body.BodyId}' box collider center must contain finite values.", nameof(body));
        }

        var bodyName = string.IsNullOrWhiteSpace(body.BodyName) ? body.BodyId : body.BodyName;
        return new PhysicsBodySnapshot(
            body.BodyId,
            bodyName,
            body.BodyType,
            body.Mass,
            transform,
            collider,
            CalculateAabb(transform, collider));
    }

    private static PhysicsAabb CalculateAabb(PhysicsTransform transform, PhysicsBoxColliderDefinition collider)
    {
        var center = transform.Position + collider.Center;
        var scale = Vector3.Abs(transform.Scale);
        var halfExtents = collider.Size * scale * 0.5f;
        return new PhysicsAabb(center - halfExtents, center + halfExtents);
    }

    private static bool Overlaps(PhysicsAabb left, PhysicsAabb right)
    {
        return left.Min.X <= right.Max.X &&
               left.Max.X >= right.Min.X &&
               left.Min.Y <= right.Max.Y &&
               left.Max.Y >= right.Min.Y &&
               left.Min.Z <= right.Max.Z &&
               left.Max.Z >= right.Min.Z;
    }

    private static bool IsValidAabb(PhysicsAabb aabb)
    {
        return IsFinite(aabb.Min) &&
               IsFinite(aabb.Max) &&
               aabb.Min.X <= aabb.Max.X &&
               aabb.Min.Y <= aabb.Max.Y &&
               aabb.Min.Z <= aabb.Max.Z;
    }

    private static bool IsFinite(Vector3 value)
    {
        return float.IsFinite(value.X) &&
               float.IsFinite(value.Y) &&
               float.IsFinite(value.Z);
    }

    private static bool IsFinite(Quaternion value)
    {
        return float.IsFinite(value.X) &&
               float.IsFinite(value.Y) &&
               float.IsFinite(value.Z) &&
               float.IsFinite(value.W);
    }

    private void TryApplyAxis(
        PhysicsBodySnapshot mover,
        PhysicsTransform desiredTransform,
        float axisDelta,
        Vector3 axis,
        ref Vector3 resolvedPosition,
        ref string? firstBlockingBodyId)
    {
        if (axisDelta == 0.0f)
        {
            return;
        }

        var candidatePosition = resolvedPosition + (axis * axisDelta);
        var candidateTransform = new PhysicsTransform(
            candidatePosition,
            desiredTransform.Rotation,
            desiredTransform.Scale);
        var candidateAabb = CalculateAabb(candidateTransform, mover.BoxCollider);
        var blockingBody = mBodies.FirstOrDefault(body =>
            body.BodyType == PhysicsBodyType.Static &&
            !string.Equals(body.BodyId, mover.BodyId, StringComparison.Ordinal) &&
            Overlaps(candidateAabb, body.Aabb));
        if (blockingBody is not null)
        {
            firstBlockingBodyId ??= blockingBody.BodyId;
            return;
        }

        resolvedPosition = candidatePosition;
    }
}

public sealed record PhysicsKinematicMoveResult(
    string BodyId,
    PhysicsTransform DesiredTransform,
    PhysicsTransform ResolvedTransform,
    bool HasHit,
    string? BlockingBodyId);
