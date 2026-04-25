namespace Engine.Contracts;

public sealed record MeshAssetData
{
    public MeshAssetData(IReadOnlyList<MeshAssetVertex> vertices, IReadOnlyList<int> indices)
    {
        Vertices = vertices ?? throw new ArgumentNullException(nameof(vertices));
        Indices = indices ?? throw new ArgumentNullException(nameof(indices));
    }

    public IReadOnlyList<MeshAssetVertex> Vertices { get; }

    public IReadOnlyList<int> Indices { get; }
}
