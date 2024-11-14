using System;

namespace StructuredFilter.Filters.Common;

// TODO: 支持本地化
[AttributeUsage(AttributeTargets.Class)]
public class FilterLabel : Attribute
{
    public string Label { get; }

    public FilterLabel(string label)
    {
        Label = label;
    }
}

[AttributeUsage(AttributeTargets.Class)]
public class FilterKey : Attribute
{
    public string Key { get; }

    public FilterKey(string key)
    {
        Key = key;
    }
}

[AttributeUsage(AttributeTargets.Class)]
public class FilterType : Attribute
{
    public string Type { get; }

    public FilterType(string type)
    {
        Type = type;
    }
}
