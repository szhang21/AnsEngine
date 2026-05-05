using Engine.Contracts;
using Engine.Core;
using Engine.SceneData;

namespace Engine.Scene;

public sealed class SceneGraphService : ISceneRenderContractProvider
{
    private const string kDefaultMeshId = "mesh://cube";
    private const string kMissingMeshId = "mesh://missing";
    private const string kDefaultMaterialId = "material://default";
    private const float kCameraNearPlane = 0.1f;
    private const float kCameraFarPlane = 10.0f;
    private const float kCameraAspectRatio = 16.0f / 9.0f;

    private readonly EngineRuntimeInfo mRuntimeInfo;
    private readonly RuntimeScene mRuntimeScene = new();
    private int mNextNodeId = 1;
    private int mFrameNumber;

    public SceneGraphService(EngineRuntimeInfo runtimeInfo)
    {
        mRuntimeInfo = runtimeInfo;
    }

    public int NodeCount => mRuntimeScene.ObjectCount;

    internal RuntimeScene RuntimeScene => mRuntimeScene;

    public RuntimeSceneSnapshot CreateRuntimeSnapshot()
    {
        return mRuntimeScene.CreateSnapshot();
    }

    public SceneRuntimeObjectSnapshot? FindObject(string objectId)
    {
        return mRuntimeScene.FindObject(objectId);
    }

    public SceneScriptObjectBindResult BindScriptObject(string objectId)
    {
        return mRuntimeScene.BindScriptObject(objectId);
    }

    public SceneTransformWriteResult TrySetObjectTransform(string objectId, SceneTransform transform)
    {
        return mRuntimeScene.TrySetObjectTransform(objectId, transform);
    }

    public void AddRootNode()
    {
        _ = mRuntimeInfo.EngineName;

        var nodeId = mNextNodeId;
        mNextNodeId += 1;
        mRuntimeScene.CreateObject(
            nodeId,
            $"node-{nodeId}",
            $"Node {nodeId}",
            SceneTransformComponent.FromDescription(SceneTransformDescription.Identity),
            new SceneMeshRendererComponent(
                new SceneMeshRef(GetDefaultMeshId(nodeId)),
                new SceneMaterialRef(kDefaultMaterialId)));
    }

    public void LoadSceneDescription(SceneDescription sceneDescription)
    {
        ArgumentNullException.ThrowIfNull(sceneDescription);

        _ = mRuntimeInfo.EngineName;

        mRuntimeScene.LoadFromDescription(sceneDescription);
        mFrameNumber = 0;
        mNextNodeId = mRuntimeScene.ObjectCount + 1;
    }

    public void UpdateRuntime(SceneUpdateContext context)
    {
        mRuntimeScene.Update(context);
    }

    public SceneRenderFrame BuildRenderFrame()
    {
        var items = mRuntimeScene.BuildRenderItems();
        var camera = mRuntimeScene.BuildCamera(kCameraAspectRatio, kCameraNearPlane, kCameraFarPlane);
        var frame = new SceneRenderFrame(mFrameNumber, items, camera);
        mFrameNumber += 1;
        return frame;
    }

    private static string GetDefaultMeshId(int nodeId)
    {
        return (nodeId & 1) == 0 ? kMissingMeshId : kDefaultMeshId;
    }
}
