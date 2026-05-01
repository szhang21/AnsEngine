namespace Engine.Scene;

public sealed record RuntimeSceneSnapshot(
    IReadOnlyList<SceneRuntimeObjectSnapshot> Objects,
    SceneCameraRuntimeSnapshot Camera)
{
    public int ObjectCount => Objects.Count;
}
