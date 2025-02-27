using System;
using System.Text;

namespace StructuredFilter.Builder.Builders.Common;

public interface IFilterBuilder
{
    string Build();
}

public abstract class FilterBuilder : IFilterBuilder
{
    private readonly StringBuilder _sb = new ();

    public string Build()
    {
        _sb.Clear();
        _sb.Append("{");
        _sb.Append("\"");
        _sb.Append(GetFilterBuilderKey());
        _sb.Append("\"");
        _sb.Append(":");
        BuildValue(_sb);
        _sb.Append("}");
        return _sb.ToString();
    }

    private string GetFilterBuilderKey()
    {
        var keyAttribute = (FilterBuilderKey?)Attribute.GetCustomAttribute(GetType(), typeof(FilterBuilderKey));
        if (keyAttribute == null)
        {
            throw new FilterBuilderException($"FilterBuilderKey Attribute is required on the class {GetType().FullName}");
        }
        return keyAttribute.Key;
    }

    protected abstract void BuildValue(StringBuilder sb);
}
