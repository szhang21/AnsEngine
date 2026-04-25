using Engine.Asset;
using Engine.Contracts;
using Engine.Core;
using Engine.Platform;
using System.Numerics;

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

    [Fact]
    public void GetMesh_DelegatesToDiskProvider_WhenProviderIsConfigured()
    {
        using var assets = TestAssetWorkspace.Create(
            ("catalog.txt", "mesh://triangle=triangle.obj"),
            ("triangle.obj", CreateTriangleObj()));

        var runtimeInfo = new EngineRuntimeInfo("AnsEngine", "0.1.0");
        using var windowService = new NullWindowService(new WindowConfig(1280, 720, "AnsEngine"), useNativeWindow: false);
        var provider = new DiskMeshAssetProvider(assets.GetPath("catalog.txt"));
        var assetService = new NullAssetService(runtimeInfo, windowService, provider);

        var result = assetService.GetMesh(new SceneMeshRef("mesh://triangle"));

        Assert.True(result.IsSuccess);
        var asset = Assert.IsType<MeshAssetData>(result.Asset);
        Assert.Equal(3, asset.Vertices.Count);
        Assert.Equal(new[] { 0, 1, 2 }, asset.Indices);
    }

    [Fact]
    public void DiskMeshAssetProvider_LoadsMeshThroughCatalogMapping()
    {
        using var assets = TestAssetWorkspace.Create(
            ("catalog.txt", "mesh://triangle=meshes/triangle.obj"),
            ("meshes/triangle.obj", CreateTriangleObj()));
        var provider = new DiskMeshAssetProvider(assets.GetPath("catalog.txt"));

        var result = provider.GetMesh(new SceneMeshRef("mesh://triangle"));

        Assert.True(result.IsSuccess);
        var asset = Assert.IsType<MeshAssetData>(result.Asset);
        Assert.Equal(3, asset.Vertices.Count);
        Assert.Equal(3, asset.Indices.Count);
        Assert.Equal(Vector3.Zero, asset.Vertices[0].Position);
        Assert.Equal(Vector3.UnitZ, asset.Vertices[0].Normal);
        Assert.Equal(Vector2.Zero, asset.Vertices[0].TexCoord);
    }

    [Fact]
    public void DiskMeshAssetProvider_LoadsMesh_WithPositionOnlyFaceFormat()
    {
        using var assets = TestAssetWorkspace.Create(
            ("catalog.txt", "mesh://triangle=triangle.obj"),
            ("triangle.obj", """
                             v 0 0 0
                             v 1 0 0
                             v 0 1 0
                             f 1 2 3
                             """));
        var provider = new DiskMeshAssetProvider(assets.GetPath("catalog.txt"));

        var result = provider.GetMesh(new SceneMeshRef("mesh://triangle"));

        Assert.True(result.IsSuccess);
        var asset = Assert.IsType<MeshAssetData>(result.Asset);
        Assert.All(asset.Vertices, vertex =>
        {
            Assert.Equal(Vector2.Zero, vertex.TexCoord);
            Assert.Equal(Vector3.Zero, vertex.Normal);
        });
    }

    [Fact]
    public void DiskMeshAssetProvider_LoadsMesh_WithPositionAndTexCoordFaceFormat()
    {
        using var assets = TestAssetWorkspace.Create(
            ("catalog.txt", "mesh://triangle=triangle.obj"),
            ("triangle.obj", """
                             v 0 0 0
                             v 1 0 0
                             v 0 1 0
                             vt 0 0
                             vt 1 0
                             vt 0 1
                             f 1/1 2/2 3/3
                             """));
        var provider = new DiskMeshAssetProvider(assets.GetPath("catalog.txt"));

        var result = provider.GetMesh(new SceneMeshRef("mesh://triangle"));

        Assert.True(result.IsSuccess);
        var asset = Assert.IsType<MeshAssetData>(result.Asset);
        Assert.Equal(new Vector2(1f, 0f), asset.Vertices[1].TexCoord);
        Assert.All(asset.Vertices, vertex => Assert.Equal(Vector3.Zero, vertex.Normal));
    }

    [Fact]
    public void DiskMeshAssetProvider_LoadsMesh_WithPositionAndNormalFaceFormat()
    {
        using var assets = TestAssetWorkspace.Create(
            ("catalog.txt", "mesh://triangle=triangle.obj"),
            ("triangle.obj", """
                             v 0 0 0
                             v 1 0 0
                             v 0 1 0
                             vn 0 0 1
                             f 1//1 2//1 3//1
                             """));
        var provider = new DiskMeshAssetProvider(assets.GetPath("catalog.txt"));

        var result = provider.GetMesh(new SceneMeshRef("mesh://triangle"));

        Assert.True(result.IsSuccess);
        var asset = Assert.IsType<MeshAssetData>(result.Asset);
        Assert.All(asset.Vertices, vertex =>
        {
            Assert.Equal(Vector3.UnitZ, vertex.Normal);
            Assert.Equal(Vector2.Zero, vertex.TexCoord);
        });
    }

    [Fact]
    public void DiskMeshAssetProvider_LoadsMesh_WithNegativeIndices()
    {
        using var assets = TestAssetWorkspace.Create(
            ("catalog.txt", "mesh://triangle=triangle.obj"),
            ("triangle.obj", """
                             v 0 0 0
                             v 1 0 0
                             v 0 1 0
                             vt 0 0
                             vt 1 0
                             vt 0 1
                             vn 0 0 1
                             f -3/-3/-1 -2/-2/-1 -1/-1/-1
                             """));
        var provider = new DiskMeshAssetProvider(assets.GetPath("catalog.txt"));

        var result = provider.GetMesh(new SceneMeshRef("mesh://triangle"));

        Assert.True(result.IsSuccess);
        var asset = Assert.IsType<MeshAssetData>(result.Asset);
        Assert.Equal(3, asset.Vertices.Count);
        Assert.Equal(new[] { 0, 1, 2 }, asset.Indices);
    }

    [Fact]
    public void DiskMeshAssetProvider_MissingCatalogEntry_ReturnsNotFoundFailure()
    {
        using var assets = TestAssetWorkspace.Create(("catalog.txt", "mesh://triangle=triangle.obj"));
        var provider = new DiskMeshAssetProvider(assets.GetPath("catalog.txt"));

        var result = provider.GetMesh(new SceneMeshRef("mesh://missing"));

        Assert.False(result.IsSuccess);
        Assert.Equal(MeshAssetLoadFailureKind.NotFound, result.Failure!.Kind);
    }

    [Fact]
    public void DiskMeshAssetProvider_InvalidObj_ReturnsInvalidDataFailure()
    {
        using var assets = TestAssetWorkspace.Create(
            ("catalog.txt", "mesh://broken=broken.obj"),
            ("broken.obj", "v 0 0 0\nf 1/1/1 1/1/1 1/1/1\n"));
        var provider = new DiskMeshAssetProvider(assets.GetPath("catalog.txt"));

        var result = provider.GetMesh(new SceneMeshRef("mesh://broken"));

        Assert.False(result.IsSuccess);
        Assert.Equal(MeshAssetLoadFailureKind.InvalidData, result.Failure!.Kind);
    }

    [Fact]
    public void DiskMeshAssetProvider_RepeatedRequests_UseCachedMeshResult()
    {
        using var assets = TestAssetWorkspace.Create(
            ("catalog.txt", "mesh://triangle=triangle.obj"),
            ("triangle.obj", CreateTriangleObj()));
        var provider = new DiskMeshAssetProvider(assets.GetPath("catalog.txt"));

        var firstResult = provider.GetMesh(new SceneMeshRef("mesh://triangle"));
        File.WriteAllText(assets.GetPath("triangle.obj"), "invalid");
        var secondResult = provider.GetMesh(new SceneMeshRef("mesh://triangle"));

        Assert.True(firstResult.IsSuccess);
        Assert.True(secondResult.IsSuccess);
        Assert.Same(firstResult.Asset, secondResult.Asset);
    }

    [Fact]
    public void DiskMeshAssetProvider_HeadlessPath_LoadsRealDiskMeshAndLeavesWindowUsable()
    {
        using var assets = TestAssetWorkspace.Create(
            ("catalog.txt", "mesh://triangle=triangle.obj"),
            ("triangle.obj", CreateTriangleObj()));
        using var windowService = new NullWindowService(new WindowConfig(1280, 720, "AnsEngine"), useNativeWindow: false);
        var runtimeInfo = new EngineRuntimeInfo("AnsEngine", "0.1.0");
        var provider = new DiskMeshAssetProvider(assets.GetPath("catalog.txt"));
        var assetService = new NullAssetService(runtimeInfo, windowService, provider);

        var result = assetService.GetMesh(new SceneMeshRef("mesh://triangle"));
        var handle = assetService.Load("asset://bootstrap");

        Assert.True(result.IsSuccess);
        Assert.True(windowService.Exists);
        Assert.False(windowService.IsCloseRequested);
        Assert.True(handle.IsValid);
    }

    [Fact]
    public void DiskMeshAssetProvider_UnsupportedExtension_ReturnsUnsupportedFormatFailure()
    {
        using var assets = TestAssetWorkspace.Create(
            ("catalog.txt", "mesh://triangle=triangle.mesh"),
            ("triangle.mesh", "not-an-obj"));
        var provider = new DiskMeshAssetProvider(assets.GetPath("catalog.txt"));

        var result = provider.GetMesh(new SceneMeshRef("mesh://triangle"));

        Assert.False(result.IsSuccess);
        Assert.Equal(MeshAssetLoadFailureKind.UnsupportedFormat, result.Failure!.Kind);
    }

    private static string CreateTriangleObj()
    {
        return """
               v 0 0 0
               v 1 0 0
               v 0 1 0
               vt 0 0
               vt 1 0
               vt 0 1
               vn 0 0 1
               f 1/1/1 2/2/1 3/3/1
               """;
    }

    private sealed class TestAssetWorkspace : IDisposable
    {
        private readonly string mRootPath;

        private TestAssetWorkspace(string rootPath)
        {
            mRootPath = rootPath;
        }

        public static TestAssetWorkspace Create(params (string RelativePath, string Content)[] files)
        {
            var rootPath = Path.Combine(Path.GetTempPath(), "AnsEngine.Asset.Tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(rootPath);
            foreach (var (relativePath, content) in files)
            {
                var fullPath = Path.Combine(rootPath, relativePath);
                var directoryPath = Path.GetDirectoryName(fullPath);
                if (!string.IsNullOrWhiteSpace(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                File.WriteAllText(fullPath, content);
            }

            return new TestAssetWorkspace(rootPath);
        }

        public string GetPath(string relativePath)
        {
            return Path.Combine(mRootPath, relativePath);
        }

        public void Dispose()
        {
            if (Directory.Exists(mRootPath))
            {
                Directory.Delete(mRootPath, recursive: true);
            }
        }
    }
}
