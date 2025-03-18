using System.Text.Json;
using System.Threading.Tasks;
using StructuredFilter.Filters.Common;
using StructuredFilter.Filters.Common.FilterTypes;
using StructuredFilter.Utils;

namespace StructuredFilter.Filters.BasicFilters.Common;

[FilterLabel("大于等于")]
[FilterKey("$ge")]
internal class GeFilter<T>: Filter<T>, IBasicFilter<T>
{
    public override void Valid(JsonElement element)
    {
        element.AssertIsRightElementType(this);
    }

    public async Task LazyMatchAsync(JsonElement element, LazyObjectGetter<T> matchTargetGetter)
    {
        try
        {
            var matchTarget = await matchTargetGetter.GetAsync();
            if (element.MatchGe(this, matchTarget))
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
        if (element.MatchGe(this, matchTarget))
        {
            return Task.CompletedTask;
        }

        this.ThrowNotMatchException(matchTarget, element.ToString());
        return Task.CompletedTask;
    }
}
