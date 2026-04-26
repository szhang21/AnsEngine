namespace Engine.SceneData;

public sealed record SceneDescriptionLoadResult
{
    private SceneDescriptionLoadResult(
        bool isSuccess,
        SceneDescription? scene,
        SceneDescriptionLoadFailure? failure)
    {
        IsSuccess = isSuccess;
        Scene = scene;
        Failure = failure;
    }

    public bool IsSuccess { get; }

    public SceneDescription? Scene { get; }

    public SceneDescriptionLoadFailure? Failure { get; }

    public static SceneDescriptionLoadResult Success(SceneDescription scene)
    {
        ArgumentNullException.ThrowIfNull(scene);
        return new SceneDescriptionLoadResult(true, scene, null);
    }

    public static SceneDescriptionLoadResult FailureResult(SceneDescriptionLoadFailure failure)
    {
        ArgumentNullException.ThrowIfNull(failure);
        return new SceneDescriptionLoadResult(false, null, failure);
    }
}
