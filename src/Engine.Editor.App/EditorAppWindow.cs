using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace Engine.Editor.App;

public sealed class EditorAppWindow : GameWindow
{
    private readonly EditorAppController mController;
    private readonly EditorAppOptions mOptions;
    private readonly EditorGuiRenderer mGuiRenderer;
    private ImGuiOpenGlRenderer? mImGuiRenderer;
    private double mElapsedSeconds;

    public EditorAppWindow(EditorAppController controller, EditorAppOptions options)
        : base(
            GameWindowSettings.Default,
            new NativeWindowSettings
            {
                ClientSize = new Vector2i(1100, 720),
                Title = "AnsEngine Editor"
            })
    {
        mController = controller ?? throw new ArgumentNullException(nameof(controller));
        mOptions = options ?? throw new ArgumentNullException(nameof(options));
        mGuiRenderer = new EditorGuiRenderer(mController);
    }

    protected override void OnLoad()
    {
        base.OnLoad();
        if (mOptions.EnableNativeImGuiFrames)
        {
            mImGuiRenderer = new ImGuiOpenGlRenderer(this);
        }

        GL.ClearColor(0.08f, 0.09f, 0.10f, 1.0f);
        UpdateTitle();
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);
        mElapsedSeconds += args.Time;
        if (mOptions.AutoExitSeconds > 0 && mElapsedSeconds >= mOptions.AutoExitSeconds)
        {
            Close();
        }
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);
        GL.Clear(ClearBufferMask.ColorBufferBit);
        if (mOptions.EnableNativeImGuiFrames)
        {
            mImGuiRenderer!.BeginFrame(args.Time);
        }

        mGuiRenderer.RenderFrame(mOptions.EnableNativeImGuiFrames);
        if (mOptions.EnableNativeImGuiFrames)
        {
            mImGuiRenderer!.EndFrame();
        }

        UpdateTitle();
        SwapBuffers();
    }

    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);
        mImGuiRenderer?.Resize(ClientSize.X, ClientSize.Y);
    }

    protected override void OnTextInput(TextInputEventArgs e)
    {
        base.OnTextInput(e);
        mImGuiRenderer?.QueueTextInput(e.Unicode);
    }

    protected override void OnUnload()
    {
        mImGuiRenderer?.Dispose();
        mImGuiRenderer = null;
        base.OnUnload();
    }

    private void UpdateTitle()
    {
        var path = string.IsNullOrWhiteSpace(mController.Session.SceneFilePath)
            ? "<no scene>"
            : Path.GetFileName(mController.Session.SceneFilePath);
        var dirty = mController.Session.IsDirty ? "dirty" : "clean";
        Title = $"AnsEngine Editor - {path} - {dirty}";
    }
}
