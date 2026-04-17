using Engine.Contracts;
using Engine.Core;
using System.Numerics;

namespace Engine.Scene;

public sealed class SceneGraphService : ISceneRenderContractProvider
{
    private const string DefaultMeshId = "mesh://triangle";
    private const string DefaultMaterialId = "material://default";
    private const string AnimatedMaterialId = "material://pulse";
    private const float PositionStepPerFrame = 0.00005f;
    private const float RotationStepPerFrameRadians = 0.005f;
    private const float CameraOrbitStepPerFrameRadians = 0.01f;
    private const float CameraOrbitRadius = 0.15f;
    private const float CameraDistance = 2.2f;
    private const float CameraFieldOfViewRadians = 1.0471976f;
    private const float CameraNearPlane = 0.1f;
    private const float CameraFarPlane = 10.0f;
    private const float CameraAspectRatio = 16.0f / 9.0f;

    private readonly EngineRuntimeInfo runtimeInfo;
    private readonly List<SceneRenderItem> renderItems = [];
    private int nextNodeId = 1;
    private int frameNumber;

    public SceneGraphService(EngineRuntimeInfo runtimeInfo)
    {
        this.runtimeInfo = runtimeInfo;
    }

    public int NodeCount { get; private set; }

    public void AddRootNode()
    {
        _ = runtimeInfo.EngineName;

        var nodeId = nextNodeId;
        nextNodeId += 1;
        NodeCount += 1;
        renderItems.Add(new SceneRenderItem(nodeId, DefaultMeshId, DefaultMaterialId, SceneTransform.Identity));
    }

    public SceneRenderFrame BuildRenderFrame()
    {
        if (renderItems.Count > 0)
        {
            var currentMaterialId = (frameNumber & 1) == 0 ? DefaultMaterialId : AnimatedMaterialId;
            var currentTransform = BuildFrameTransform(frameNumber);
            var firstItem = renderItems[0];
            renderItems[0] = firstItem with
            {
                MaterialId = currentMaterialId,
                Transform = currentTransform
            };
        }

        var itemsSnapshot = renderItems.ToArray();
        var camera = BuildFrameCamera(frameNumber);
        var frame = new SceneRenderFrame(frameNumber, itemsSnapshot, camera);
        frameNumber += 1;
        return frame;
    }

    private static SceneTransform BuildFrameTransform(int frameNumber)
    {
        var position = new Vector3(frameNumber * PositionStepPerFrame, 0.0f, 0.0f);
        var scale = Vector3.One;
        var rotation = Quaternion.CreateFromYawPitchRoll(frameNumber * RotationStepPerFrameRadians, 0.0f, 0.0f);
        return new SceneTransform(position, scale, rotation);
    }

    private static SceneCamera BuildFrameCamera(int frameNumber)
    {
        var orbitAngle = frameNumber * CameraOrbitStepPerFrameRadians;
        var cameraPosition = new Vector3(
            MathF.Sin(orbitAngle) * CameraOrbitRadius,
            0.0f,
            CameraDistance);
        var view = Matrix4x4.CreateLookAt(cameraPosition, Vector3.Zero, Vector3.UnitY);
        var projection = Matrix4x4.CreatePerspectiveFieldOfView(
            CameraFieldOfViewRadians,
            CameraAspectRatio,
            CameraNearPlane,
            CameraFarPlane);
        return new SceneCamera(view, projection);
    }
}
