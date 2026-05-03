namespace Engine.Platform;

public sealed class NativeWindowInputService : IInputService
{
    private readonly IKeyboardStateProvider mKeyboardStateProvider;

    public NativeWindowInputService(IKeyboardStateProvider keyboardStateProvider)
    {
        mKeyboardStateProvider = keyboardStateProvider ?? throw new ArgumentNullException(nameof(keyboardStateProvider));
    }

    public InputSnapshot GetSnapshot()
    {
        return InputSnapshot.FromKeyStates(
            mKeyboardStateProvider.IsKeyDown(EngineKey.W),
            mKeyboardStateProvider.IsKeyDown(EngineKey.A),
            mKeyboardStateProvider.IsKeyDown(EngineKey.S),
            mKeyboardStateProvider.IsKeyDown(EngineKey.D));
    }
}
