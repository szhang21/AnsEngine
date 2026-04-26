using Engine.SceneData;

namespace Engine.App;

public interface ISceneRuntime
{
    void InitializeScene(SceneDescription sceneDescription);
}
