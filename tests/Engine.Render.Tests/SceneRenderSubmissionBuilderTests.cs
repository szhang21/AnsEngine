using Engine.Contracts;
using Engine.Render;
using System.Numerics;
using Xunit;

namespace Engine.Render.Tests;

public sealed class SceneRenderSubmissionBuilderTests
{
    [Fact]
    public void Build_EmptyFrame_ReturnsNoVertices()
    {
        var frame = new SceneRenderFrame(0, Array.Empty<SceneRenderItem>());

        var submission = SceneRenderSubmissionBuilder.Build(frame);

        Assert.Empty(submission.Batches);
    }

    [Fact]
    public void Build_SingleItem_ReturnsSingleBatchWithTriangleVertices()
    {
        var frame = new SceneRenderFrame(
            1,
            new[]
            {
                new SceneRenderItem(1, "mesh://triangle", "material://default")
            });

        var submission = SceneRenderSubmissionBuilder.Build(frame);
        var batch = Assert.Single(submission.Batches);

        Assert.Equal(3, batch.Vertices.Count);
        Assert.NotEqual(batch.Vertices[0].X, batch.Vertices[1].X);
        Assert.NotEqual(batch.Vertices[1].X, batch.Vertices[2].X);
    }

    [Fact]
    public void Build_IdentityTransform_MatchesLegacyTriangleLayout()
    {
        var frame = new SceneRenderFrame(
            3,
            new[]
            {
                new SceneRenderItem(
                    7,
                    "mesh://triangle",
                    "material://default",
                    SceneTransform.Identity)
            });

        var submission = SceneRenderSubmissionBuilder.Build(frame);
        var batch = Assert.Single(submission.Batches);

        Assert.Equal(3, batch.Vertices.Count);
        var top = TransformToClip(batch.Vertices[0], batch.ModelViewProjection);
        var left = TransformToClip(batch.Vertices[1], batch.ModelViewProjection);
        var right = TransformToClip(batch.Vertices[2], batch.ModelViewProjection);

        AssertClose(0f, top.X);
        AssertClose(0.20f, top.Y);
        AssertClose(0f, top.Z);

        AssertClose(-0.22f, left.X);
        AssertClose(-0.20f, left.Y);
        AssertClose(0f, left.Z);

        AssertClose(0.22f, right.X);
        AssertClose(-0.20f, right.Y);
        AssertClose(0f, right.Z);
    }

    [Fact]
    public void Build_RotationTransform_AffectsTriangleVertices()
    {
        var rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, MathF.PI / 2f);
        var frame = new SceneRenderFrame(
            4,
            new[]
            {
                new SceneRenderItem(
                    9,
                    "mesh://triangle",
                    "material://default",
                    new SceneTransform(Vector3.Zero, Vector3.One, rotation))
            });

        var submission = SceneRenderSubmissionBuilder.Build(frame);
        var batch = Assert.Single(submission.Batches);
        var top = TransformToClip(batch.Vertices[0], batch.ModelViewProjection);
        var left = TransformToClip(batch.Vertices[1], batch.ModelViewProjection);
        var right = TransformToClip(batch.Vertices[2], batch.ModelViewProjection);
        var expectedTop = Vector3.Transform(new Vector3(0f, 0.20f, 0f), rotation);
        var expectedLeft = Vector3.Transform(new Vector3(-0.22f, -0.20f, 0f), rotation);
        var expectedRight = Vector3.Transform(new Vector3(0.22f, -0.20f, 0f), rotation);

        Assert.Equal(3, batch.Vertices.Count);
        AssertClose(expectedTop.X, top.X);
        AssertClose(expectedTop.Y, top.Y);

        AssertClose(expectedLeft.X, left.X);
        AssertClose(expectedLeft.Y, left.Y);

