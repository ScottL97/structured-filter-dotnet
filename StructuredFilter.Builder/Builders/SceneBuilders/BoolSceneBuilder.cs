using StructuredFilter.Builder.Builders.BasicBuilders;

namespace StructuredFilter.Builder.Builders.SceneBuilders;

public class BoolSceneBuilder : SceneBuilder
{
    public BoolSceneBuilder Eq(bool value)
    {
        SetBasicBuilder(new BoolEqBuilder(value));
        return this;
    }

    public BoolSceneBuilder Ne(bool value)
    {
        SetBasicBuilder(new BoolNeBuilder(value));
        return this;
    }
}
