using Engine.Contracts;
using Engine.Core;
using Engine.Platform;

namespace Engine.Asset;

public sealed class NullAssetService : IAssetService, IMeshAssetProvider
{
    private readonly EngineRuntimeInfo mRuntimeInfo;
    private readonly IWindowService mWindowService;
    private readonly IMeshAssetProvider? mMeshAssetProvider;

    public NullAssetService(EngineRuntimeInfo runtimeInfo, IWindowService windowService)
        : this(runtimeInfo, windowService, meshAssetProvider: null)
    {
    }

    public NullAssetService(
        EngineRuntimeInfo runtimeInfo,
        IWindowService windowService,
        IMeshAssetProvider? meshAssetProvider)
    {
        mRuntimeInfo = runtimeInfo;
        mWindowService = windowService;
        mMeshAssetProvider = meshAssetProvider;
    }

    public IAssetHandle Load(string assetId)
    {
        _ = mRuntimeInfo.Version;
        _ = mWindowService.Configuration.Title;
        var isValid = !string.IsNullOrWhiteSpace(assetId);
        return new AssetHandle(assetId, isValid);
    }

    public MeshAssetLoadResult GetMesh(SceneMeshRef mesh)
    {
        _ = mRuntimeInfo.EngineName;
        _ = mWindowService.Exists;

        return mMeshAssetProvider is null
            ? MeshAssetLoadResult.FailureResult(
                mesh,
                new MeshAssetLoadFailure(MeshAssetLoadFailureKind.NotFound, "No mesh asset provider is configured."))
            : mMeshAssetProvider.GetMesh(mesh);
    }
}
