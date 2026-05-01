using Engine.SceneData;
using Engine.Platform;

namespace Engine.App;

public interface ISceneRuntime
{
    void InitializeScene(SceneDescription sceneDescription);
    void Update(TimeSnapshot time, InputSnapshot input);
}
