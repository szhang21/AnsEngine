namespace Engine.SceneData;

public sealed record SceneDocumentEditResult
{
    private SceneDocumentEditResult(SceneFileDocument? document, SceneDocumentEditFailure? failure)
    {
        Document = document;
        Failure = failure;
    }

    public bool IsSuccess => Failure is null;

    public SceneFileDocument? Document { get; }

    public SceneDocumentEditFailure? Failure { get; }

    public static SceneDocumentEditResult Success(SceneFileDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);
        return new SceneDocumentEditResult(document, failure: null);
    }

    public static SceneDocumentEditResult FailureResult(SceneDocumentEditFailure failure)
    {
        ArgumentNullException.ThrowIfNull(failure);
        return new SceneDocumentEditResult(document: null, failure);
    }
}
