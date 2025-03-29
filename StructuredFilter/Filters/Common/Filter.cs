using System;

namespace StructuredFilter.Filters.Common;

public abstract class Filter<T>
{
    protected string? KeyOverride { get; set; }
    protected string? LabelOverride { get; set; }
    protected string? BasicTypeOverride { get; set; }
    
    private string? KeyInAttribute { get; set; }
    private string? LabelInAttribute { get; set; }
    private string? BasicTypeInAttribute { get; set; }

    public string GetKey()
    {
        if (!string.IsNullOrEmpty(KeyOverride))
        {
            return KeyOverride;
        }

        if (!string.IsNullOrEmpty(KeyInAttribute))
        {
            return KeyInAttribute;
        }

        var keyAttribute = (FilterKey?)Attribute.GetCustomAttribute(GetType(), typeof(FilterKey));
        if (keyAttribute == null)
        {
            throw new FilterException(FilterStatusCode.Invalid, $"type {GetType()} FilterKey Attribute is missing", $"<{GetType()}>");
        }
        KeyInAttribute = keyAttribute.Key;
        return KeyInAttribute;
    }

    public string GetLabel()
    {
        if (!string.IsNullOrEmpty(LabelOverride))
        {
            return LabelOverride;
        }
        
        if (!string.IsNullOrEmpty(LabelInAttribute))
        {
            return LabelInAttribute;
        }

        var labelAttribute = (FilterLabel?)Attribute.GetCustomAttribute(GetType(), typeof(FilterLabel));
        LabelInAttribute = labelAttribute != null ? labelAttribute.Label : "UNKNOWN_LABEL";
        return LabelInAttribute;
    }

    public string GetBasicType()
    {
        if (!string.IsNullOrEmpty(BasicTypeOverride))
        {
            return BasicTypeOverride;
        }

        if (!string.IsNullOrEmpty(BasicTypeInAttribute))
        {
            return BasicTypeInAttribute;
        }

        var typeAttribute = (FilterType?)Attribute.GetCustomAttribute(GetType(), typeof(FilterType));
        if (typeAttribute == null)
        {
            throw new FilterException(FilterStatusCode.Invalid, $"type {GetType()} FilterType Attribute is missing", $"<{GetType()}>");
        }
        BasicTypeInAttribute = typeAttribute.Type;
        return BasicTypeInAttribute;
    }
}
