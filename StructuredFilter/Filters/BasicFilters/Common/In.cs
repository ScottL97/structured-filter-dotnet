using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using StructuredFilter.Filters.Common;
using StructuredFilter.Filters.Common.FilterTypes;
using StructuredFilter.Utils;

namespace StructuredFilter.Filters.BasicFilters.Common;

[FilterLabel("属于")]
[FilterKey("$in")]
internal class InFilter<T>: Filter<T>, IBasicFilter<T>
{
    public FilterException? Valid(JsonElement element)
    {
        return element.AssertIsValidArray(this);
    }

    public async Task<FilterException?> LazyMatchAsync(JsonElement element, LazyObjectGetter<T> matchTargetGetter)
    {
        try
        {
            var matchTarget = await matchTargetGetter.GetAsync();
            if (element.EnumerateArray().Any(e => e.MatchEq(this, matchTarget)))
            {
                return null;
            }

            return this.CreateNotMatchException(matchTarget, element.ToString());
        }
        catch (LazyObjectGetException)
        {
            return this.CreateMatchTargetGetFailedException(matchTargetGetter.Args);
        }
    }

    public Task<FilterException?> MatchAsync(JsonElement element, T matchTarget)
    {
        if (element.EnumerateArray().Any(e => e.MatchEq(this, matchTarget)))
        {
            return Task.FromResult<FilterException?>(null);
        }

        return Task.FromResult(this.CreateNotMatchException(matchTarget, element.ToString()));
    }
}