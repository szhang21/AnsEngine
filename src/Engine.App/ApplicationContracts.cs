namespace Engine.App;

using Engine.Runtime;
using Engine.SceneData;

public interface IApplication
{
    int Run();
}

public interface IRuntimeBootstrap
{
    IApplication Build();
}

public interface IRuntimeSessionHost
{
    RuntimeInitializationResult Initialize(SceneDescription sceneDescription);
    RuntimeTickResult Tick(RuntimeTickContext context);
}
