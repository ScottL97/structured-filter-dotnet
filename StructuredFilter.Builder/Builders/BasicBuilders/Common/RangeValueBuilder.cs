using System;
using StructuredFilter.Builder.Builders.Common;

namespace StructuredFilter.Builder.Builders.BasicBuilders.Common;

public abstract class RangeValueBuilder<T>(T leftValue, T rightValue) : FilterBuilder, IBasicBuilder
{
    protected override void BuildValue(System.Text.StringBuilder sb)
    {
        if (typeof(T) == typeof(string) || typeof(T) == typeof(Version))
        {
            sb.Append('[');
            sb.Append('"');
            sb.Append(leftValue);
            sb.Append('"');
            sb.Append(',');
            sb.Append('"');
            sb.Append(rightValue);
            sb.Append('"');
            sb.Append(']');

            return;
        }

        sb.Append('[');
        sb.Append(leftValue);
        sb.Append(',');
        sb.Append(rightValue);
        sb.Append(']');
    }
}