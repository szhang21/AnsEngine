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

        Assert.Empty(submission.Vertices);
    }

    [Fact]
    public void Build_SingleItem_ReturnsTriangleSubmission()
    {
        var frame = new SceneRenderFrame(
            1,
            new[]
            {
                new SceneRenderItem(1, "mesh://triangle", "material://default")
            });

        var submission = SceneRenderSubmissionBuilder.Build(frame);

        Assert.Equal(3, submission.Vertices.Count);
        Assert.NotEqual(submission.Vertices[0].X, submission.Vertices[1].X);
        Assert.NotEqual(submission.Vertices[1].X, submission.Vertices[2].X);
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

        Assert.Equal(3, submission.Vertices.Count);
        AssertClose(-0.65f, submission.Vertices[0].X);
        AssertClose(0.05f, submission.Vertices[0].Y);
        AssertClose(0f, submission.Vertices[0].Z);

        AssertClose(-0.87f, submission.Vertices[1].X);
        AssertClose(-0.35f, submission.Vertices[1].Y);
        AssertClose(0f, submission.Vertices[1].Z);

        AssertClose(-0.43f, submission.Vertices[2].X);
        AssertClose(-0.35f, submission.Vertices[2].Y);
        AssertClose(0f, submission.Vertices[2].Z);
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

        Assert.Equal(3, submission.Vertices.Count);
        AssertClose(-0.05f, submission.Vertices[0].X);
        AssertClose(-0.65f, submission.Vertices[0].Y);

        AssertClose(0.35f, submission.Vertices[1].X);
        AssertClose(-0.87f, submission.Vertices[1].Y);

        AssertClose(0.35f, submission.Vertices[2].X);
        AssertClose(-0.43f, submission.Vertices[2].Y);
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

        Assert.Equal(6, submission.Vertices.Count);
        Assert.True(submission.Vertices[3].X > submission.Vertices[0].X);
    }

    private static void AssertClose(float expected, float actual)
    {
        Assert.InRange(actual, expected - 0.0001f, expected + 0.0001f);
    }
}
