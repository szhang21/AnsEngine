namespace Engine.Render;

internal sealed class SceneRenderMeshGeometry
{
    public SceneRenderMeshGeometry(string meshCacheKey, IReadOnlyList<SceneRenderMeshVertex> vertices)
    {
        if (string.IsNullOrWhiteSpace(meshCacheKey))
        {
            throw new ArgumentException("Mesh cache key must not be null or whitespace.", nameof(meshCacheKey));
        }

        MeshCacheKey = meshCacheKey;
        Vertices = vertices ?? throw new ArgumentNullException(nameof(vertices));
    }

    public string MeshCacheKey { get; }

    public IReadOnlyList<SceneRenderMeshVertex> Vertices { get; }
}
