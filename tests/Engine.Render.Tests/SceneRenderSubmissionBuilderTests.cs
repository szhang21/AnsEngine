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

        AssertClose(-0.65f, top.X);
        AssertClose(0.05f, top.Y);
        AssertClose(0f, top.Z);

        AssertClose(-0.87f, left.X);
        AssertClose(-0.35f, left.Y);
        AssertClose(0f, left.Z);

        AssertClose(-0.43f, right.X);
        AssertClose(-0.35f, right.Y);
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

        Assert.Equal(3, batch.Vertices.Count);
        AssertClose(-0.05f, top.X);
        AssertClose(-0.65f, top.Y);

        AssertClose(0.35f, left.X);
        AssertClose(-0.87f, left.Y);

        AssertClose(0.35f, right.X);
        AssertClose(-0.43f, right.Y);
    }

    [Fact]
    public void Build_MultipleItems_ProducesMultipleTriangles()
    {
        var frame = new SceneRenderFrame(
            2,
            new[]
            {
                new SceneRenderItem(1, "mesh://triangle", "material://default"),
                new SceneRenderItem(2, "mesh://triangle", "material://highlight")
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
}
