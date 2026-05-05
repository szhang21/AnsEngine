using Engine.Contracts;
using Engine.SceneData;
using Engine.Platform;
using Engine.Scene;

namespace Engine.App;

public interface ISceneRuntime
{
    void InitializeScene(SceneDescription sceneDescription);
    SceneScriptObjectBindResult BindScriptObject(string objectId);
    RuntimeSceneSnapshot CreateRuntimeSnapshot();
    SceneTransformWriteResult TrySetObjectTransform(string objectId, SceneTransform transform);
    void Update(TimeSnapshot time, InputSnapshot input);
}
