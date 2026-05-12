using Engine.Contracts;
using Engine.Runtime.Abstractions;
using Engine.SceneData;
using System.Numerics;

namespace Engine.Scene;

internal sealed class SceneTransformComponent : IRuntimeTransformComponent
{
    public SceneTransformComponent(Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
    {
        LocalPosition = localPosition;
        LocalRotation = localRotation;
        LocalScale = localScale;
    }

    public Vector3 LocalPosition { get; private set; }

    public Quaternion LocalRotation { get; private set; }

    public Vector3 LocalScale { get; private set; }

    public SceneTransform LocalTransform => ToSceneTransform();

    public static SceneTransformComponent FromDescription(SceneTransformDescription transformDescription)
    {
        return new SceneTransformComponent(
            transformDescription.Position,
            transformDescription.Rotation,
            transformDescription.Scale);
    }

    public static SceneTransformComponent FromDescription(SceneTransformComponentDescription transformDescription)
    {
        ArgumentNullException.ThrowIfNull(transformDescription);
        return FromDescription(transformDescription.Transform);
    }

    public SceneTransform ToSceneTransform()
    {
        return new SceneTransform(LocalPosition, LocalScale, LocalRotation);
    }

    public void SetLocalTransform(Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
    {
        LocalPosition = localPosition;
        LocalRotation = localRotation;
        LocalScale = localScale;
    }

    public void SetLocalTransform(SceneTransform transform)
    {
        SetLocalTransform(transform.Position, transform.Rotation, transform.Scale);
    }
}
