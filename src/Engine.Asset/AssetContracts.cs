namespace Engine.Asset;

public interface IAssetHandle
{
    string AssetId { get; }

    bool IsValid { get; }
}

public interface IAssetService
{
    IAssetHandle Load(string assetId);
}
