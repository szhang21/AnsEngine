namespace Engine.Scripting;

public sealed record ScriptBindingDescription(
    string ObjectId,
    string ObjectName,
    IScriptSelfTransform SelfTransform,
    string ScriptId,
    IReadOnlyDictionary<string, ScriptPropertyValue> Properties);
