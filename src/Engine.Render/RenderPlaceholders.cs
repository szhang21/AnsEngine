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
                                             uniform mat4 uMvp;
                                             out vec3 vColor;
                                             void main()
                                             {
                                                 vColor = aColor;
                                                 gl_Position = uMvp * vec4(aPosition, 1.0);
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

    private readonly IWindowService windowService;
    private readonly EngineRuntimeInfo runtimeInfo;
    private readonly ISceneRenderContractProvider sceneProvider;
    private int shaderProgramHandle;
    private int vertexShaderHandle;
    private int fragmentShaderHandle;
    private int vertexArrayHandle;
    private int vertexBufferHandle;
    private int mvpUniformLocation = -1;

    public NullRenderer(
        IWindowService windowService,
        EngineRuntimeInfo runtimeInfo,
        ISceneRenderContractProvider sceneProvider)
    {
        this.windowService = windowService ?? throw new ArgumentNullException(nameof(windowService));
        this.runtimeInfo = runtimeInfo ?? throw new ArgumentNullException(nameof(runtimeInfo));
        this.sceneProvider = sceneProvider ?? throw new ArgumentNullException(nameof(sceneProvider));
    }

    public bool IsInitialized { get; private set; }

    public void Initialize()
    {
        _ = windowService.Configuration;
        _ = runtimeInfo.Version;
        GL.Viewport(0, 0, windowService.Configuration.Width, windowService.Configuration.Height);
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

        var sceneFrame = sceneProvider.BuildRenderFrame();
        var submission = SceneRenderSubmissionBuilder.Build(sceneFrame);

        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        if (submission.Batches.Count == 0)
        {
            GL.Flush();
            return;
        }

        GL.UseProgram(shaderProgramHandle);
        GL.BindVertexArray(vertexArrayHandle);
        GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferHandle);

        foreach (var batch in submission.Batches)
        {
            var vertices = batch.Vertices.Count == 0 ? EmptyVertices : Flatten(batch.Vertices);
            if (vertices.Length == 0)
            {
                continue;
            }

            GL.BufferData(
                BufferTarget.ArrayBuffer,
                vertices.Length * sizeof(float),
                vertices,
                BufferUsageHint.DynamicDraw);
            SetModelViewProjection(batch.ModelViewProjection);
            GL.DrawArrays(PrimitiveType.Triangles, 0, vertices.Length / VertexStride);
        }

        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
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
        vertexShaderHandle = CompileShader(ShaderType.VertexShader, VertexShaderSource);
        fragmentShaderHandle = CompileShader(ShaderType.FragmentShader, FragmentShaderSource);

        shaderProgramHandle = GL.CreateProgram();
        GL.AttachShader(shaderProgramHandle, vertexShaderHandle);
        GL.AttachShader(shaderProgramHandle, fragmentShaderHandle);
        GL.LinkProgram(shaderProgramHandle);
        GL.GetProgram(shaderProgramHandle, GetProgramParameterName.LinkStatus, out var linkStatus);
        if (linkStatus == 0)
        {
            var info = GL.GetProgramInfoLog(shaderProgramHandle);
            throw new InvalidOperationException($"Render program link failed: {info}");
        }
        mvpUniformLocation = GL.GetUniformLocation(shaderProgramHandle, "uMvp");
        if (mvpUniformLocation < 0)
        {
            throw new InvalidOperationException("Render program is missing required uniform uMvp.");
        }

        vertexArrayHandle = GL.GenVertexArray();
        vertexBufferHandle = GL.GenBuffer();
        GL.BindVertexArray(vertexArrayHandle);
        GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferHandle);
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

    private void SetModelViewProjection(System.Numerics.Matrix4x4 matrix)
    {
        var flattened = new[]
        {
            matrix.M11, matrix.M12, matrix.M13, matrix.M14,
            matrix.M21, matrix.M22, matrix.M23, matrix.M24,
            matrix.M31, matrix.M32, matrix.M33, matrix.M34,
            matrix.M41, matrix.M42, matrix.M43, matrix.M44
        };
        GL.UniformMatrix4(mvpUniformLocation, 1, true, flattened);
    }

    private void ReleaseTrianglePipeline()
    {
        if (vertexBufferHandle != 0)
        {
            GL.DeleteBuffer(vertexBufferHandle);
            vertexBufferHandle = 0;
        }

        if (vertexArrayHandle != 0)
        {
            GL.DeleteVertexArray(vertexArrayHandle);
            vertexArrayHandle = 0;
        }

        if (shaderProgramHandle != 0)
        {
            GL.DeleteProgram(shaderProgramHandle);
            shaderProgramHandle = 0;
        }

        if (vertexShaderHandle != 0)
        {
            GL.DeleteShader(vertexShaderHandle);
            vertexShaderHandle = 0;
        }

        if (fragmentShaderHandle != 0)
        {
            GL.DeleteShader(fragmentShaderHandle);
            fragmentShaderHandle = 0;
        }

        mvpUniformLocation = -1;
    }
}
