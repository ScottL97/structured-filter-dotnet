using StructuredFilter.Builder.Builders.BasicBuilders;
using StructuredFilter.Builder.Builders.Common;

namespace StructuredFilter.Builder.Builders.SceneBuilders;

public class SceneBuilder : FilterBuilder
{
    private IBasicBuilder? _basicBuilder;

    protected override void BuildValue(System.Text.StringBuilder sb)
    {
        if (_basicBuilder is null)
        {
            throw new FilterBuilderException("Cannot build a SceneBuilder without BasicBuilder set");
        }
        sb.Append(_basicBuilder.Build());
    }

    protected void SetBasicBuilder(IBasicBuilder builder)
    {
        _basicBuilder = builder;
    }
}
