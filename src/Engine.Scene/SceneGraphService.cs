using Engine.Contracts;
using Engine.Core;
using System.Numerics;

namespace Engine.Scene;

public sealed class SceneGraphService : ISceneRenderContractProvider
{
    private const string kDefaultMeshId = "mesh://cube";
    private const string kMissingMeshId = "mesh://missing";
    private const string kDefaultMaterialId = "material://default";
    private const string kPulseMaterialId = "material://pulse";
    private const string kHighlightMaterialId = "material://highlight";
    private const string kMissingMaterialId = "material://missing";
    private const float kPositionStepPerFrame = 0.00005f;
    private const float kRotationStepPerFrameRadians = 0.005f;
    private const float kCameraOrbitStepPerFrameRadians = 0.01f;
    private const float kCameraOrbitRadius = 0.15f;
    private const float kCameraDistance = 2.2f;
    private const float kCameraFieldOfViewRadians = 1.0471976f;
    private const float kCameraNearPlane = 0.1f;
    private const float kCameraFarPlane = 10.0f;
    private const float kCameraAspectRatio = 16.0f / 9.0f;
    private static readonly string[] sMaterialCycle =
    {
        kDefaultMaterialId,
        kPulseMaterialId,
        kHighlightMaterialId,
        kMissingMaterialId
    };
    private static readonly HashSet<string> sSupportedMaterialIds = new(StringComparer.Ordinal)
    {
        kDefaultMaterialId,
        kPulseMaterialId,
        kHighlightMaterialId
    };

    private readonly EngineRuntimeInfo mRuntimeInfo;
    private readonly List<SceneRenderItem> mRenderItems = new();
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
        mRenderItems.Add(CreateRenderItem(nodeId, kDefaultMeshId, kDefaultMaterialId, SceneTransform.Identity));
    }

    public SceneRenderFrame BuildRenderFrame()
    {
        for (var index = 0; index < mRenderItems.Count; index += 1)
        {
            var item = mRenderItems[index];
            var meshCandidate = BuildMeshCandidate(item.NodeId);
            var materialCandidate = BuildMaterialCandidate(mFrameNumber, item.NodeId);
            var currentTransform = BuildFrameTransform(mFrameNumber, index);
            mRenderItems[index] = CreateRenderItem(item.NodeId, meshCandidate, materialCandidate, currentTransform);
        }

        var itemsSnapshot = mRenderItems.ToArray();
        var camera = BuildFrameCamera(mFrameNumber);
        var frame = new SceneRenderFrame(mFrameNumber, itemsSnapshot, camera);
        mFrameNumber += 1;
        return frame;
    }

    private static SceneRenderItem CreateRenderItem(
        int nodeId,
        string meshCandidate,
        string materialCandidate,
        SceneTransform transform)
    {
        var materialId = ResolveMaterialId(materialCandidate);
        return new SceneRenderItem(nodeId, new SceneMeshRef(meshCandidate), new SceneMaterialRef(materialId), transform);
    }

    private static string BuildMeshCandidate(int nodeId)
    {
        return (nodeId & 1) == 0 ? kMissingMeshId : kDefaultMeshId;
    }

    private static string BuildMaterialCandidate(int frameNumber, int nodeId)
    {
        var cycleIndex = (frameNumber + nodeId - 1) % sMaterialCycle.Length;
        return sMaterialCycle[cycleIndex];
    }

    private static string ResolveMaterialId(string materialCandidate)
    {
        return sSupportedMaterialIds.Contains(materialCandidate) ? materialCandidate : kDefaultMaterialId;
    }

    private static SceneTransform BuildFrameTransform(int frameNumber, int itemIndex)
    {
        var position = new Vector3(frameNumber * kPositionStepPerFrame, itemIndex * 0.2f, 0.0f);
        var scale = Vector3.One;
        var rotation = Quaternion.CreateFromYawPitchRoll(
            frameNumber * kRotationStepPerFrameRadians,
            frameNumber * (kRotationStepPerFrameRadians * 0.6f),
            0.0f);
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
