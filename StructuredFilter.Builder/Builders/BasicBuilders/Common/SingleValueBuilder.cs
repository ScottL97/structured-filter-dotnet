using System;
using StructuredFilter.Builder.Builders.Common;

namespace StructuredFilter.Builder.Builders.BasicBuilders.Common;

public abstract class SingleValueBuilder<T>(T value) : FilterBuilder, IBasicBuilder
{
    private const string NullValue = "null";

    protected override void BuildValue(System.Text.StringBuilder sb)
    {
        if (value is null)
        {
            sb.Append(NullValue);
            return;
        }

        if (typeof(T) == typeof(string) || typeof(T) == typeof(Version))
        {
            sb.Append("\"");
            sb.Append(value);
            sb.Append("\"");
            return;
        }

        if (typeof(T) == typeof(bool))
        {
            sb.Append(value.ToString()!.ToLower());
            return;
        }

        sb.Append(value);
    }
}
