using System;

namespace StructuredFilter.Builder.Builders.Common;

[AttributeUsage(AttributeTargets.Class)]
public class FilterBuilderKey : Attribute
{
    public string Key { get; }

    public FilterBuilderKey(string key)
    {
        Key = key;
    }
}
