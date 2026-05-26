namespace Engine.Runtime;

using Engine.Contracts;
using Engine.Runtime.Abstractions;
using Engine.Scene;

internal sealed class RuntimeScriptTransformComponent : IRuntimeTransformComponent
{
    private readonly SceneScriptObjectHandle mHandle;

    public RuntimeScriptTransformComponent(SceneScriptObjectHandle handle)
    {
        mHandle = handle ?? throw new ArgumentNullException(nameof(handle));
    }

    public SceneTransform LocalTransform => mHandle.LocalTransform;

    public void SetLocalTransform(SceneTransform transform)
    {
        mHandle.SetLocalTransform(transform);
    }
}
