using System.Numerics;
using System.Runtime.InteropServices;
using ImGuiNET;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Engine.Editor.App;

public sealed class ImGuiOpenGlRenderer : IDisposable
{
    private const int kVertexAttributePosition = 0;
    private const int kVertexAttributeUv = 1;
    private const int kVertexAttributeColor = 2;

    private readonly GameWindow mWindow;
    private IntPtr mContext;
    private int mVertexArray;
    private int mVertexBuffer;
    private int mIndexBuffer;
    private int mFontTexture;
    private int mShaderProgram;
    private int mProjectionLocation;
    private bool mDisposed;

    public ImGuiOpenGlRenderer(GameWindow window)
    {
        mWindow = window ?? throw new ArgumentNullException(nameof(window));
        mContext = ImGui.CreateContext();
        ImGui.SetCurrentContext(mContext);

        var io = ImGui.GetIO();
        io.Fonts.AddFontDefault();
        io.BackendFlags |= ImGuiBackendFlags.RendererHasVtxOffset;

        CreateDeviceResources();
        RecreateFontTexture();
    }

    public void BeginFrame(double deltaSeconds)
    {
        ImGui.SetCurrentContext(mContext);

        var io = ImGui.GetIO();
        io.DisplaySize = new Vector2(mWindow.ClientSize.X, mWindow.ClientSize.Y);
        io.DisplayFramebufferScale = Vector2.One;
        io.DeltaTime = deltaSeconds > 0 ? (float)deltaSeconds : 1.0f / 60.0f;
        io.MousePos = new Vector2(mWindow.MousePosition.X, mWindow.MousePosition.Y);
        io.MouseDown[0] = mWindow.MouseState.IsButtonDown(MouseButton.Left);
        io.MouseDown[1] = mWindow.MouseState.IsButtonDown(MouseButton.Right);
        io.MouseDown[2] = mWindow.MouseState.IsButtonDown(MouseButton.Middle);

        ImGui.NewFrame();
    }

    public void EndFrame()
    {
        ImGui.Render();
        RenderDrawData(ImGui.GetDrawData());
    }

    public void Resize(int width, int height)
    {
        GL.Viewport(0, 0, width, height);
    }

    public void Dispose()
    {
        if (mDisposed)
        {
            return;
        }

        if (mFontTexture != 0)
        {
            GL.DeleteTexture(mFontTexture);
            mFontTexture = 0;
        }

        if (mIndexBuffer != 0)
        {
            GL.DeleteBuffer(mIndexBuffer);
            mIndexBuffer = 0;
        }

        if (mVertexBuffer != 0)
        {
            GL.DeleteBuffer(mVertexBuffer);
            mVertexBuffer = 0;
        }

        if (mVertexArray != 0)
        {
            GL.DeleteVertexArray(mVertexArray);
            mVertexArray = 0;
        }

        if (mShaderProgram != 0)
        {
            GL.DeleteProgram(mShaderProgram);
            mShaderProgram = 0;
        }

        if (mContext != IntPtr.Zero)
        {
            ImGui.SetCurrentContext(mContext);
            ImGui.DestroyContext(mContext);
            mContext = IntPtr.Zero;
        }

        mDisposed = true;
    }

    private void CreateDeviceResources()
    {
        mVertexArray = GL.GenVertexArray();
        mVertexBuffer = GL.GenBuffer();
        mIndexBuffer = GL.GenBuffer();
        mShaderProgram = CreateShaderProgram();
        mProjectionLocation = GL.GetUniformLocation(mShaderProgram, "projection_matrix");

        GL.BindVertexArray(mVertexArray);
        GL.BindBuffer(BufferTarget.ArrayBuffer, mVertexBuffer);
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, mIndexBuffer);

        var stride = Marshal.SizeOf<ImDrawVert>();
        GL.EnableVertexAttribArray(kVertexAttributePosition);
        GL.VertexAttribPointer(kVertexAttributePosition, 2, VertexAttribPointerType.Float, false, stride, 0);
        GL.EnableVertexAttribArray(kVertexAttributeUv);
        GL.VertexAttribPointer(kVertexAttributeUv, 2, VertexAttribPointerType.Float, false, stride, 8);
        GL.EnableVertexAttribArray(kVertexAttributeColor);
        GL.VertexAttribPointer(kVertexAttributeColor, 4, VertexAttribPointerType.UnsignedByte, true, stride, 16);

