namespace Engine.Runtime.Abstractions;

public interface IRuntimeObject
{
    string ObjectId { get; }

    string ObjectName { get; }

    T? GetComponent<T>() where T : class, IRuntimeComponent;

    bool HasComponent<T>() where T : class, IRuntimeComponent;
}
