using System.ComponentModel;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;

namespace Engine.Platform;

public sealed class NullWindowService : IWindowService
{
    private readonly NativeWindow? nativeWindow;
    private readonly bool usesNativeWindow;
    private bool closeRequested;
    private bool disposed;

    public NullWindowService(WindowConfig configuration, bool useNativeWindow = true)
    {
        Configuration = configuration;
        usesNativeWindow = useNativeWindow;

        if (!usesNativeWindow)
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

        nativeWindow = new NativeWindow(settings);
        nativeWindow.MakeCurrent();
        nativeWindow.Closing += OnClosing;
    }

    public WindowConfig Configuration { get; }
    public bool IsCloseRequested => closeRequested || (nativeWindow?.IsExiting ?? false);
    public bool Exists => !disposed && (nativeWindow?.Exists ?? true);

    public void ProcessEvents(double timeoutSeconds = 0)
    {
        ThrowIfDisposed();

        if (!usesNativeWindow)
        {
            return;
        }

        nativeWindow!.ProcessEvents(timeoutSeconds);
        closeRequested |= nativeWindow.IsExiting;
    }

    public void RequestClose()
    {
        ThrowIfDisposed();
        closeRequested = true;

        if (usesNativeWindow)
        {
            nativeWindow!.Close();
        }
    }

    public void Present()
    {
        ThrowIfDisposed();

        if (!usesNativeWindow)
        {
            return;
        }

        nativeWindow!.Context.SwapBuffers();
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (disposed)
        {
            return;
        }

        if (disposing && nativeWindow is not null)
        {
            nativeWindow.Closing -= OnClosing;
            nativeWindow.Dispose();
        }

        disposed = true;
    }

    private void OnClosing(CancelEventArgs _)
    {
        closeRequested = true;
    }

    private void ThrowIfDisposed()
    {
        if (disposed)
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
