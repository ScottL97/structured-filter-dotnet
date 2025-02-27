using System.Collections.Generic;
using StructuredFilter.Builder.Builders.BasicBuilders;

namespace StructuredFilter.Builder.Builders.SceneBuilders;

public class StringSceneBuilder : SceneBuilder
{
    public StringSceneBuilder In(IEnumerable<string> values)
    {
        SetBasicBuilder(new StringInBuilder(values));
        return this;
    }

    public StringSceneBuilder Eq(string value)
    {
        SetBasicBuilder(new StringEqBuilder(value));
        return this;
    }
    
    public StringSceneBuilder Ne(string value)
    {
        SetBasicBuilder(new StringNeBuilder(value));
        return this;
    }
    
    public StringSceneBuilder Range(string leftValue, string rightValue)
    {
        SetBasicBuilder(new StringRangeBuilder(leftValue, rightValue));
        return this;
    }

    public StringSceneBuilder Regex(string value)
    {
        SetBasicBuilder(new StringRegexBuilder(value));
        return this;
    }
}