using System.ComponentModel;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;

namespace Engine.Platform;

public sealed class NullWindowService : IWindowService
{
    private readonly NativeWindow? _nativeWindow;
    private readonly bool _usesNativeWindow;
    private bool _closeRequested;
    private bool _disposed;

    public NullWindowService(WindowConfig configuration, bool useNativeWindow = true)
    {
        Configuration = configuration;
        _usesNativeWindow = useNativeWindow;

        if (!_usesNativeWindow)
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

        _nativeWindow = new NativeWindow(settings);
        _nativeWindow.Closing += OnClosing;
    }

    public WindowConfig Configuration { get; }
    public bool IsCloseRequested => _closeRequested || (_nativeWindow?.IsExiting ?? false);
    public bool Exists => !_disposed && (_nativeWindow?.Exists ?? true);

    public void ProcessEvents(double timeoutSeconds = 0)
    {
        ThrowIfDisposed();

        if (!_usesNativeWindow)
        {
            return;
        }

        _nativeWindow!.ProcessEvents(timeoutSeconds);
        _closeRequested |= _nativeWindow.IsExiting;
    }

    public void RequestClose()
    {
        ThrowIfDisposed();
        _closeRequested = true;

        if (_usesNativeWindow)
        {
            _nativeWindow!.Close();
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing && _nativeWindow is not null)
        {
            _nativeWindow.Closing -= OnClosing;
            _nativeWindow.Dispose();
        }

        _disposed = true;
    }

    private void OnClosing(CancelEventArgs _)
    {
        _closeRequested = true;
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
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
