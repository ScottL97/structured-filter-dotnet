using System;
using System.Text.Json;

namespace StructuredFilter.Filters.Common;

public abstract class Filter<T>
{
    protected string? KeyOverride { get; set; }
    protected string? LabelOverride { get; set; }
    protected string? BasicTypeOverride { get; set; }

    public string GetKey()
    {
        if (!string.IsNullOrEmpty(KeyOverride))
        {
            return KeyOverride;
        }

        var keyAttribute = (FilterKey?)Attribute.GetCustomAttribute(GetType(), typeof(FilterKey));
        if (keyAttribute == null)
        {
            throw new FilterException(FilterStatusCode.Invalid, $"type {GetType()} FilterKey Attribute is missing", $"<{GetType()}>");
        }
        return keyAttribute.Key;
    }

    public string GetLabel()
    {
        if (!string.IsNullOrEmpty(LabelOverride))
        {
            return LabelOverride;
        }

        var labelAttribute = (FilterLabel?)Attribute.GetCustomAttribute(GetType(), typeof(FilterLabel));
        return labelAttribute != null ? labelAttribute.Label : "UNKNOWN_LABEL";
    }

    public string GetBasicType()
    {
        if (!string.IsNullOrEmpty(BasicTypeOverride))
        {
            return BasicTypeOverride;
        }

        var typeAttribute = (FilterType?)Attribute.GetCustomAttribute(GetType(), typeof(FilterType));
        if (typeAttribute == null)
        {
            throw new FilterException(FilterStatusCode.Invalid, $"type {GetType()} FilterType Attribute is missing", $"<{GetType()}>");
        }
        return typeAttribute.Type;
    }

    public abstract void Valid(JsonElement element);
}