        GL.BindVertexArray(0);
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
    }

    private void RecreateFontTexture()
    {
        var io = ImGui.GetIO();
        io.Fonts.GetTexDataAsRGBA32(out IntPtr pixels, out var width, out var height, out _);

        mFontTexture = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, mFontTexture);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        GL.PixelStore(PixelStoreParameter.UnpackRowLength, 0);
        GL.TexImage2D(
            TextureTarget.Texture2D,
            0,
            PixelInternalFormat.Rgba,
            width,
            height,
            0,
            PixelFormat.Rgba,
            PixelType.UnsignedByte,
            pixels);

        io.Fonts.SetTexID((IntPtr)mFontTexture);
        io.Fonts.ClearTexData();
    }

    private void RenderDrawData(ImDrawDataPtr drawData)
    {
        var framebufferWidth = (int)(drawData.DisplaySize.X * drawData.FramebufferScale.X);
        var framebufferHeight = (int)(drawData.DisplaySize.Y * drawData.FramebufferScale.Y);
        if (framebufferWidth <= 0 || framebufferHeight <= 0)
        {
            return;
        }

        GL.Viewport(0, 0, framebufferWidth, framebufferHeight);
        SetupRenderState(drawData.DisplaySize.X, drawData.DisplaySize.Y);

        GL.BindVertexArray(mVertexArray);
        for (var listIndex = 0; listIndex < drawData.CmdListsCount; listIndex += 1)
        {
            var commandList = drawData.CmdLists[listIndex];
            GL.BindBuffer(BufferTarget.ArrayBuffer, mVertexBuffer);
            GL.BufferData(
                BufferTarget.ArrayBuffer,
                commandList.VtxBuffer.Size * Marshal.SizeOf<ImDrawVert>(),
                commandList.VtxBuffer.Data,
                BufferUsageHint.StreamDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, mIndexBuffer);
            GL.BufferData(
                BufferTarget.ElementArrayBuffer,
                commandList.IdxBuffer.Size * sizeof(ushort),
                commandList.IdxBuffer.Data,
                BufferUsageHint.StreamDraw);

            for (var commandIndex = 0; commandIndex < commandList.CmdBuffer.Size; commandIndex += 1)
            {
                var command = commandList.CmdBuffer[commandIndex];
                if (command.UserCallback != IntPtr.Zero)
                {
                    continue;
                }

                var clip = command.ClipRect;
                var clipX = Math.Max(0, clip.X);
                var clipY = Math.Max(0, clip.Y);
                var clipZ = Math.Min(framebufferWidth, clip.Z);
                var clipW = Math.Min(framebufferHeight, clip.W);
                if (clipZ <= clipX || clipW <= clipY)
                {
                    continue;
                }

                GL.BindTexture(TextureTarget.Texture2D, command.TextureId.ToInt32());
                GL.Scissor(
                    (int)clipX,
                    framebufferHeight - (int)clipW,
                    (int)(clipZ - clipX),
                    (int)(clipW - clipY));
                GL.DrawElementsBaseVertex(
                    PrimitiveType.Triangles,
                    (int)command.ElemCount,
                    DrawElementsType.UnsignedShort,
                    (IntPtr)(command.IdxOffset * sizeof(ushort)),
                    (int)command.VtxOffset);
            }
        }

        GL.BindVertexArray(0);
        GL.Disable(EnableCap.ScissorTest);
    }

    private void SetupRenderState(float width, float height)
    {
        GL.Enable(EnableCap.Blend);
        GL.BlendEquation(BlendEquationMode.FuncAdd);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        GL.Disable(EnableCap.CullFace);
        GL.Disable(EnableCap.DepthTest);
        GL.Enable(EnableCap.ScissorTest);
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.UseProgram(mShaderProgram);

        var projection = new[]
        {
            2.0f / width, 0.0f, 0.0f, 0.0f,
            0.0f, 2.0f / -height, 0.0f, 0.0f,
            0.0f, 0.0f, -1.0f, 0.0f,
            -1.0f, 1.0f, 0.0f, 1.0f
        };
        GL.UniformMatrix4(mProjectionLocation, 1, false, projection);
    }

    private static int CreateShaderProgram()
    {
        var vertexShader = CompileShader(
            ShaderType.VertexShader,
            """
            #version 330 core
            layout (location = 0) in vec2 in_position;
            layout (location = 1) in vec2 in_uv;
            layout (location = 2) in vec4 in_color;
            uniform mat4 projection_matrix;
            out vec2 frag_uv;
            out vec4 frag_color;
            void main()
            {
                frag_uv = in_uv;
                frag_color = in_color;
                gl_Position = projection_matrix * vec4(in_position.xy, 0.0, 1.0);
            }
            """);
        var fragmentShader = CompileShader(
            ShaderType.FragmentShader,
            """
            #version 330 core
            in vec2 frag_uv;
            in vec4 frag_color;
            uniform sampler2D in_font_texture;
            out vec4 output_color;
            void main()
            {
                output_color = frag_color * texture(in_font_texture, frag_uv.st);
            }
            """);

        var program = GL.CreateProgram();
        GL.AttachShader(program, vertexShader);
        GL.AttachShader(program, fragmentShader);
        GL.LinkProgram(program);
        GL.GetProgram(program, GetProgramParameterName.LinkStatus, out var status);
        if (status == 0)
        {
            throw new InvalidOperationException(GL.GetProgramInfoLog(program));
        }

        GL.DetachShader(program, vertexShader);
        GL.DetachShader(program, fragmentShader);
        GL.DeleteShader(vertexShader);
        GL.DeleteShader(fragmentShader);
        GL.UseProgram(program);
        GL.Uniform1(GL.GetUniformLocation(program, "in_font_texture"), 0);
        return program;
    }

    private static int CompileShader(ShaderType type, string source)
    {
        var shader = GL.CreateShader(type);
        GL.ShaderSource(shader, source);
        GL.CompileShader(shader);
        GL.GetShader(shader, ShaderParameter.CompileStatus, out var status);
        if (status == 0)
        {
            throw new InvalidOperationException(GL.GetShaderInfoLog(shader));
        }

        return shader;
    }
}
