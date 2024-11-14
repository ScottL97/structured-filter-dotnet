using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace StructuredFilter.Filters.Common;

public abstract class Filter<T> : IFilter<T>
{
    public string GetKey()
    {
        var keyAttribute = (FilterKey?)Attribute.GetCustomAttribute(GetType(), typeof(FilterKey));
        if (keyAttribute == null)
        {
            throw new FilterException(FilterStatusCode.Invalid, $"type {GetType()} FilterKey Attribute is missing", $"<{GetType()}>");
        }
        return keyAttribute.Key;
    }

    public string GetLabel()
    {
        var labelAttribute = (FilterLabel?)Attribute.GetCustomAttribute(GetType(), typeof(FilterLabel));
        return labelAttribute != null ? labelAttribute.Label : "UNKNOWN_LABEL";
    }

    public string GetBasicType()
    {
        var typeAttribute = (FilterType?)Attribute.GetCustomAttribute(GetType(), typeof(FilterType));
        if (typeAttribute == null)
        {
            throw new FilterException(FilterStatusCode.Invalid, $"type {GetType()} FilterType Attribute is missing", $"<{GetType()}>");
        }
        return typeAttribute.Type;
    }

    public delegate T MatchTargetGetter(Dictionary<string, object>? args);

    public abstract void Valid(JsonElement element);

    public abstract Task LazyMatchAsync(JsonElement element, IFilter<T>.MatchTargetGetter targetGetter, Dictionary<string, object>? args);
    public abstract void Match(JsonElement element, T matchTarget);
}