namespace Engine.Scripting;

public sealed record ScriptUpdateComponentBindingDescription(
    string ObjectId,
    string ObjectName,
    string ScriptId,
    IReadOnlyDictionary<string, ScriptPropertyValue> Properties);
