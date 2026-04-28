namespace Engine.SceneData.Abstractions;

public interface ISceneDocumentStore
{
    SceneDocumentLoadResult Load(string sceneFilePath);

    SceneDocumentSaveResult Save(string sceneFilePath, SceneFileDocument document);
}
