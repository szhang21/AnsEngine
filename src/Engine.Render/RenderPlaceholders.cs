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
                                             layout (location = 1) in vec3 aNormal;
                                             uniform mat4 uMvp;
                                             uniform vec3 uColor;
                                             out vec3 vColor;
                                             out vec3 vNormal;
                                             void main()
                                             {
                                                 vColor = uColor;
                                                 vNormal = aNormal;
                                                 gl_Position = uMvp * vec4(aPosition, 1.0);
                                             }
                                             """;
    private const string kFragmentShaderSource = """
                                               #version 330 core
                                               in vec3 vColor;
                                               in vec3 vNormal;
                                               out vec4 fragColor;
                                               void main()
                                               {
                                                   vec3 lightDirection = normalize(vec3(0.35, 0.55, 0.75));
                                                   vec3 normal = normalize(vNormal);
                                                   float diffuse = max(dot(normal, lightDirection), 0.0);
                                                   float lighting = 0.35 + (0.65 * diffuse);
                                                   fragColor = vec4(vColor * lighting, 1.0);
                                               }
                                               """;
    private const int kPositionOffset = 0;
    private const int kNormalOffset = 3;
    private const int kVertexStride = 6;

    private readonly IWindowService mWindowService;
    private readonly EngineRuntimeInfo mRuntimeInfo;
    private readonly ISceneRenderContractProvider mSceneProvider;
    private readonly IMeshAssetProvider? mMeshAssetProvider;
    private readonly SceneRenderGpuMeshResourceCache<SceneRenderGpuMeshResource> mGpuMeshCache = new();
    private int mShaderProgramHandle;
    private int mVertexShaderHandle;
    private int mFragmentShaderHandle;
    private int mMvpUniformLocation = -1;
    private int mColorUniformLocation = -1;

    public NullRenderer(
        IWindowService windowService,
        EngineRuntimeInfo runtimeInfo,
        ISceneRenderContractProvider sceneProvider)
        : this(windowService, runtimeInfo, sceneProvider, meshAssetProvider: null)
    {
    }

    public NullRenderer(
        IWindowService windowService,
        EngineRuntimeInfo runtimeInfo,
        ISceneRenderContractProvider sceneProvider,
        IMeshAssetProvider? meshAssetProvider)
    {
        mWindowService = windowService ?? throw new ArgumentNullException(nameof(windowService));
        mRuntimeInfo = runtimeInfo ?? throw new ArgumentNullException(nameof(runtimeInfo));
        mSceneProvider = sceneProvider ?? throw new ArgumentNullException(nameof(sceneProvider));
        mMeshAssetProvider = meshAssetProvider;
    }

    public bool IsInitialized { get; private set; }

    public void Initialize()
    {
        _ = mWindowService.Configuration;
        _ = mRuntimeInfo.Version;
        GL.Viewport(0, 0, mWindowService.Configuration.Width, mWindowService.Configuration.Height);
        GL.ClearColor(kClearRed, kClearGreen, kClearBlue, kClearAlpha);
        GL.ClearDepth(1.0);
        GL.Enable(EnableCap.DepthTest);
        GL.DepthFunc(DepthFunction.Lequal);
        GL.DepthMask(true);
        GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
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
        var submission = SceneRenderSubmissionBuilder.Build(sceneFrame, mMeshAssetProvider);

        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        if (submission.Batches.Count == 0)
        {
            GL.Flush();
            return;
        }

        GL.UseProgram(mShaderProgramHandle);

        foreach (var batch in submission.Batches)
        {
            if (batch.MeshVertices.Count == 0)
            {
                continue;
            }

            var meshResource = mGpuMeshCache.GetOrCreate(
                batch.MeshCacheKey,
                () => CreateGpuMeshResource(batch.MeshVertices));

            GL.BindVertexArray(meshResource.VertexArrayHandle);
            SetModelViewProjection(batch.ModelViewProjection);
            SetMaterialColor(batch.Material);
            GL.DrawArrays(PrimitiveType.Triangles, 0, meshResource.VertexCount);
        }

        GL.BindVertexArray(0);
        GL.Flush();
    }

    public void Shutdown()
    {
        ReleaseTrianglePipeline();
        GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
        GL.Disable(EnableCap.DepthTest);
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
        mColorUniformLocation = GL.GetUniformLocation(mShaderProgramHandle, "uColor");
        if (mColorUniformLocation < 0)
        {
            throw new InvalidOperationException("Render program is missing required uniform uColor.");
        }
    }

    private static SceneRenderGpuMeshResource CreateGpuMeshResource(IReadOnlyList<SceneRenderMeshVertex> meshVertices)
    {
        var flattenedVertices = Flatten(meshVertices);
        var vertexArrayHandle = GL.GenVertexArray();
        var vertexBufferHandle = GL.GenBuffer();
        GL.BindVertexArray(vertexArrayHandle);
        GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferHandle);
        GL.BufferData(
            BufferTarget.ArrayBuffer,
            flattenedVertices.Length * sizeof(float),
            flattenedVertices,
            BufferUsageHint.StaticDraw);
        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, kVertexStride * sizeof(float), kPositionOffset * sizeof(float));
        GL.EnableVertexAttribArray(1);
        GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, kVertexStride * sizeof(float), kNormalOffset * sizeof(float));
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        GL.BindVertexArray(0);
        return new SceneRenderGpuMeshResource(vertexArrayHandle, vertexBufferHandle, meshVertices.Count);
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

    private static float[] Flatten(IReadOnlyList<SceneRenderMeshVertex> vertices)
    {
        var flattened = new float[vertices.Count * kVertexStride];
        var writeIndex = 0;
        foreach (var vertex in vertices)
        {
            flattened[writeIndex] = vertex.X;
            flattened[writeIndex + 1] = vertex.Y;
            flattened[writeIndex + 2] = vertex.Z;
            flattened[writeIndex + 3] = vertex.NormalX;
            flattened[writeIndex + 4] = vertex.NormalY;
            flattened[writeIndex + 5] = vertex.NormalZ;
            writeIndex += kVertexStride;
        }

        return flattened;
    }

    internal const bool ShouldTransposeModelViewProjectionUniform = false;

    internal static float[] FlattenModelViewProjection(System.Numerics.Matrix4x4 matrix)
    {
        return new[]
        {
            matrix.M11, matrix.M12, matrix.M13, matrix.M14,
            matrix.M21, matrix.M22, matrix.M23, matrix.M24,
            matrix.M31, matrix.M32, matrix.M33, matrix.M34,
            matrix.M41, matrix.M42, matrix.M43, matrix.M44
        };
    }

    private void SetModelViewProjection(System.Numerics.Matrix4x4 matrix)
    {
        var flattened = FlattenModelViewProjection(matrix);
        GL.UniformMatrix4(mMvpUniformLocation, 1, ShouldTransposeModelViewProjectionUniform, flattened);
    }

    private void SetMaterialColor(SceneRenderMaterialParameters material)
    {
        GL.Uniform3(mColorUniformLocation, material.Red, material.Green, material.Blue);
    }

    private void ReleaseTrianglePipeline()
    {
        mGpuMeshCache.Clear(ReleaseGpuMeshResource);

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

        mColorUniformLocation = -1;
        mMvpUniformLocation = -1;
    }

    private static void ReleaseGpuMeshResource(SceneRenderGpuMeshResource meshResource)
    {
        if (meshResource.VertexBufferHandle != 0)
        {
            GL.DeleteBuffer(meshResource.VertexBufferHandle);
        }

        if (meshResource.VertexArrayHandle != 0)
        {
            GL.DeleteVertexArray(meshResource.VertexArrayHandle);
        }
    }
}
