using Engine.Contracts;

namespace Engine.Scripting;

public interface IScriptSelfTransform
{
    SceneTransform LocalTransform { get; }

    void SetLocalTransform(SceneTransform transform);
}
