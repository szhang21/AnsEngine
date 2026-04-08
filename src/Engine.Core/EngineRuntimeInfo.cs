namespace Engine.Core;

public sealed class EngineRuntimeInfo
{
    public EngineRuntimeInfo(string engineName, string version)
    {
        EngineName = engineName;
        Version = version;
    }

    public string EngineName { get; }

    public string Version { get; }
}
