using Engine.Contracts;
using System.Runtime.CompilerServices;

namespace Engine.Render;

internal sealed class SceneRenderMeshGeometryCache
{
    private readonly ConditionalWeakTable<IMeshAssetProvider, Dictionary<string, SceneRenderMeshGeometry>> providerCaches = new();
    private readonly SceneRenderMeshGeometry fallbackGeometry;

    public SceneRenderMeshGeometryCache(SceneRenderMeshGeometry fallbackGeometry)
    {
        this.fallbackGeometry = fallbackGeometry ?? throw new ArgumentNullException(nameof(fallbackGeometry));
    }

    public SceneRenderMeshGeometry Resolve(SceneMeshRef mesh, IMeshAssetProvider? meshAssetProvider)
    {
        if (meshAssetProvider is null)
        {
            return fallbackGeometry;
        }

        var cache = providerCaches.GetValue(meshAssetProvider, _ => new Dictionary<string, SceneRenderMeshGeometry>(StringComparer.Ordinal));
        lock (cache)
        {
            if (cache.TryGetValue(mesh.MeshId, out var cachedGeometry))
            {
                return cachedGeometry;
            }
        }

        var resolvedGeometry = ResolveGeometry(mesh, meshAssetProvider);
        lock (cache)
        {
            if (!cache.TryGetValue(mesh.MeshId, out var cachedGeometry))
            {
                cache.Add(mesh.MeshId, resolvedGeometry);
                return resolvedGeometry;
            }

            return cachedGeometry;
        }
    }

    private SceneRenderMeshGeometry ResolveGeometry(SceneMeshRef mesh, IMeshAssetProvider meshAssetProvider)
    {
        var result = meshAssetProvider.GetMesh(mesh);
        if (!result.IsSuccess || result.Asset is null)
        {
            return fallbackGeometry;
        }

        return TryExpandMesh(result.Asset, mesh.MeshId, out var geometry)
            ? geometry
            : fallbackGeometry;
    }

    private static bool TryExpandMesh(
        MeshAssetData asset,
        string meshId,
        out SceneRenderMeshGeometry geometry)
    {
        if (asset.Vertices.Count == 0 || asset.Indices.Count == 0 || asset.Indices.Count % 3 != 0)
        {
            geometry = default!;
            return false;
        }

        var vertices = new SceneRenderMeshVertex[asset.Indices.Count];
        for (var index = 0; index < asset.Indices.Count; index += 1)
        {
            var sourceIndex = asset.Indices[index];
            if (sourceIndex < 0 || sourceIndex >= asset.Vertices.Count)
            {
                geometry = default!;
                return false;
            }

            var assetVertex = asset.Vertices[sourceIndex];
            vertices[index] = new SceneRenderMeshVertex(
                assetVertex.Position.X,
                assetVertex.Position.Y,
                assetVertex.Position.Z);
        }

        geometry = new SceneRenderMeshGeometry(meshId, vertices);
        return true;
    }
}
