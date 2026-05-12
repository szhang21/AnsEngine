using Engine.Runtime.Abstractions;

namespace Engine.Scripting;

public interface IScriptSelfObject : IRuntimeObject
{
    IRuntimeTransformComponent Transform { get; }
}
