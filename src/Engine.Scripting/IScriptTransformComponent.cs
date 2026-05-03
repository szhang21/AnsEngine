using Engine.Contracts;

namespace Engine.Scripting;

public interface IScriptTransformComponent
{
    SceneTransform LocalTransform { get; }

    void SetLocalTransform(SceneTransform transform);
}
