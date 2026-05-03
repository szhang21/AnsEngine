using System.ComponentModel;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Engine.Platform;

public sealed class NullWindowService : IWindowService, IKeyboardStateProvider
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

        var settings = NativeWindowSettings.Default;
        settings.Title = configuration.Title;
        settings.ClientSize = new Vector2i(configuration.Width, configuration.Height);
        settings.StartVisible = true;
        settings.StartFocused = true;
        settings.API = ContextAPI.OpenGL;
        settings.Profile = ContextProfile.Core;
        settings.Flags = ContextFlags.ForwardCompatible;
        settings.DepthBits = 24;
        settings.StencilBits = 8;

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

    public bool IsKeyDown(EngineKey key)
    {
        ThrowIfDisposed();

        if (!mUsesNativeWindow)
        {
            return false;
        }

        return mNativeWindow!.KeyboardState.IsKeyDown(MapKey(key));
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

    private static Keys MapKey(EngineKey key)
    {
        return key switch
        {
            EngineKey.W => Keys.W,
            EngineKey.A => Keys.A,
            EngineKey.S => Keys.S,
            EngineKey.D => Keys.D,
            _ => throw new ArgumentOutOfRangeException(nameof(key), key, "Unsupported engine key.")
        };
    }
}

public sealed class NullInputService : IInputService
{
    public InputSnapshot GetSnapshot()
    {
        return InputSnapshot.Empty;
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
