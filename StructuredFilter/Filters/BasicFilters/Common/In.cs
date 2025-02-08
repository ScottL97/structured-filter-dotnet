using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using StructuredFilter.Filters.Common;
using StructuredFilter.Filters.Common.FilterTypes;
using StructuredFilter.Utils;

namespace StructuredFilter.Filters.BasicFilters.Common;

[FilterLabel("属于")]
[FilterKey("$in")]
internal class InFilter<T>: Filter<T>
{
    public override void Valid(JsonElement element)
    {
        element.AssertIsValidArray(this);
    }

    public override async Task LazyMatchAsync(JsonElement element, LazyObjectGetter<T> matchTargetGetter)
    {
        try
        {
            var matchTarget = await matchTargetGetter.GetAsync();
            if (element.EnumerateArray().Any(e => e.MatchEq(this, matchTarget)))
            {
                return;
            }

            this.ThrowNotMatchException(matchTarget, element.ToString());
        }
        catch (LazyObjectGetException)
        {
            this.ThrowMatchTargetGetFailedException(matchTargetGetter.Args);
        }
    }

    public override Task MatchAsync(JsonElement element, T matchTarget)
    {
        if (element.EnumerateArray().Any(e => e.MatchEq(this, matchTarget)))
        {
            return Task.CompletedTask;
        }

        this.ThrowNotMatchException(matchTarget, element.ToString());
        return Task.CompletedTask;
    }
}