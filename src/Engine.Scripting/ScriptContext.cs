namespace Engine.Scripting;

public sealed class ScriptContext
{
    public ScriptContext(
        string objectId,
        string objectName,
        IScriptSelfObject self,
        IReadOnlyDictionary<string, ScriptPropertyValue> properties,
        double deltaSeconds,
        double totalSeconds)
        : this(objectId, objectName, self, properties, deltaSeconds, totalSeconds, ScriptInputSnapshot.Empty)
    {
    }

    public ScriptContext(
        string objectId,
        string objectName,
        IScriptSelfObject self,
        IReadOnlyDictionary<string, ScriptPropertyValue> properties,
        double deltaSeconds,
        double totalSeconds,
        ScriptInputSnapshot input)
    {
        ObjectId = string.IsNullOrWhiteSpace(objectId)
            ? throw new ArgumentException("Script object id is required.", nameof(objectId))
            : objectId;
        ObjectName = objectName ?? string.Empty;
        Self = self ?? throw new ArgumentNullException(nameof(self));
        Properties = properties ?? throw new ArgumentNullException(nameof(properties));
        DeltaSeconds = deltaSeconds;
        TotalSeconds = totalSeconds;
        Input = input;
    }

    public string ObjectId { get; }

    public string ObjectName { get; }

    public IScriptSelfObject Self { get; }

    public IReadOnlyDictionary<string, ScriptPropertyValue> Properties { get; }

    public double DeltaSeconds { get; }

    public double TotalSeconds { get; }

    public ScriptInputSnapshot Input { get; }

    public ScriptContext WithTiming(double deltaSeconds, double totalSeconds)
    {
        return WithFrame(deltaSeconds, totalSeconds, ScriptInputSnapshot.Empty);
    }

    public ScriptContext WithFrame(double deltaSeconds, double totalSeconds, ScriptInputSnapshot input)
    {
        return new ScriptContext(ObjectId, ObjectName, Self, Properties, deltaSeconds, totalSeconds, input);
    }
}
