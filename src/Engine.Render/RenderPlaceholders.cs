using Engine.Core;
using Engine.Platform;
using OpenTK.Graphics.OpenGL4;

namespace Engine.Render;

public sealed class NullShaderProgram : IShaderProgram
{
    public NullShaderProgram(string name)
    {
        Name = name;
    }

    public string Name { get; }

    public void Bind()
    {
    }
}

public sealed class NullRenderer : IRenderer
{
    private const float ClearRed = 0.08f;
    private const float ClearGreen = 0.16f;
    private const float ClearBlue = 0.24f;
    private const float ClearAlpha = 1.0f;
    private readonly IWindowService _windowService;
    private readonly EngineRuntimeInfo _runtimeInfo;

    public NullRenderer(IWindowService windowService, EngineRuntimeInfo runtimeInfo)
    {
        _windowService = windowService;
        _runtimeInfo = runtimeInfo;
    }

    public bool IsInitialized { get; private set; }

    public void Initialize()
    {
        _ = _windowService.Configuration;
        _ = _runtimeInfo.Version;
        GL.ClearColor(ClearRed, ClearGreen, ClearBlue, ClearAlpha);
        IsInitialized = true;
    }

    public void RenderFrame()
    {
        if (!IsInitialized)
        {
            throw new InvalidOperationException("Renderer is not initialized.");
        }

        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
    }

    public void Shutdown()
    {
        IsInitialized = false;
    }
}
