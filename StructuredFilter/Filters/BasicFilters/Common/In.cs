using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using StructuredFilter.Filters.Common;
using StructuredFilter.Filters.Common.FilterTypes;

namespace StructuredFilter.Filters.BasicFilters.Common;

[FilterLabel("属于")]
[FilterKey("$in")]
internal class InFilter<T>: Filter<T>
{
    public override void Valid(JsonElement element)
    {
        element.AssertIsValidArray(this);
    }

    public override async Task LazyMatchAsync(JsonElement element, IFilter<T>.MatchTargetGetter targetGetter, Dictionary<string, object>? args)
    {
        var (matchTarget, isExists) = await targetGetter(args);
        if (!isExists)
        {
            this.ThrowMatchTargetGetFailedException(args);
        }

        if (element.EnumerateArray().Any(e => e.MatchEq(this, matchTarget)))
        {
            return;
        }

        this.ThrowNotMatchException(matchTarget, element.ToString());
    }

    public override void Match(JsonElement element, T matchTarget)
    {
        if (element.EnumerateArray().Any(e => e.MatchEq(this, matchTarget)))
        {
            return;
        }

        this.ThrowNotMatchException(matchTarget, element.ToString());
    }
}