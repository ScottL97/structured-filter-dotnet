using System.Collections.Generic;
using System.Text;
using StructuredFilter.Builder.Builders.Common;
using StructuredFilter.Builder.Builders.SceneBuilders;

namespace StructuredFilter.Builder.Builders.LogicBuilders;

public class LogicFilterBuilder(List<SceneBuilder> sceneFilterBuilders) : FilterBuilder
{
    protected override void BuildValue(StringBuilder sb)
    {
        sb.Append("[");
        foreach (var sceneFilterBuilder in sceneFilterBuilders)
        {
            sb.Append(sceneFilterBuilder.Build());
            sb.Append(",");
        }
        sb.Remove(sb.Length - 1, 1);
        sb.Append("]");
    }
}

[FilterBuilderKey("$and")]
public class AndLogicFilterBuilder(List<SceneBuilder> sceneFilterBuilders) : LogicFilterBuilder(sceneFilterBuilders);

[FilterBuilderKey("$or")]
public class OrLogicFilterBuilder(List<SceneBuilder> sceneFilterBuilders) : LogicFilterBuilder(sceneFilterBuilders);
