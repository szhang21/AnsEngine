namespace Engine.Platform;

public readonly record struct WindowConfig(int Width, int Height, string Title);

public readonly record struct InputSnapshot(bool AnyInputDetected);

public readonly record struct TimeSnapshot(double DeltaSeconds, double TotalSeconds, double FramesPerSecond);

public interface IWindowService : IDisposable
{
    WindowConfig Configuration { get; }
    bool IsCloseRequested { get; }
    bool Exists { get; }

    void ProcessEvents(double timeoutSeconds = 0);
    void RequestClose();
}

public interface IInputService
{
    InputSnapshot GetSnapshot();
}

public interface ITimeService
{
    TimeSnapshot Current { get; }
}
