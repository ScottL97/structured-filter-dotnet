using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using StructuredFilter.Filters.Common;
using StructuredFilter.Filters.Common.FilterTypes;

namespace StructuredFilter.Filters.BasicFilters.Common;

[FilterLabel("大于")]
[FilterKey("$gt")]
internal class GtFilter<T>: Filter<T>
{
    public override void Valid(JsonElement element)
    {
        element.AssertIsRightElementType(this);
    }

    public override async Task LazyMatchAsync(JsonElement element, IFilter<T>.MatchTargetGetter targetGetter, Dictionary<string, object>? args)
    {
        var (matchTarget, ok) = await targetGetter(args);
        if (!ok)
        {
            this.ThrowMatchTargetGetFailedException(args);
        }

        if (element.MatchGt(this, matchTarget))
        {
            return;
        }

        this.ThrowNotMatchException(matchTarget, element.ToString());
    }

    public override void Match(JsonElement element, T matchTarget)
    {
        if (element.MatchGt(this, matchTarget))
        {
            return;
        }

        this.ThrowNotMatchException(matchTarget, element.ToString());
    }
}