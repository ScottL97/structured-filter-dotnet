using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using StructuredFilter.Filters.Common;
using StructuredFilter.Filters.Common.FilterTypes;

namespace StructuredFilter.Filters.BasicFilters.Common;

[FilterLabel("大于等于")]
[FilterKey("$ge")]
internal class GeFilter<T>: Filter<T>
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

        if (element.MatchGe(this, matchTarget))
        {
            return;
        }

        this.ThrowNotMatchException(matchTarget, element.ToString());
    }

    public override void Match(JsonElement element, T matchTarget)
    {
        if (element.MatchGe(this, matchTarget))
        {
            return;
        }

        this.ThrowNotMatchException(matchTarget, element.ToString());
    }
}
