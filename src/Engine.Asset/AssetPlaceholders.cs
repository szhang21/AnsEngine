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
    private readonly EngineRuntimeInfo _runtimeInfo;
    private readonly IWindowService _windowService;

    public NullAssetService(EngineRuntimeInfo runtimeInfo, IWindowService windowService)
    {
        _runtimeInfo = runtimeInfo;
        _windowService = windowService;
    }

    public IAssetHandle Load(string assetId)
    {
        _ = _runtimeInfo.Version;
        _ = _windowService.Configuration.Title;
        var isValid = !string.IsNullOrWhiteSpace(assetId);
        return new AssetHandle(assetId, isValid);
    }
}
