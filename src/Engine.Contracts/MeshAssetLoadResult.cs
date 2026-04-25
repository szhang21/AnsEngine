namespace Engine.Contracts;

public sealed class MeshAssetLoadResult
{
    private MeshAssetLoadResult(SceneMeshRef mesh, MeshAssetData? asset, MeshAssetLoadFailure? failure)
    {
        Mesh = mesh;
        Asset = asset;
        Failure = failure;
    }

    public SceneMeshRef Mesh { get; }

    public bool IsSuccess => Asset is not null;

    public MeshAssetData? Asset { get; }

    public MeshAssetLoadFailure? Failure { get; }

    public static MeshAssetLoadResult Success(SceneMeshRef mesh, MeshAssetData asset)
    {
        ArgumentNullException.ThrowIfNull(asset);
        return new MeshAssetLoadResult(mesh, asset, null);
    }

    public static MeshAssetLoadResult FailureResult(SceneMeshRef mesh, MeshAssetLoadFailure failure)
    {
        ArgumentNullException.ThrowIfNull(failure);
        return new MeshAssetLoadResult(mesh, null, failure);
    }
}
