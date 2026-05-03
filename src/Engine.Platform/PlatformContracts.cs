namespace Engine.Platform;

public readonly record struct WindowConfig(int Width, int Height, string Title);

public enum EngineKey
{
    W,
    A,
    S,
    D
}

public readonly record struct InputSnapshot
{
    private const byte kKeyW = 1 << 0;
    private const byte kKeyA = 1 << 1;
    private const byte kKeyS = 1 << 2;
    private const byte kKeyD = 1 << 3;
    private const byte kAllSupportedKeys = kKeyW | kKeyA | kKeyS | kKeyD;

    private readonly byte mPressedKeys;

    public InputSnapshot(bool anyInputDetected)
        : this(anyInputDetected ? kAllSupportedKeys : (byte)0)
    {
    }

    private InputSnapshot(byte pressedKeys)
    {
        mPressedKeys = (byte)(pressedKeys & kAllSupportedKeys);
    }

    public static InputSnapshot Empty { get; } = new(0);

    public bool AnyInputDetected => mPressedKeys != 0;

    public static InputSnapshot FromKeys(params EngineKey[] keys)
    {
        ArgumentNullException.ThrowIfNull(keys);

        var pressedKeys = (byte)0;
        foreach (var key in keys)
        {
            pressedKeys |= GetKeyMask(key);
        }

        return new InputSnapshot(pressedKeys);
    }

    public static InputSnapshot FromKeyStates(bool isWDown, bool isADown, bool isSDown, bool isDDown)
    {
        var pressedKeys = (byte)0;
        pressedKeys |= isWDown ? kKeyW : (byte)0;
        pressedKeys |= isADown ? kKeyA : (byte)0;
        pressedKeys |= isSDown ? kKeyS : (byte)0;
        pressedKeys |= isDDown ? kKeyD : (byte)0;
        return new InputSnapshot(pressedKeys);
    }

    public bool IsKeyDown(EngineKey key)
    {
        return (mPressedKeys & GetKeyMask(key)) != 0;
    }

    private static byte GetKeyMask(EngineKey key)
    {
        return key switch
        {
            EngineKey.W => kKeyW,
            EngineKey.A => kKeyA,
            EngineKey.S => kKeyS,
            EngineKey.D => kKeyD,
            _ => throw new ArgumentOutOfRangeException(nameof(key), key, "Unsupported engine key.")
        };
    }
}

public readonly record struct TimeSnapshot(double DeltaSeconds, double TotalSeconds, double FramesPerSecond);

public interface IWindowService : IDisposable
{
    WindowConfig Configuration { get; }
    bool IsCloseRequested { get; }
    bool Exists { get; }

    void ProcessEvents(double timeoutSeconds = 0);
    void Present();
    void RequestClose();
}

public interface IInputService
{
    InputSnapshot GetSnapshot();
}

public interface IKeyboardStateProvider
{
    bool IsKeyDown(EngineKey key);
}

public interface ITimeService
{
    TimeSnapshot Current { get; }
}
