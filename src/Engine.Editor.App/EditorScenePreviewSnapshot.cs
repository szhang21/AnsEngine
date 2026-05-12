using System.Numerics;

namespace Engine.Editor.App;

public readonly record struct EditorScenePreviewTriangle(
    Vector2 First,
    Vector2 Second,
    Vector2 Third,
    Vector3 Color);

public sealed record EditorScenePreviewSnapshot(
    bool HasScene,
    bool IsNonBlank,
    int RenderItemCount,
    int BatchCount,
    int MeshVertexCount,
    IReadOnlyList<EditorScenePreviewTriangle> ProjectedTriangles,
    int RefreshVersion,
    string StatusText)
{
    public static EditorScenePreviewSnapshot Empty { get; } = new(
        false,
        false,
        0,
        0,
        0,
        Array.Empty<EditorScenePreviewTriangle>(),
        0,
        "No scene loaded.");
}
