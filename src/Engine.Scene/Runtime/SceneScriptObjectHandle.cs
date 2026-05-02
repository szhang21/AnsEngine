using Engine.Contracts;

namespace Engine.Scene;

public sealed class SceneScriptObjectHandle
{
    private readonly SceneRuntimeObject mRuntimeObject;

    internal SceneScriptObjectHandle(SceneRuntimeObject runtimeObject)
    {
        mRuntimeObject = runtimeObject ?? throw new ArgumentNullException(nameof(runtimeObject));
    }

    public string ObjectId => mRuntimeObject.ObjectId;

    public string ObjectName => mRuntimeObject.ObjectName;

    public SceneTransform LocalTransform => mRuntimeObject.Transform!.ToSceneTransform();

    public void SetLocalTransform(SceneTransform transform)
    {
        mRuntimeObject.Transform!.SetLocalTransform(transform.Position, transform.Rotation, transform.Scale);
    }
}
