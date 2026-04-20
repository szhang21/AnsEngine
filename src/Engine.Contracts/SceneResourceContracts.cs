namespace Engine.Contracts;

public readonly record struct SceneMeshRef
{
    public SceneMeshRef(string meshId)
    {
        if (string.IsNullOrWhiteSpace(meshId))
        {
            throw new ArgumentException("MeshId must not be null or whitespace.", nameof(meshId));
        }

        MeshId = meshId;
    }

    public string MeshId { get; }
}

public readonly record struct SceneMaterialRef
{
    public SceneMaterialRef(string materialId)
    {
        if (string.IsNullOrWhiteSpace(materialId))
        {
            throw new ArgumentException("MaterialId must not be null or whitespace.", nameof(materialId));
        }

        MaterialId = materialId;
    }

    public string MaterialId { get; }
}
