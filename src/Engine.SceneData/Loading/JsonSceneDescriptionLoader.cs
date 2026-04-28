using Engine.SceneData.Abstractions;

namespace Engine.SceneData;

public sealed class JsonSceneDescriptionLoader : ISceneDescriptionLoader
{
    private readonly ISceneDocumentStore mDocumentStore;

    public JsonSceneDescriptionLoader()
        : this(new JsonSceneDocumentStore())
    {
    }

    internal JsonSceneDescriptionLoader(ISceneDocumentStore documentStore)
    {
        mDocumentStore = documentStore ?? throw new ArgumentNullException(nameof(documentStore));
    }

    public SceneDescriptionLoadResult Load(string sceneFilePath)
    {
        var documentResult = mDocumentStore.Load(sceneFilePath);
        if (!documentResult.IsSuccess)
        {
            return MapDocumentFailure(documentResult.Failure!);
        }

        return SceneFileDocumentNormalizer.Normalize(documentResult.Document!, sceneFilePath);
    }

    private static SceneDescriptionLoadResult MapDocumentFailure(SceneDocumentStoreFailure failure)
    {
        return SceneDescriptionLoadResult.FailureResult(
            new SceneDescriptionLoadFailure(
                MapFailureKind(failure.Kind),
                failure.Message,
                failure.Path,
                failure.LineNumber));
    }

    private static SceneDescriptionLoadFailureKind MapFailureKind(SceneDocumentStoreFailureKind kind)
    {
        return kind switch
        {
            SceneDocumentStoreFailureKind.NotFound => SceneDescriptionLoadFailureKind.NotFound,
            SceneDocumentStoreFailureKind.InvalidJson => SceneDescriptionLoadFailureKind.InvalidJson,
            SceneDocumentStoreFailureKind.InvalidPath => SceneDescriptionLoadFailureKind.MissingRequiredField,
            SceneDocumentStoreFailureKind.ReadFailed => SceneDescriptionLoadFailureKind.InvalidValue,
            SceneDocumentStoreFailureKind.WriteFailed => SceneDescriptionLoadFailureKind.InvalidValue,
            _ => SceneDescriptionLoadFailureKind.InvalidValue
        };
    }
}
