using System.Collections.Generic;
using StructuredFilter.Builder.Builders.BasicBuilders;

namespace StructuredFilter.Builder.Builders.SceneBuilders;

public class DoubleSceneBuilder : SceneBuilder
{
    public DoubleSceneBuilder In(IEnumerable<double> values)
    {
        // TODO: 如果 BasicBuilder 无变化，这里创建新对象比较浪费
        SetBasicBuilder(new DoubleInBuilder(values));
        return this;
    }

    public DoubleSceneBuilder Eq(double value)
    {
        SetBasicBuilder(new DoubleEqBuilder(value));
        return this;
    }
    
    public DoubleSceneBuilder Ne(double value)
    {
        SetBasicBuilder(new DoubleNeBuilder(value));
        return this;
    }
    
    public DoubleSceneBuilder Gt(double value)
    {
        SetBasicBuilder(new DoubleGtBuilder(value));
        return this;
    }
    
    public DoubleSceneBuilder Ge(double value)
    {
        SetBasicBuilder(new DoubleGeBuilder(value));
        return this;
    }
    
    public DoubleSceneBuilder Lt(double value)
    {
        SetBasicBuilder(new DoubleLtBuilder(value));
        return this;
    }
    
    public DoubleSceneBuilder Le(double value)
    {
        SetBasicBuilder(new DoubleLeBuilder(value));
        return this;
    }
    
    public DoubleSceneBuilder Range(double leftValue, double rightValue)
    {
        SetBasicBuilder(new DoubleRangeBuilder(leftValue, rightValue));
        return this;
    }
}
