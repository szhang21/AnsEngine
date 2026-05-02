namespace Engine.Scripting;

public readonly record struct ScriptPropertyValue
{
    private ScriptPropertyValue(double? number, bool? boolean, string? text)
    {
        Number = number;
        Boolean = boolean;
        Text = text;
    }

    public double? Number { get; }

    public bool? Boolean { get; }

    public string? Text { get; }

    public bool IsNumber => Number is not null;

    public bool IsBoolean => Boolean is not null;

    public bool IsString => Text is not null;

    public static ScriptPropertyValue FromNumber(double value)
    {
        return new ScriptPropertyValue(value, null, null);
    }

    public static ScriptPropertyValue FromBoolean(bool value)
    {
        return new ScriptPropertyValue(null, value, null);
    }

    public static ScriptPropertyValue FromString(string value)
    {
        return new ScriptPropertyValue(null, null, value ?? string.Empty);
    }
}
