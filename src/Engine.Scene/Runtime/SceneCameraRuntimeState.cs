using Engine.Contracts;
using Engine.SceneData;
using System.Numerics;

namespace Engine.Scene;

internal sealed class SceneCameraRuntimeState
{
    private const float kDefaultCameraDistance = 2.2f;
    private const float kDefaultCameraFieldOfViewRadians = 1.0471976f;

    public SceneCameraRuntimeState(Vector3 position, Vector3 target, float fieldOfViewRadians)
    {
        Position = position;
        Target = target;
        FieldOfViewRadians = fieldOfViewRadians;
    }

    public Vector3 Position { get; }

    public Vector3 Target { get; }

    public float FieldOfViewRadians { get; }

    public static SceneCameraRuntimeState FromDescription(SceneCameraDescription? cameraDescription)
    {
        return cameraDescription is null
            ? CreateDefault()
            : new SceneCameraRuntimeState(
                cameraDescription.Position,
                cameraDescription.Target,
                cameraDescription.FieldOfViewRadians);
    }

    public static SceneCameraRuntimeState CreateDefault()
    {
        return new SceneCameraRuntimeState(
            new Vector3(0.0f, 0.0f, kDefaultCameraDistance),
            Vector3.Zero,
            kDefaultCameraFieldOfViewRadians);
    }

    public SceneCamera BuildCamera(float aspectRatio, float nearPlane, float farPlane)
    {
        var view = Matrix4x4.CreateLookAt(Position, Target, Vector3.UnitY);
        var projection = Matrix4x4.CreatePerspectiveFieldOfView(
            FieldOfViewRadians,
            aspectRatio,
            nearPlane,
            farPlane);
        return new SceneCamera(view, projection);
    }

    public SceneCameraRuntimeSnapshot CreateSnapshot()
    {
        return new SceneCameraRuntimeSnapshot(Position, Target, FieldOfViewRadians);
    }
}