        AssertClose(expectedRight.X, right.X);
        AssertClose(expectedRight.Y, right.Y);
    }

    [Fact]
    public void Build_MultipleItems_ProducesMultipleTriangles()
    {
        var frame = new SceneRenderFrame(
            2,
            new[]
            {
                new SceneRenderItem(
                    1,
                    "mesh://triangle",
                    "material://default",
                    new SceneTransform(new Vector3(-0.30f, 0.0f, 0.0f), Vector3.One, Quaternion.Identity)),
                new SceneRenderItem(
                    2,
                    "mesh://triangle",
                    "material://highlight",
                    new SceneTransform(new Vector3(0.30f, 0.0f, 0.0f), Vector3.One, Quaternion.Identity))
            });

        var submission = SceneRenderSubmissionBuilder.Build(frame);
        var firstBatch = submission.Batches[0];
        var secondBatch = submission.Batches[1];
        var firstTop = TransformToClip(firstBatch.Vertices[0], firstBatch.ModelViewProjection);
        var secondTop = TransformToClip(secondBatch.Vertices[0], secondBatch.ModelViewProjection);

        Assert.Equal(2, submission.Batches.Count);
        Assert.Equal(3, firstBatch.Vertices.Count);
        Assert.Equal(3, secondBatch.Vertices.Count);
        Assert.True(secondTop.X > firstTop.X);
    }

    [Fact]
    public void Build_KnownMaterialId_UsesResolvedMaterialColor()
    {
        var frame = new SceneRenderFrame(
            6,
            new[]
            {
                new SceneRenderItem(1, "mesh://triangle", "material://pulse")
            });

        var submission = SceneRenderSubmissionBuilder.Build(frame);
        var batch = Assert.Single(submission.Batches);
        var vertex = batch.Vertices[0];

        AssertClose(0.98f, vertex.R);
        AssertClose(0.54f, vertex.G);
        AssertClose(0.32f, vertex.B);
    }

    [Fact]
    public void Build_UnknownMeshId_FallsBackToTriangleMesh()
    {
        var frame = new SceneRenderFrame(
            7,
            new[]
            {
                new SceneRenderItem(1, "mesh://missing", "material://default")
            });

        var submission = SceneRenderSubmissionBuilder.Build(frame);
        var batch = Assert.Single(submission.Batches);
        var top = TransformToClip(batch.Vertices[0], batch.ModelViewProjection);
        var left = TransformToClip(batch.Vertices[1], batch.ModelViewProjection);
        var right = TransformToClip(batch.Vertices[2], batch.ModelViewProjection);

        Assert.Equal(3, batch.Vertices.Count);
        AssertClose(0f, top.X);
        AssertClose(0.20f, top.Y);
        AssertClose(-0.22f, left.X);
        AssertClose(-0.20f, left.Y);
        AssertClose(0.22f, right.X);
        AssertClose(-0.20f, right.Y);
    }

    [Fact]
    public void Build_WithMeshAssetProvider_UsesResolvedMeshGeometry()
    {
        var provider = new CountingMeshAssetProvider(
            MeshAssetLoadResult.Success(
                new SceneMeshRef("mesh://quad"),
                new MeshAssetData(
                    new[]
                    {
                        new MeshAssetVertex(new Vector3(-0.4f, 0.3f, 0f), Vector3.UnitZ, Vector2.Zero),
                        new MeshAssetVertex(new Vector3(-0.2f, 0.1f, 0f), Vector3.UnitZ, Vector2.Zero),
                        new MeshAssetVertex(new Vector3(0.2f, 0.1f, 0f), Vector3.UnitZ, Vector2.Zero)
                    },
                    new[] { 0, 1, 2 })));
        var frame = new SceneRenderFrame(
            9,
            new[]
            {
                new SceneRenderItem(1, "mesh://quad", "material://default")
            });

        var submission = SceneRenderSubmissionBuilder.Build(frame, provider);
        var batch = Assert.Single(submission.Batches);

        Assert.Equal(1, provider.CallCount);
        Assert.Equal("mesh://quad", batch.MeshCacheKey);
        Assert.Equal(3, batch.MeshVertices.Count);
        AssertClose(-0.4f, batch.MeshVertices[0].X);
        AssertClose(0.3f, batch.MeshVertices[0].Y);
        AssertClose(0f, batch.MeshVertices[0].NormalX);
        AssertClose(0f, batch.MeshVertices[0].NormalY);
        AssertClose(1f, batch.MeshVertices[0].NormalZ);
    }

    [Fact]
    public void Build_MeshProviderFailure_UsesFallbackGeometry()
    {
        var provider = new CountingMeshAssetProvider(
            MeshAssetLoadResult.FailureResult(
                new SceneMeshRef("mesh://missing"),
                new MeshAssetLoadFailure(MeshAssetLoadFailureKind.NotFound, "Missing mesh.")));
        var frame = new SceneRenderFrame(
            10,
            new[]
            {
                new SceneRenderItem(1, "mesh://missing", "material://default")
            });

        var submission = SceneRenderSubmissionBuilder.Build(frame, provider);
        var batch = Assert.Single(submission.Batches);
        var top = TransformToClip(batch.Vertices[0], batch.ModelViewProjection);

        Assert.Equal(1, provider.CallCount);
        Assert.Equal("fallback://triangle", batch.MeshCacheKey);
        Assert.Equal(3, batch.MeshVertices.Count);
        AssertClose(0f, batch.MeshVertices[0].NormalX);
        AssertClose(0f, batch.MeshVertices[0].NormalY);
        AssertClose(1f, batch.MeshVertices[0].NormalZ);
        AssertClose(0f, top.X);
        AssertClose(0.20f, top.Y);
    }

    [Fact]
    public void Build_SharedMeshProviderResult_IsCachedAcrossBatches()
    {
        var provider = new CountingMeshAssetProvider(
            MeshAssetLoadResult.Success(
                new SceneMeshRef("mesh://shared"),
                new MeshAssetData(
                    new[]
                    {
                        new MeshAssetVertex(new Vector3(0f, 0.2f, 0f), Vector3.UnitZ, Vector2.Zero),
                        new MeshAssetVertex(new Vector3(-0.2f, -0.2f, 0f), Vector3.UnitZ, Vector2.Zero),
                        new MeshAssetVertex(new Vector3(0.2f, -0.2f, 0f), Vector3.UnitZ, Vector2.Zero)
                    },
                    new[] { 0, 1, 2 })));
        var frame = new SceneRenderFrame(
            11,
            new[]
            {
                new SceneRenderItem(1, "mesh://shared", "material://default"),
                new SceneRenderItem(2, "mesh://shared", "material://highlight")
            });

        var submission = SceneRenderSubmissionBuilder.Build(frame, provider);

        Assert.Equal(1, provider.CallCount);
        Assert.Equal(2, submission.Batches.Count);
        Assert.Equal("mesh://shared", submission.Batches[0].MeshCacheKey);
        Assert.Equal(submission.Batches[0].MeshCacheKey, submission.Batches[1].MeshCacheKey);
        AssertClose(0.72f, submission.Batches[0].Vertices[0].R);
        AssertClose(0.42f, submission.Batches[1].Vertices[0].R);
    }

    [Fact]
    public void Build_UnknownMaterialId_FallsBackToDefaultMaterialColor()
    {
        var frame = new SceneRenderFrame(
            8,
            new[]
            {
                new SceneRenderItem(1, "mesh://triangle", "material://missing")
            });

        var submission = SceneRenderSubmissionBuilder.Build(frame);
        var batch = Assert.Single(submission.Batches);
        var vertex = batch.Vertices[0];

        AssertClose(0.72f, vertex.R);
        AssertClose(0.78f, vertex.G);
        AssertClose(0.86f, vertex.B);
    }

    [Fact]
    public void Build_CameraViewProjection_AffectsClipCoordinates()
    {
        var camera = new SceneCamera(
            Matrix4x4.CreateLookAt(new Vector3(0.2f, 0.0f, 2.0f), Vector3.Zero, Vector3.UnitY),
            Matrix4x4.CreatePerspectiveFieldOfView(1.0471976f, 16f / 9f, 0.1f, 10f));
        var frame = new SceneRenderFrame(
            5,
            new[]
            {
                new SceneRenderItem(1, "mesh://triangle", "material://default", SceneTransform.Identity)
            },
            camera);

        var submission = SceneRenderSubmissionBuilder.Build(frame);
        var batch = Assert.Single(submission.Batches);
        var top = TransformToClip(batch.Vertices[0], batch.ModelViewProjection);

        Assert.InRange(top.X, -1f, 1f);
        Assert.InRange(top.Y, -1f, 1f);
        Assert.InRange(top.Z, -1f, 1f);
    }

    [Fact]
    public void FlattenModelViewProjection_UploadsRowVectorMatrixForColumnVectorShader()
    {
        var matrix =
            Matrix4x4.CreateScale(new Vector3(1.5f, 0.5f, 2.0f))
            * Matrix4x4.CreateFromYawPitchRoll(0.3f, 0.2f, 0.1f)
            * Matrix4x4.CreateTranslation(new Vector3(0.4f, -0.2f, 1.1f))
            * Matrix4x4.CreateLookAt(new Vector3(1.4f, 0.9f, 3.2f), Vector3.Zero, Vector3.UnitY)
            * Matrix4x4.CreatePerspectiveFieldOfView(0.7853982f, 16f / 9f, 0.1f, 10f);
        var vertex = new Vector4(0.25f, -0.5f, 0.75f, 1.0f);

        var expected = Vector4.Transform(vertex, matrix);
        var flattened = NullRenderer.FlattenModelViewProjection(matrix);
        var actual = TransformAsOpenGlShaderColumnVector(
            flattened,
            NullRenderer.ShouldTransposeModelViewProjectionUniform,
            vertex);

        Assert.False(NullRenderer.ShouldTransposeModelViewProjectionUniform);
        AssertClose(expected.X, actual.X);
        AssertClose(expected.Y, actual.Y);
        AssertClose(expected.Z, actual.Z);
        AssertClose(expected.W, actual.W);
    }

    private static void AssertClose(float expected, float actual)
    {
        Assert.InRange(actual, expected - 0.0001f, expected + 0.0001f);
    }

    private static Vector3 TransformToClip(SceneRenderVertex vertex, Matrix4x4 modelViewProjection)
    {
        var vector = new Vector4(vertex.X, vertex.Y, vertex.Z, 1f);
        var transformed = Vector4.Transform(vector, modelViewProjection);
        var w = MathF.Abs(transformed.W) > 0.00001f ? transformed.W : 1f;
        return new Vector3(transformed.X / w, transformed.Y / w, transformed.Z / w);
    }

    private static Vector4 TransformAsOpenGlShaderColumnVector(
        IReadOnlyList<float> flattened,
        bool transpose,
        Vector4 vector)
    {
        var matrix = transpose
            ? new Matrix4x4(
                flattened[0], flattened[1], flattened[2], flattened[3],
                flattened[4], flattened[5], flattened[6], flattened[7],
                flattened[8], flattened[9], flattened[10], flattened[11],
                flattened[12], flattened[13], flattened[14], flattened[15])
            : new Matrix4x4(
                flattened[0], flattened[4], flattened[8], flattened[12],
                flattened[1], flattened[5], flattened[9], flattened[13],
                flattened[2], flattened[6], flattened[10], flattened[14],
                flattened[3], flattened[7], flattened[11], flattened[15]);

        return new Vector4(
            (matrix.M11 * vector.X) + (matrix.M12 * vector.Y) + (matrix.M13 * vector.Z) + (matrix.M14 * vector.W),
            (matrix.M21 * vector.X) + (matrix.M22 * vector.Y) + (matrix.M23 * vector.Z) + (matrix.M24 * vector.W),
            (matrix.M31 * vector.X) + (matrix.M32 * vector.Y) + (matrix.M33 * vector.Z) + (matrix.M34 * vector.W),
            (matrix.M41 * vector.X) + (matrix.M42 * vector.Y) + (matrix.M43 * vector.Z) + (matrix.M44 * vector.W));
    }

    private sealed class CountingMeshAssetProvider : IMeshAssetProvider
    {
        private readonly MeshAssetLoadResult result;

        public CountingMeshAssetProvider(MeshAssetLoadResult result)
        {
            this.result = result;
        }

        public int CallCount { get; private set; }

        public MeshAssetLoadResult GetMesh(SceneMeshRef mesh)
        {
            CallCount += 1;
            return result;
        }
    }
}
