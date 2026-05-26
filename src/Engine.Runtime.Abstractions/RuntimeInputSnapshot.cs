namespace Engine.Runtime.Abstractions;

public readonly record struct RuntimeInputSnapshot
{
    private readonly RuntimeKey[]? mKeys;

    private RuntimeInputSnapshot(RuntimeKey[] keys)
    {
        mKeys = keys;
    }

    public static RuntimeInputSnapshot Empty => default;

    public bool AnyInputDetected => mKeys is { Length: > 0 };

    public static RuntimeInputSnapshot FromKeys(params RuntimeKey[] keys)
    {
        ArgumentNullException.ThrowIfNull(keys);

        return keys.Length == 0
            ? Empty
            : new RuntimeInputSnapshot(keys.Distinct().ToArray());
    }

    public bool IsKeyDown(RuntimeKey key)
    {
        return mKeys is not null && Array.IndexOf(mKeys, key) >= 0;
    }
}
