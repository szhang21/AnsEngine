namespace Engine.Scripting;

public sealed class ScriptContext
{
    public ScriptContext(
        string objectId,
        string objectName,
        IScriptSelfTransform selfTransform,
        IReadOnlyDictionary<string, ScriptPropertyValue> properties,
        double deltaSeconds,
        double totalSeconds)
    {
        ObjectId = string.IsNullOrWhiteSpace(objectId)
            ? throw new ArgumentException("Script object id is required.", nameof(objectId))
            : objectId;
        ObjectName = objectName ?? string.Empty;
        SelfTransform = selfTransform ?? throw new ArgumentNullException(nameof(selfTransform));
        Properties = properties ?? throw new ArgumentNullException(nameof(properties));
        DeltaSeconds = deltaSeconds;
        TotalSeconds = totalSeconds;
    }

    public string ObjectId { get; }

    public string ObjectName { get; }

    public IScriptSelfTransform SelfTransform { get; }

    public IReadOnlyDictionary<string, ScriptPropertyValue> Properties { get; }

    public double DeltaSeconds { get; }

    public double TotalSeconds { get; }

    public ScriptContext WithTiming(double deltaSeconds, double totalSeconds)
    {
        return new ScriptContext(ObjectId, ObjectName, SelfTransform, Properties, deltaSeconds, totalSeconds);
    }
}
