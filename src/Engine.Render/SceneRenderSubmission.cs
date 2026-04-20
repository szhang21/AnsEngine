using Engine.Contracts;
using System.Numerics;

namespace Engine.Render;

public readonly record struct SceneRenderMeshVertex(
    float X,
    float Y,
    float Z);

public readonly record struct SceneRenderVertex(
    float X,
    float Y,
    float Z,
    float R,
    float G,
    float B);

public readonly record struct SceneRenderMaterialParameters(
    float Red,
    float Green,
    float Blue);

public sealed record SceneRenderBatch(
    IReadOnlyList<SceneRenderVertex> Vertices,
    Matrix4x4 ModelViewProjection);

public sealed record SceneRenderSubmission(IReadOnlyList<SceneRenderBatch> Batches);

public static class SceneRenderSubmissionBuilder
{
    private const float kTriangleHalfWidth = 0.22f;
    private const float kTriangleHalfHeight = 0.20f;
    private const float kBaseY = -0.15f;
    private const string kTriangleMeshId = "mesh://triangle";
    private const string kDefaultMaterialId = "material://default";
    private static readonly SceneRenderMaterialParameters sDefaultMaterial =
        new(0.72f, 0.78f, 0.86f);
    private static readonly IReadOnlyDictionary<string, SceneRenderMeshVertex[]> sMeshes =
        new Dictionary<string, SceneRenderMeshVertex[]>(StringComparer.Ordinal)
        {
            [kTriangleMeshId] =
            [
                new SceneRenderMeshVertex(0f, kTriangleHalfHeight, 0f),
                new SceneRenderMeshVertex(-kTriangleHalfWidth, -kTriangleHalfHeight, 0f),
                new SceneRenderMeshVertex(kTriangleHalfWidth, -kTriangleHalfHeight, 0f)
            ]
        };
    private static readonly IReadOnlyDictionary<string, SceneRenderMaterialParameters> sMaterials =
        new Dictionary<string, SceneRenderMaterialParameters>(StringComparer.Ordinal)
        {
            [kDefaultMaterialId] = sDefaultMaterial,
            ["material://pulse"] = new SceneRenderMaterialParameters(0.98f, 0.54f, 0.32f),
            ["material://highlight"] = new SceneRenderMaterialParameters(0.42f, 0.85f, 0.58f)
        };

    public static SceneRenderSubmission Build(SceneRenderFrame frame)
    {
        if (frame.Items.Count == 0)
        {
            return new SceneRenderSubmission(Array.Empty<SceneRenderBatch>());
        }

        var batches = new List<SceneRenderBatch>(frame.Items.Count);
        for (var index = 0; index < frame.Items.Count; index += 1)
        {
            var item = frame.Items[index];
            var material = ResolveMaterial(item.MaterialId);
            var meshVertices = ResolveMesh(item.MeshId);
            var vertices = new List<SceneRenderVertex>(meshVertices.Length);
            for (var vertexIndex = 0; vertexIndex < meshVertices.Length; vertexIndex += 1)
            {
                var meshVertex = meshVertices[vertexIndex];
                vertices.Add(new SceneRenderVertex(meshVertex.X, meshVertex.Y, meshVertex.Z, material.Red, material.Green, material.Blue));
            }

            var centerX = -0.65f + (index * 0.6f);
            var layoutMatrix = Matrix4x4.CreateTranslation(centerX, kBaseY, 0f);
            var modelMatrix = layoutMatrix * BuildTransformMatrix(item.Transform);
            var modelViewProjection = modelMatrix * frame.Camera.View * frame.Camera.Projection;
            batches.Add(new SceneRenderBatch(vertices, modelViewProjection));
        }

        return new SceneRenderSubmission(batches);
    }

    private static SceneRenderMeshVertex[] ResolveMesh(string meshId)
    {
        return sMeshes.TryGetValue(meshId, out var meshVertices)
            ? meshVertices
            : sMeshes[kTriangleMeshId];
    }

    private static SceneRenderMaterialParameters ResolveMaterial(string materialId)
    {
        return sMaterials.TryGetValue(materialId, out var material)
            ? material
            : sDefaultMaterial;
    }

    private static Matrix4x4 BuildTransformMatrix(SceneTransform transform)
    {
        if (transform.Equals(SceneTransform.Identity))
        {
            return Matrix4x4.Identity;
        }

        var scale = Matrix4x4.CreateScale(transform.Scale);
        var rotation = Matrix4x4.CreateFromQuaternion(transform.Rotation);
        var translation = Matrix4x4.CreateTranslation(transform.Position);
        return scale * rotation * translation;
    }
}
