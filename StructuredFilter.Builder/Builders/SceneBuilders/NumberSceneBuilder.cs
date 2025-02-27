using System.Collections.Generic;
using StructuredFilter.Builder.Builders.BasicBuilders;

namespace StructuredFilter.Builder.Builders.SceneBuilders;

public class NumberSceneBuilder : SceneBuilder
{
    public NumberSceneBuilder In(IEnumerable<double> values)
    {
        // TODO: 如果 BasicBuilder 无变化，这里创建新对象比较浪费
        SetBasicBuilder(new NumberInBuilder(values));
        return this;
    }

    public NumberSceneBuilder Eq(double value)
    {
        SetBasicBuilder(new NumberEqBuilder(value));
        return this;
    }
    
    public NumberSceneBuilder Ne(double value)
    {
        SetBasicBuilder(new NumberNeBuilder(value));
        return this;
    }
    
    public NumberSceneBuilder Gt(double value)
    {
        SetBasicBuilder(new NumberGtBuilder(value));
        return this;
    }
    
    public NumberSceneBuilder Ge(double value)
    {
        SetBasicBuilder(new NumberGeBuilder(value));
        return this;
    }
    
    public NumberSceneBuilder Lt(double value)
    {
        SetBasicBuilder(new NumberLtBuilder(value));
        return this;
    }
    
    public NumberSceneBuilder Le(double value)
    {
        SetBasicBuilder(new NumberLeBuilder(value));
        return this;
    }
    
    public NumberSceneBuilder Range(double leftValue, double rightValue)
    {
        SetBasicBuilder(new NumberRangeBuilder(leftValue, rightValue));
        return this;
    }
}