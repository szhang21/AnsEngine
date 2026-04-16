using Engine.Contracts;
using System.Numerics;

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

            var top = new Vector3(centerX, BaseY + TriangleHalfHeight, 0f);
            var left = new Vector3(centerX - TriangleHalfWidth, BaseY - TriangleHalfHeight, 0f);
            var right = new Vector3(centerX + TriangleHalfWidth, BaseY - TriangleHalfHeight, 0f);

            top = ApplyTransform(top, item.Transform);
            left = ApplyTransform(left, item.Transform);
            right = ApplyTransform(right, item.Transform);

            vertices.Add(new SceneRenderVertex(top.X, top.Y, top.Z, red, green, blue));
            vertices.Add(new SceneRenderVertex(left.X, left.Y, left.Z, red, green, blue));
            vertices.Add(new SceneRenderVertex(right.X, right.Y, right.Z, red, green, blue));
        }

        return new SceneRenderSubmission(vertices);
    }

    private static Vector3 ApplyTransform(Vector3 vertex, SceneTransform transform)
    {
        if (transform.Equals(SceneTransform.Identity))
        {
            return vertex;
        }

        var scaled = vertex * transform.Scale;
        var rotated = Vector3.Transform(scaled, transform.Rotation);
        return rotated + transform.Position;
    }
}
