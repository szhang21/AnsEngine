using Engine.Scene;
using Engine.Render;
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
}
