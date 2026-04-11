using Engine.Scene;

namespace Engine.Render;

public readonly record struct SceneRenderVertex(
    float X,
    float Y,
    float Z,
    float R,
    float G,
    float B);

public sealed record SceneRenderSubmission(IReadOnlyList<SceneRenderVertex> Vertices);

public static class SceneRenderSubmissionBuilder
{
    private const float TriangleHalfWidth = 0.22f;
    private const float TriangleHalfHeight = 0.20f;
    private const float BaseY = -0.15f;

    public static SceneRenderSubmission Build(SceneRenderFrame frame)
    {
        if (frame.Items.Count == 0)
        {
            return new SceneRenderSubmission(Array.Empty<SceneRenderVertex>());
        }

        var vertices = new List<SceneRenderVertex>(frame.Items.Count * 3);
        for (var index = 0; index < frame.Items.Count; index += 1)
        {
            var item = frame.Items[index];
            var centerX = -0.65f + (index * 0.6f);
            var materialHash = Math.Abs(item.MaterialId.GetHashCode(StringComparison.Ordinal));
            var red = 0.45f + (materialHash % 40) / 100f;
            var green = 0.55f + (materialHash % 30) / 120f;
            var blue = 0.65f + (materialHash % 20) / 140f;

            vertices.Add(new SceneRenderVertex(centerX, BaseY + TriangleHalfHeight, 0f, red, green, blue));
            vertices.Add(new SceneRenderVertex(centerX - TriangleHalfWidth, BaseY - TriangleHalfHeight, 0f, red, green, blue));
            vertices.Add(new SceneRenderVertex(centerX + TriangleHalfWidth, BaseY - TriangleHalfHeight, 0f, red, green, blue));
        }

        return new SceneRenderSubmission(vertices);
    }
}

internal sealed class DefaultSceneRenderContractProvider : ISceneRenderContractProvider
{
    private int _frameNumber;

    public SceneRenderFrame BuildRenderFrame()
    {
        var items = new[]
        {
            new SceneRenderItem(1, "mesh://triangle", "material://default")
        };
        var frame = new SceneRenderFrame(_frameNumber, items);
        _frameNumber += 1;
        return frame;
    }
}
