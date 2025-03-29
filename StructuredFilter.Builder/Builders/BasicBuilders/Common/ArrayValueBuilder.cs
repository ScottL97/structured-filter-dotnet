using System;
using System.Collections.Generic;
using StructuredFilter.Builder.Builders.Common;

namespace StructuredFilter.Builder.Builders.BasicBuilders.Common;

public abstract class ArrayValueBuilder<T>(IEnumerable<T> values) : FilterBuilder, IBasicBuilder
{
    protected override void BuildValue(System.Text.StringBuilder sb)
    {
        sb.Append('[');
        if (typeof(T) == typeof(string) || typeof(T) == typeof(Version))
        {
            foreach (var value in values)
            {
                sb.Append('"');
                sb.Append(value);
                sb.Append('"');
                sb.Append(',');
            }
            sb.Remove(sb.Length - 1, 1);
            sb.Append(']');

            return;
        }

        foreach (var value in values)
        {
            sb.Append(value);
            sb.Append(',');
        }
        sb.Remove(sb.Length - 1, 1);
        sb.Append(']');
    }
}