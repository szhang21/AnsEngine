using Engine.Core;
using Engine.Contracts;
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
    private const string VertexShaderSource = """
                                             #version 330 core
                                             layout (location = 0) in vec3 aPosition;
                                             layout (location = 1) in vec3 aColor;
                                             out vec3 vColor;
                                             void main()
                                             {
                                                 vColor = aColor;
                                                 gl_Position = vec4(aPosition, 1.0);
                                             }
                                             """;
    private const string FragmentShaderSource = """
                                               #version 330 core
                                               in vec3 vColor;
                                               out vec4 fragColor;
                                               void main()
                                               {
                                                   fragColor = vec4(vColor, 1.0);
                                               }
                                               """;
    private const int VertexStride = 6;
    private static readonly float[] EmptyVertices = [];

    private readonly IWindowService _windowService;
    private readonly EngineRuntimeInfo _runtimeInfo;
    private readonly ISceneRenderContractProvider _sceneProvider;
    private int _shaderProgramHandle;
    private int _vertexShaderHandle;
    private int _fragmentShaderHandle;
    private int _vertexArrayHandle;
    private int _vertexBufferHandle;

    public NullRenderer(IWindowService windowService, EngineRuntimeInfo runtimeInfo)
        : this(windowService, runtimeInfo, new DefaultSceneRenderContractProvider())
    {
    }

    public NullRenderer(
        IWindowService windowService,
        EngineRuntimeInfo runtimeInfo,
        ISceneRenderContractProvider sceneProvider)
    {
        _windowService = windowService;
        _runtimeInfo = runtimeInfo;
        _sceneProvider = sceneProvider;
    }

    public bool IsInitialized { get; private set; }

    public void Initialize()
    {
        _ = _windowService.Configuration;
        _ = _runtimeInfo.Version;
        GL.Viewport(0, 0, _windowService.Configuration.Width, _windowService.Configuration.Height);
        GL.ClearColor(ClearRed, ClearGreen, ClearBlue, ClearAlpha);
        BuildTrianglePipeline();
        IsInitialized = true;
    }

    public void RenderFrame()
    {
        if (!IsInitialized)
        {
            throw new InvalidOperationException("Renderer is not initialized.");
        }

        var sceneFrame = _sceneProvider.BuildRenderFrame();
        var submission = SceneRenderSubmissionBuilder.Build(sceneFrame);
        var vertices = submission.Vertices.Count == 0 ? EmptyVertices : Flatten(submission.Vertices);

        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        if (vertices.Length == 0)
        {
            GL.Flush();
            return;
        }

        GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferHandle);
        GL.BufferData(
            BufferTarget.ArrayBuffer,
            vertices.Length * sizeof(float),
            vertices,
            BufferUsageHint.DynamicDraw);

        GL.UseProgram(_shaderProgramHandle);
        GL.BindVertexArray(_vertexArrayHandle);
        GL.DrawArrays(PrimitiveType.Triangles, 0, vertices.Length / VertexStride);
        GL.BindVertexArray(0);
        GL.Flush();
    }

    public void Shutdown()
    {
        ReleaseTrianglePipeline();
        IsInitialized = false;
    }

    private void BuildTrianglePipeline()
    {
        _vertexShaderHandle = CompileShader(ShaderType.VertexShader, VertexShaderSource);
        _fragmentShaderHandle = CompileShader(ShaderType.FragmentShader, FragmentShaderSource);

        _shaderProgramHandle = GL.CreateProgram();
        GL.AttachShader(_shaderProgramHandle, _vertexShaderHandle);
        GL.AttachShader(_shaderProgramHandle, _fragmentShaderHandle);
        GL.LinkProgram(_shaderProgramHandle);
        GL.GetProgram(_shaderProgramHandle, GetProgramParameterName.LinkStatus, out var linkStatus);
        if (linkStatus == 0)
        {
            var info = GL.GetProgramInfoLog(_shaderProgramHandle);
            throw new InvalidOperationException($"Render program link failed: {info}");
        }

        _vertexArrayHandle = GL.GenVertexArray();
        _vertexBufferHandle = GL.GenBuffer();
        GL.BindVertexArray(_vertexArrayHandle);
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferHandle);
        GL.BufferData(BufferTarget.ArrayBuffer, 0, IntPtr.Zero, BufferUsageHint.DynamicDraw);
        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, VertexStride * sizeof(float), 0);
        GL.EnableVertexAttribArray(1);
        GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, VertexStride * sizeof(float), 3 * sizeof(float));
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        GL.BindVertexArray(0);
    }

    private static int CompileShader(ShaderType shaderType, string shaderSource)
    {
        var shaderHandle = GL.CreateShader(shaderType);
        GL.ShaderSource(shaderHandle, shaderSource);
        GL.CompileShader(shaderHandle);
        GL.GetShader(shaderHandle, ShaderParameter.CompileStatus, out var compileStatus);
        if (compileStatus == 0)
        {
            var info = GL.GetShaderInfoLog(shaderHandle);
            throw new InvalidOperationException($"{shaderType} compile failed: {info}");
        }

        return shaderHandle;
    }

    private static float[] Flatten(IReadOnlyList<SceneRenderVertex> vertices)
    {
        var flattened = new float[vertices.Count * VertexStride];
        var writeIndex = 0;
        foreach (var vertex in vertices)
        {
            flattened[writeIndex] = vertex.X;
            flattened[writeIndex + 1] = vertex.Y;
            flattened[writeIndex + 2] = vertex.Z;
            flattened[writeIndex + 3] = vertex.R;
            flattened[writeIndex + 4] = vertex.G;
            flattened[writeIndex + 5] = vertex.B;
            writeIndex += VertexStride;
        }

        return flattened;
    }

    private void ReleaseTrianglePipeline()
    {
        if (_vertexBufferHandle != 0)
        {
            GL.DeleteBuffer(_vertexBufferHandle);
            _vertexBufferHandle = 0;
        }

        if (_vertexArrayHandle != 0)
        {
            GL.DeleteVertexArray(_vertexArrayHandle);
            _vertexArrayHandle = 0;
        }

        if (_shaderProgramHandle != 0)
        {
            GL.DeleteProgram(_shaderProgramHandle);
            _shaderProgramHandle = 0;
        }

        if (_vertexShaderHandle != 0)
        {
            GL.DeleteShader(_vertexShaderHandle);
            _vertexShaderHandle = 0;
        }

        if (_fragmentShaderHandle != 0)
        {
            GL.DeleteShader(_fragmentShaderHandle);
            _fragmentShaderHandle = 0;
        }
    }
}
