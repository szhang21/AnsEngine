namespace Engine.Editor.App;

public sealed record EditorScenePreviewSnapshot(
    bool HasScene,
    bool IsNonBlank,
    int RenderItemCount,
    int BatchCount,
    int MeshVertexCount,
    int RefreshVersion,
    string StatusText)
{
    public static EditorScenePreviewSnapshot Empty { get; } = new(
        false,
        false,
        0,
        0,
        0,
        0,
        "No scene loaded.");
}
