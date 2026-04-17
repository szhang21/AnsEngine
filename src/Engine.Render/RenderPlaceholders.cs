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
    private const float kClearRed = 0.08f;
    private const float kClearGreen = 0.16f;
    private const float kClearBlue = 0.24f;
    private const float kClearAlpha = 1.0f;
    private const string kVertexShaderSource = """
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
    private const string kFragmentShaderSource = """
                                               #version 330 core
                                               in vec3 vColor;
                                               out vec4 fragColor;
                                               void main()
                                               {
                                                   fragColor = vec4(vColor, 1.0);
                                               }
                                               """;
    private const int kVertexStride = 6;
    private static readonly float[] sEmptyVertices = [];

    private readonly IWindowService mWindowService;
    private readonly EngineRuntimeInfo mRuntimeInfo;
    private readonly ISceneRenderContractProvider mSceneProvider;
    private int mShaderProgramHandle;
    private int mVertexShaderHandle;
    private int mFragmentShaderHandle;
    private int mVertexArrayHandle;
    private int mVertexBufferHandle;
    private int mMvpUniformLocation = -1;

    public NullRenderer(
        IWindowService windowService,
        EngineRuntimeInfo runtimeInfo,
        ISceneRenderContractProvider sceneProvider)
    {
        mWindowService = windowService ?? throw new ArgumentNullException(nameof(windowService));
        mRuntimeInfo = runtimeInfo ?? throw new ArgumentNullException(nameof(runtimeInfo));
        mSceneProvider = sceneProvider ?? throw new ArgumentNullException(nameof(sceneProvider));
    }

    public bool IsInitialized { get; private set; }

    public void Initialize()
    {
        _ = mWindowService.Configuration;
        _ = mRuntimeInfo.Version;
        GL.Viewport(0, 0, mWindowService.Configuration.Width, mWindowService.Configuration.Height);
        GL.ClearColor(kClearRed, kClearGreen, kClearBlue, kClearAlpha);
        BuildTrianglePipeline();
        IsInitialized = true;
    }

    public void RenderFrame()
    {
        if (!IsInitialized)
        {
            throw new InvalidOperationException("Renderer is not initialized.");
        }

        var sceneFrame = mSceneProvider.BuildRenderFrame();
        var submission = SceneRenderSubmissionBuilder.Build(sceneFrame);

        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        if (submission.Batches.Count == 0)
        {
            GL.Flush();
            return;
        }

        GL.UseProgram(mShaderProgramHandle);
        GL.BindVertexArray(mVertexArrayHandle);
        GL.BindBuffer(BufferTarget.ArrayBuffer, mVertexBufferHandle);

        foreach (var batch in submission.Batches)
        {
            var vertices = batch.Vertices.Count == 0 ? sEmptyVertices : Flatten(batch.Vertices);
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
            GL.DrawArrays(PrimitiveType.Triangles, 0, vertices.Length / kVertexStride);
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
        mVertexShaderHandle = CompileShader(ShaderType.VertexShader, kVertexShaderSource);
        mFragmentShaderHandle = CompileShader(ShaderType.FragmentShader, kFragmentShaderSource);

        mShaderProgramHandle = GL.CreateProgram();
        GL.AttachShader(mShaderProgramHandle, mVertexShaderHandle);
        GL.AttachShader(mShaderProgramHandle, mFragmentShaderHandle);
        GL.LinkProgram(mShaderProgramHandle);
        GL.GetProgram(mShaderProgramHandle, GetProgramParameterName.LinkStatus, out var linkStatus);
        if (linkStatus == 0)
        {
            var info = GL.GetProgramInfoLog(mShaderProgramHandle);
            throw new InvalidOperationException($"Render program link failed: {info}");
        }
        mMvpUniformLocation = GL.GetUniformLocation(mShaderProgramHandle, "uMvp");
        if (mMvpUniformLocation < 0)
        {
            throw new InvalidOperationException("Render program is missing required uniform uMvp.");
        }

        mVertexArrayHandle = GL.GenVertexArray();
        mVertexBufferHandle = GL.GenBuffer();
        GL.BindVertexArray(mVertexArrayHandle);
        GL.BindBuffer(BufferTarget.ArrayBuffer, mVertexBufferHandle);
        GL.BufferData(BufferTarget.ArrayBuffer, 0, IntPtr.Zero, BufferUsageHint.DynamicDraw);
        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, kVertexStride * sizeof(float), 0);
        GL.EnableVertexAttribArray(1);
        GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, kVertexStride * sizeof(float), 3 * sizeof(float));
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
        var flattened = new float[vertices.Count * kVertexStride];
        var writeIndex = 0;
        foreach (var vertex in vertices)
        {
            flattened[writeIndex] = vertex.X;
            flattened[writeIndex + 1] = vertex.Y;
            flattened[writeIndex + 2] = vertex.Z;
            flattened[writeIndex + 3] = vertex.R;
            flattened[writeIndex + 4] = vertex.G;
            flattened[writeIndex + 5] = vertex.B;
            writeIndex += kVertexStride;
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
        GL.UniformMatrix4(mMvpUniformLocation, 1, true, flattened);
    }

    private void ReleaseTrianglePipeline()
    {
        if (mVertexBufferHandle != 0)
        {
            GL.DeleteBuffer(mVertexBufferHandle);
            mVertexBufferHandle = 0;
        }

        if (mVertexArrayHandle != 0)
        {
            GL.DeleteVertexArray(mVertexArrayHandle);
            mVertexArrayHandle = 0;
        }

        if (mShaderProgramHandle != 0)
        {
            GL.DeleteProgram(mShaderProgramHandle);
            mShaderProgramHandle = 0;
        }

        if (mVertexShaderHandle != 0)
        {
            GL.DeleteShader(mVertexShaderHandle);
            mVertexShaderHandle = 0;
        }

        if (mFragmentShaderHandle != 0)
        {
            GL.DeleteShader(mFragmentShaderHandle);
            mFragmentShaderHandle = 0;
        }

        mMvpUniformLocation = -1;
    }
}
