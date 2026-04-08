namespace Engine.App;

public interface IApplication
{
    int Run();
}

public interface IRuntimeBootstrap
{
    IApplication Build();
}
