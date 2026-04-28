namespace Engine.SceneData;

public sealed record SceneDocumentLoadResult
{
    private SceneDocumentLoadResult(SceneFileDocument? document, SceneDocumentStoreFailure? failure)
    {
        Document = document;
        Failure = failure;
    }

    public bool IsSuccess => Failure is null;

    public SceneFileDocument? Document { get; }

    public SceneDocumentStoreFailure? Failure { get; }

    public static SceneDocumentLoadResult Success(SceneFileDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);
        return new SceneDocumentLoadResult(document, failure: null);
    }

    public static SceneDocumentLoadResult FailureResult(SceneDocumentStoreFailure failure)
    {
        ArgumentNullException.ThrowIfNull(failure);
        return new SceneDocumentLoadResult(document: null, failure);
    }
}
