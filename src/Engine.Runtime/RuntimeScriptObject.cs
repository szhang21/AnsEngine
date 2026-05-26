namespace Engine.Runtime;

using Engine.Runtime.Abstractions;
using Engine.Scene;

internal sealed class RuntimeScriptObject : IRuntimeObject
{
    private readonly RuntimeScriptTransformComponent mTransform;

    public RuntimeScriptObject(SceneScriptObjectHandle handle)
    {
        ArgumentNullException.ThrowIfNull(handle);
        ObjectId = handle.ObjectId;
        ObjectName = handle.ObjectName;
        mTransform = new RuntimeScriptTransformComponent(handle);
    }

    public string ObjectId { get; }

    public string ObjectName { get; }

    public T? GetComponent<T>() where T : class, IRuntimeComponent
    {
        return mTransform as T;
    }

    public bool HasComponent<T>() where T : class, IRuntimeComponent
    {
        return GetComponent<T>() is not null;
    }
}
