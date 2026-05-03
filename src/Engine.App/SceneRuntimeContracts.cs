using Engine.SceneData;
using Engine.Platform;
using Engine.Scene;

namespace Engine.App;

public interface ISceneRuntime
{
    void InitializeScene(SceneDescription sceneDescription);
    SceneScriptObjectBindResult BindScriptObject(string objectId);
    void Update(TimeSnapshot time, InputSnapshot input);
}
