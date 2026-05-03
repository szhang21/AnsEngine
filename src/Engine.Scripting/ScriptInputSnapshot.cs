namespace Engine.Scripting;

public readonly record struct ScriptInputSnapshot
{
    private const byte kKeyW = 1 << 0;
    private const byte kKeyA = 1 << 1;
    private const byte kKeyS = 1 << 2;
    private const byte kKeyD = 1 << 3;
    private const byte kAllSupportedKeys = kKeyW | kKeyA | kKeyS | kKeyD;

    private readonly byte mPressedKeys;

    private ScriptInputSnapshot(byte pressedKeys)
    {
        mPressedKeys = (byte)(pressedKeys & kAllSupportedKeys);
    }

    public static ScriptInputSnapshot Empty { get; } = new(0);

    public bool AnyInputDetected => mPressedKeys != 0;

    public static ScriptInputSnapshot FromKeys(params ScriptKey[] keys)
    {
        ArgumentNullException.ThrowIfNull(keys);

        var pressedKeys = (byte)0;
        foreach (var key in keys)
        {
            pressedKeys |= GetKeyMask(key);
        }

        return new ScriptInputSnapshot(pressedKeys);
    }

    public bool IsKeyDown(ScriptKey key)
    {
        return (mPressedKeys & GetKeyMask(key)) != 0;
    }

    private static byte GetKeyMask(ScriptKey key)
    {
        return key switch
        {
            ScriptKey.W => kKeyW,
            ScriptKey.A => kKeyA,
            ScriptKey.S => kKeyS,
            ScriptKey.D => kKeyD,
            _ => throw new ArgumentOutOfRangeException(nameof(key), key, "Unsupported script key.")
        };
    }
}
