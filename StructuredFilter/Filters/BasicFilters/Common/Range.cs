using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using StructuredFilter.Filters.Common;
using StructuredFilter.Filters.Common.FilterTypes;

namespace StructuredFilter.Filters.BasicFilters.Common;

[FilterLabel("在此范围（包含两端值）")]
[FilterKey("$range")]
internal class RangeFilter<T>: Filter<T>
{
    public override void Valid(JsonElement element)
    {
        element.AssertIsValidRange(this);
    }

    public override async Task LazyMatchAsync(JsonElement element, IFilter<T>.MatchTargetGetter targetGetter, Dictionary<string, object>? args)
    {
        var (matchTarget, isExists) = await targetGetter(args);
        if (!isExists)
        {
            this.ThrowMatchTargetGetFailedException(args);
        }

        if (element[1].CompareTo(this, matchTarget) >= 0 && element[0].CompareTo(this, matchTarget) <= 0)
        {
            return;
        }

        this.ThrowNotMatchException(matchTarget, element.ToString());
    }

    public override void Match(JsonElement element, T matchTarget)
    {
        if (element[1].CompareTo(this, matchTarget) >= 0 && element[0].CompareTo(this, matchTarget) <= 0)
        {
            return;
        }

        this.ThrowNotMatchException(matchTarget, element.ToString());
    }
}