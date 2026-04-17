using Engine.Contracts;
using Engine.Core;
using System.Numerics;

namespace Engine.Scene;

public sealed class SceneGraphService : ISceneRenderContractProvider
{
    private const string kDefaultMeshId = "mesh://triangle";
    private const string kDefaultMaterialId = "material://default";
    private const string kAnimatedMaterialId = "material://pulse";
    private const float kPositionStepPerFrame = 0.00005f;
    private const float kRotationStepPerFrameRadians = 0.005f;
    private const float kCameraOrbitStepPerFrameRadians = 0.01f;
    private const float kCameraOrbitRadius = 0.15f;
    private const float kCameraDistance = 2.2f;
    private const float kCameraFieldOfViewRadians = 1.0471976f;
    private const float kCameraNearPlane = 0.1f;
    private const float kCameraFarPlane = 10.0f;
    private const float kCameraAspectRatio = 16.0f / 9.0f;

    private readonly EngineRuntimeInfo mRuntimeInfo;
    private readonly List<SceneRenderItem> mRenderItems = [];
    private int mNextNodeId = 1;
    private int mFrameNumber;

    public SceneGraphService(EngineRuntimeInfo runtimeInfo)
    {
        mRuntimeInfo = runtimeInfo;
    }

    public int NodeCount { get; private set; }

    public void AddRootNode()
    {
        _ = mRuntimeInfo.EngineName;

        var nodeId = mNextNodeId;
        mNextNodeId += 1;
        NodeCount += 1;
        mRenderItems.Add(new SceneRenderItem(nodeId, kDefaultMeshId, kDefaultMaterialId, SceneTransform.Identity));
    }

    public SceneRenderFrame BuildRenderFrame()
    {
        if (mRenderItems.Count > 0)
        {
            var currentMaterialId = (mFrameNumber & 1) == 0 ? kDefaultMaterialId : kAnimatedMaterialId;
            var currentTransform = BuildFrameTransform(mFrameNumber);
            var firstItem = mRenderItems[0];
            mRenderItems[0] = firstItem with
            {
                MaterialId = currentMaterialId,
                Transform = currentTransform
            };
        }

        var itemsSnapshot = mRenderItems.ToArray();
        var camera = BuildFrameCamera(mFrameNumber);
        var frame = new SceneRenderFrame(mFrameNumber, itemsSnapshot, camera);
        mFrameNumber += 1;
        return frame;
    }

    private static SceneTransform BuildFrameTransform(int frameNumber)
    {
        var position = new Vector3(frameNumber * kPositionStepPerFrame, 0.0f, 0.0f);
        var scale = Vector3.One;
        var rotation = Quaternion.CreateFromYawPitchRoll(frameNumber * kRotationStepPerFrameRadians, 0.0f, 0.0f);
        return new SceneTransform(position, scale, rotation);
    }

    private static SceneCamera BuildFrameCamera(int frameNumber)
    {
        var orbitAngle = frameNumber * kCameraOrbitStepPerFrameRadians;
        var cameraPosition = new Vector3(
            MathF.Sin(orbitAngle) * kCameraOrbitRadius,
            0.0f,
            kCameraDistance);
        var view = Matrix4x4.CreateLookAt(cameraPosition, Vector3.Zero, Vector3.UnitY);
        var projection = Matrix4x4.CreatePerspectiveFieldOfView(
            kCameraFieldOfViewRadians,
            kCameraAspectRatio,
            kCameraNearPlane,
            kCameraFarPlane);
        return new SceneCamera(view, projection);
    }
}
