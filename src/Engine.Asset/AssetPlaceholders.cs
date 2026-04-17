using Engine.Core;
using Engine.Platform;

namespace Engine.Asset;

public sealed class AssetHandle : IAssetHandle
{
    public AssetHandle(string assetId, bool isValid)
    {
        AssetId = assetId;
        IsValid = isValid;
    }

    public string AssetId { get; }

    public bool IsValid { get; }
}

public sealed class NullAssetService : IAssetService
{
    private readonly EngineRuntimeInfo mRuntimeInfo;
    private readonly IWindowService mWindowService;

    public NullAssetService(EngineRuntimeInfo runtimeInfo, IWindowService windowService)
    {
        mRuntimeInfo = runtimeInfo;
        mWindowService = windowService;
    }

    public IAssetHandle Load(string assetId)
    {
        _ = mRuntimeInfo.Version;
        _ = mWindowService.Configuration.Title;
        var isValid = !string.IsNullOrWhiteSpace(assetId);
        return new AssetHandle(assetId, isValid);
    }
}
