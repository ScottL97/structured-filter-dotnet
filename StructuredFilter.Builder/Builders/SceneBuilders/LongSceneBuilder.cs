using System.Collections.Generic;
using StructuredFilter.Builder.Builders.BasicBuilders;

namespace StructuredFilter.Builder.Builders.SceneBuilders;

public class LongSceneBuilder : SceneBuilder
{
    public LongSceneBuilder In(IEnumerable<long> values)
    {
        // TODO: 如果 BasicBuilder 无变化，这里创建新对象比较浪费
        SetBasicBuilder(new LongInBuilder(values));
        return this;
    }

    public LongSceneBuilder Eq(long value)
    {
        SetBasicBuilder(new LongEqBuilder(value));
        return this;
    }
    
    public LongSceneBuilder Ne(long value)
    {
        SetBasicBuilder(new LongNeBuilder(value));
        return this;
    }
    
    public LongSceneBuilder Gt(long value)
    {
        SetBasicBuilder(new LongGtBuilder(value));
        return this;
    }
    
    public LongSceneBuilder Ge(long value)
    {
        SetBasicBuilder(new LongGeBuilder(value));
        return this;
    }
    
    public LongSceneBuilder Lt(long value)
    {
        SetBasicBuilder(new LongLtBuilder(value));
        return this;
    }
    
    public LongSceneBuilder Le(long value)
    {
        SetBasicBuilder(new LongLeBuilder(value));
        return this;
    }
    
    public LongSceneBuilder Range(long leftValue, long rightValue)
    {
        SetBasicBuilder(new LongRangeBuilder(leftValue, rightValue));
        return this;
    }
}
