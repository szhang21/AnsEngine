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
    private readonly EngineRuntimeInfo runtimeInfo;
    private readonly IWindowService windowService;

    public NullAssetService(EngineRuntimeInfo runtimeInfo, IWindowService windowService)
    {
        this.runtimeInfo = runtimeInfo;
        this.windowService = windowService;
    }

    public IAssetHandle Load(string assetId)
    {
        _ = runtimeInfo.Version;
        _ = windowService.Configuration.Title;
        var isValid = !string.IsNullOrWhiteSpace(assetId);
        return new AssetHandle(assetId, isValid);
    }
}
