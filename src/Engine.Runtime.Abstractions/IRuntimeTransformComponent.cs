using Engine.Contracts;

namespace Engine.Runtime.Abstractions;

public interface IRuntimeTransformComponent : IRuntimeComponent
{
    SceneTransform LocalTransform { get; }

    void SetLocalTransform(SceneTransform transform);
}
