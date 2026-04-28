namespace Engine.SceneData;

public sealed record SceneDocumentSaveResult
{
    private SceneDocumentSaveResult(SceneDocumentStoreFailure? failure)
    {
        Failure = failure;
    }

    public bool IsSuccess => Failure is null;

    public SceneDocumentStoreFailure? Failure { get; }

    public static SceneDocumentSaveResult Success()
    {
        return new SceneDocumentSaveResult(failure: null);
    }

    public static SceneDocumentSaveResult FailureResult(SceneDocumentStoreFailure failure)
    {
        ArgumentNullException.ThrowIfNull(failure);
        return new SceneDocumentSaveResult(failure);
    }
}
