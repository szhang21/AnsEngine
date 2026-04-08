namespace Engine.Render;

public interface IRenderer
{
    void Initialize();

    void RenderFrame();

    void Shutdown();
}

public interface IShaderProgram
{
    string Name { get; }

    void Bind();
}
