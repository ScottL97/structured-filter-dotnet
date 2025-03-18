using System.Text.Json;
using System.Threading.Tasks;
using StructuredFilter.Filters.Common;
using StructuredFilter.Filters.Common.FilterTypes;
using StructuredFilter.Utils;

namespace StructuredFilter.Filters.BasicFilters.Common;

[FilterLabel("在此范围（包含两端值）")]
[FilterKey("$range")]
internal class RangeFilter<T>: Filter<T>, IBasicFilter<T>
{
    public override void Valid(JsonElement element)
    {
        element.AssertIsValidRange(this);
    }

    public async Task LazyMatchAsync(JsonElement element, LazyObjectGetter<T> matchTargetGetter)
    {
        try
        {
            var matchTarget = await matchTargetGetter.GetAsync();
            if (element[1].CompareTo(this, matchTarget) >= 0 && element[0].CompareTo(this, matchTarget) <= 0)
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

    public Task MatchAsync(JsonElement element, T matchTarget)
    {
        if (element[1].CompareTo(this, matchTarget) >= 0 && element[0].CompareTo(this, matchTarget) <= 0)
        {
            return Task.CompletedTask;
        }

        this.ThrowNotMatchException(matchTarget, element.ToString());
        return Task.CompletedTask;
    }
}