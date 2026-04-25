using Engine.Contracts;

namespace Engine.Asset;

public sealed class DiskMeshAssetProvider : IMeshAssetProvider
{
    private readonly string mCatalogPath;
    private readonly object mCacheLock = new();
    private readonly Dictionary<string, MeshAssetLoadResult> mResultCache = new(StringComparer.Ordinal);
    private MeshCatalog? mCatalog;

    public DiskMeshAssetProvider(string catalogPath)
    {
        if (string.IsNullOrWhiteSpace(catalogPath))
        {
            throw new ArgumentException("Catalog path must not be null or whitespace.", nameof(catalogPath));
        }

        mCatalogPath = Path.GetFullPath(catalogPath);
    }

    public MeshAssetLoadResult GetMesh(SceneMeshRef mesh)
    {
        lock (mCacheLock)
        {
            if (mResultCache.TryGetValue(mesh.MeshId, out var cachedResult))
            {
                return cachedResult;
            }

            var result = LoadMesh(mesh);
            mResultCache[mesh.MeshId] = result;
            return result;
        }
    }

    private MeshAssetLoadResult LoadMesh(SceneMeshRef mesh)
    {
        try
        {
            var catalog = EnsureCatalogLoaded();
            if (!catalog.TryResolve(mesh.MeshId, out var relativePath))
            {
                return MeshAssetLoadResult.FailureResult(
                    mesh,
                    new MeshAssetLoadFailure(MeshAssetLoadFailureKind.NotFound, $"Mesh '{mesh.MeshId}' was not found in the catalog."));
            }

            var meshPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(mCatalogPath)!, relativePath));
            if (!File.Exists(meshPath))
            {
                return MeshAssetLoadResult.FailureResult(
                    mesh,
                    new MeshAssetLoadFailure(MeshAssetLoadFailureKind.NotFound, $"Mesh source file '{meshPath}' was not found."));
            }

            if (!string.Equals(Path.GetExtension(meshPath), ".obj", StringComparison.OrdinalIgnoreCase))
            {
                return MeshAssetLoadResult.FailureResult(
                    mesh,
                    new MeshAssetLoadFailure(MeshAssetLoadFailureKind.UnsupportedFormat, $"Mesh source '{meshPath}' is not a supported OBJ file."));
            }

            var asset = ObjMeshFileLoader.Load(meshPath);
            return MeshAssetLoadResult.Success(mesh, asset);
        }
        catch (InvalidDataException ex)
        {
            return MeshAssetLoadResult.FailureResult(
                mesh,
                new MeshAssetLoadFailure(MeshAssetLoadFailureKind.InvalidData, ex.Message));
        }
        catch (FileNotFoundException ex)
        {
            return MeshAssetLoadResult.FailureResult(
                mesh,
                new MeshAssetLoadFailure(MeshAssetLoadFailureKind.NotFound, ex.Message));
        }
    }

    private MeshCatalog EnsureCatalogLoaded()
    {
        mCatalog ??= MeshCatalog.Load(mCatalogPath);
        return mCatalog;
    }
}
