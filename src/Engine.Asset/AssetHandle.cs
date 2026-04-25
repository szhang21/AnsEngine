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
