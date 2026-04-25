using Engine.Contracts;
using System.Numerics;
using Xunit;

namespace Engine.Contracts.Tests;

public sealed class MeshAssetContractsTests
{
    [Fact]
    public void MeshAssetVertex_StoresPositionNormalAndTexCoord()
    {
        var vertex = new MeshAssetVertex(
            new Vector3(1.0f, 2.0f, 3.0f),
            Vector3.UnitZ,
            new Vector2(0.25f, 0.75f));

        Assert.Equal(new Vector3(1.0f, 2.0f, 3.0f), vertex.Position);
        Assert.Equal(Vector3.UnitZ, vertex.Normal);
        Assert.Equal(new Vector2(0.25f, 0.75f), vertex.TexCoord);
    }

    [Fact]
    public void MeshAssetData_StoresNormalizedMeshPayload()
    {
        var vertices = new[]
        {
            new MeshAssetVertex(Vector3.Zero, Vector3.UnitY, Vector2.Zero),
            new MeshAssetVertex(Vector3.UnitX, Vector3.UnitY, Vector2.UnitX)
        };
        var indices = new[] { 0, 1, 0 };

        var asset = new MeshAssetData(vertices, indices);

        Assert.Equal(vertices, asset.Vertices);
        Assert.Equal(indices, asset.Indices);
    }

    [Fact]
    public void MeshAssetData_NullVertices_ThrowsArgumentNullException()
    {
        _ = Assert.Throws<ArgumentNullException>(() => new MeshAssetData(null!, Array.Empty<int>()));
    }

    [Fact]
    public void MeshAssetData_NullIndices_ThrowsArgumentNullException()
    {
        _ = Assert.Throws<ArgumentNullException>(() => new MeshAssetData(Array.Empty<MeshAssetVertex>(), null!));
    }

    [Fact]
    public void MeshAssetLoadFailure_NoneKind_ThrowsArgumentOutOfRangeException()
    {
        _ = Assert.Throws<ArgumentOutOfRangeException>(() => new MeshAssetLoadFailure(MeshAssetLoadFailureKind.None, "missing"));
    }

    [Fact]
    public void MeshAssetLoadFailure_EmptyMessage_ThrowsArgumentException()
    {
        _ = Assert.Throws<ArgumentException>(() => new MeshAssetLoadFailure(MeshAssetLoadFailureKind.NotFound, " "));
    }

    [Fact]
    public void MeshAssetLoadResult_Success_PreservesMeshAndAsset()
    {
        var mesh = new SceneMeshRef("mesh://sample");
        var asset = new MeshAssetData(
            new[]
            {
                new MeshAssetVertex(Vector3.Zero, Vector3.UnitZ, Vector2.Zero)
            },
            new[] { 0 });

        var result = MeshAssetLoadResult.Success(mesh, asset);

        Assert.True(result.IsSuccess);
        Assert.Equal(mesh, result.Mesh);
        Assert.Same(asset, result.Asset);
        Assert.Null(result.Failure);
    }

    [Fact]
    public void MeshAssetLoadResult_Failure_PreservesFailureSemantics()
    {
        var mesh = new SceneMeshRef("mesh://missing");
        var failure = new MeshAssetLoadFailure(MeshAssetLoadFailureKind.NotFound, "Mesh asset was not found.");

        var result = MeshAssetLoadResult.FailureResult(mesh, failure);

        Assert.False(result.IsSuccess);
        Assert.Equal(mesh, result.Mesh);
        Assert.Null(result.Asset);
        Assert.Same(failure, result.Failure);
    }

    [Fact]
    public void MeshAssetProvider_ReturnsExplicitFailureResult()
    {
        IMeshAssetProvider provider = new StubMeshAssetProvider();

        var result = provider.GetMesh(new SceneMeshRef("mesh://missing"));

        Assert.False(result.IsSuccess);
        Assert.Equal(MeshAssetLoadFailureKind.NotFound, result.Failure!.Kind);
    }

    private sealed class StubMeshAssetProvider : IMeshAssetProvider
    {
        public MeshAssetLoadResult GetMesh(SceneMeshRef mesh)
        {
            return MeshAssetLoadResult.FailureResult(
                mesh,
                new MeshAssetLoadFailure(MeshAssetLoadFailureKind.NotFound, "Mesh asset was not found."));
        }
    }
}
