namespace Engine.Scripting;

public sealed record ScriptBindingDescription(
    string ObjectId,
    string ObjectName,
    IScriptSelfObject Self,
    string ScriptId,
    IReadOnlyDictionary<string, ScriptPropertyValue> Properties);
