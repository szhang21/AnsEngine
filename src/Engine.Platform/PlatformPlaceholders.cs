using System.ComponentModel;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;

namespace Engine.Platform;

public sealed class NullWindowService : IWindowService
{
    private readonly NativeWindow? mNativeWindow;
    private readonly bool mUsesNativeWindow;
    private bool mCloseRequested;
    private bool mDisposed;

    public NullWindowService(WindowConfig configuration, bool useNativeWindow = true)
    {
        Configuration = configuration;
        mUsesNativeWindow = useNativeWindow;

        if (!mUsesNativeWindow)
        {
            return;
        }

        var settings = new NativeWindowSettings
        {
            Title = configuration.Title,
            ClientSize = new Vector2i(configuration.Width, configuration.Height),
            StartVisible = true,
            StartFocused = true
        };

        mNativeWindow = new NativeWindow(settings);
        mNativeWindow.MakeCurrent();
        mNativeWindow.Closing += OnClosing;
    }

    public WindowConfig Configuration { get; }
    public bool IsCloseRequested => mCloseRequested || (mNativeWindow?.IsExiting ?? false);
    public bool Exists => !mDisposed && (mNativeWindow?.Exists ?? true);

    public void ProcessEvents(double timeoutSeconds = 0)
    {
        ThrowIfDisposed();

        if (!mUsesNativeWindow)
        {
            return;
        }

        mNativeWindow!.ProcessEvents(timeoutSeconds);
        mCloseRequested |= mNativeWindow.IsExiting;
    }

    public void RequestClose()
    {
        ThrowIfDisposed();
        mCloseRequested = true;

        if (mUsesNativeWindow)
        {
            mNativeWindow!.Close();
        }
    }

    public void Present()
    {
        ThrowIfDisposed();

        if (!mUsesNativeWindow)
        {
            return;
        }

        mNativeWindow!.Context.SwapBuffers();
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (mDisposed)
        {
            return;
        }

        if (disposing && mNativeWindow is not null)
        {
            mNativeWindow.Closing -= OnClosing;
            mNativeWindow.Dispose();
        }

        mDisposed = true;
    }

    private void OnClosing(CancelEventArgs _)
    {
        mCloseRequested = true;
    }

    private void ThrowIfDisposed()
    {
        if (mDisposed)
        {
            throw new ObjectDisposedException(nameof(NullWindowService));
        }
    }
}

public sealed class NullInputService : IInputService
{
    public InputSnapshot GetSnapshot()
    {
        return new InputSnapshot(AnyInputDetected: false);
    }
}

public sealed class FixedTimeService : ITimeService
{
    public FixedTimeService(TimeSnapshot current)
    {
        Current = current;
    }

    public TimeSnapshot Current { get; }
}
