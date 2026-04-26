using Engine.SceneData;

namespace Engine.SceneData.Abstractions;

public interface ISceneDescriptionLoader
{
    SceneDescriptionLoadResult Load(string sceneFilePath);
}
