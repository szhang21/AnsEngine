using Engine.Asset;
using Engine.Core;
using Engine.Platform;

namespace Engine.Asset.Tests;

public sealed class AssetServiceTests
{
    [Fact]
    public void Load_ReturnsValidHandle_ForNonEmptyAssetId()
    {
        var runtimeInfo = new EngineRuntimeInfo("AnsEngine", "0.1.0");
        var windowService = new NullWindowService(new WindowConfig(1280, 720, "AnsEngine"), useNativeWindow: false);
        var assetService = new NullAssetService(runtimeInfo, windowService);

        IAssetHandle handle = assetService.Load("asset://placeholder");

        Assert.True(handle.IsValid);
        Assert.Equal("asset://placeholder", handle.AssetId);
    }
}
