namespace Engine.Scripting;

public static class ScriptPropertyReader
{
    public static double RequireNumber(ScriptContext context, string propertyName)
    {
        ArgumentNullException.ThrowIfNull(context);
        ValidatePropertyName(propertyName);

        var value = GetRequiredValue(context, propertyName);
        if (!value.IsNumber || !value.Number.HasValue || !double.IsFinite(value.Number.Value))
        {
            throw new InvalidOperationException(
                $"Script on object '{context.ObjectId}' requires finite numeric property '{propertyName}'.");
        }

        return value.Number.Value;
    }

    public static bool RequireBoolean(ScriptContext context, string propertyName)
    {
        ArgumentNullException.ThrowIfNull(context);
        ValidatePropertyName(propertyName);

        var value = GetRequiredValue(context, propertyName);
        if (!value.IsBoolean || !value.Boolean.HasValue)
        {
            throw new InvalidOperationException(
                $"Script on object '{context.ObjectId}' requires boolean property '{propertyName}'.");
        }

        return value.Boolean.Value;
    }

    public static string RequireString(ScriptContext context, string propertyName)
    {
        ArgumentNullException.ThrowIfNull(context);
        ValidatePropertyName(propertyName);

        var value = GetRequiredValue(context, propertyName);
        if (!value.IsString)
        {
            throw new InvalidOperationException(
                $"Script on object '{context.ObjectId}' requires string property '{propertyName}'.");
        }

        return value.Text ?? string.Empty;
    }

    private static ScriptPropertyValue GetRequiredValue(ScriptContext context, string propertyName)
    {
        if (!context.Properties.TryGetValue(propertyName, out var value))
        {
            throw new InvalidOperationException(
                $"Script on object '{context.ObjectId}' is missing required property '{propertyName}'.");
        }

        return value;
    }

    private static void ValidatePropertyName(string propertyName)
    {
        if (string.IsNullOrWhiteSpace(propertyName))
        {
            throw new ArgumentException("Property name is required.", nameof(propertyName));
        }
    }
}
