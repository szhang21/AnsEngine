namespace Engine.Scene;

public sealed record RuntimeSceneSnapshot(
    IReadOnlyList<SceneRuntimeObjectSnapshot> Objects,
    SceneCameraRuntimeSnapshot Camera,
    int UpdateFrameCount,
    double AccumulatedUpdateSeconds)
{
    public int ObjectCount => Objects.Count;
}
