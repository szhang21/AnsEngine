namespace Engine.Scripting;

public enum ScriptFailureKind
{
    None = 0,
    DuplicateScriptId,
    MissingScriptId,
    ScriptFactoryFailed,
    InvalidProperty,
    ScriptException
}
