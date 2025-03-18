using System.Text.Json;
using System.Threading.Tasks;
using StructuredFilter.Filters.Common;
using StructuredFilter.Filters.Common.FilterTypes;
using StructuredFilter.Utils;

namespace StructuredFilter.Filters.BasicFilters.Common;

[FilterLabel("小于")]
[FilterKey("$lt")]
internal class LtFilter<T>: Filter<T>, IBasicFilter<T>
{
    public FilterException? Valid(JsonElement element)
    {
        return element.AssertIsRightElementType(this);
    }

    public async Task<FilterException?> LazyMatchAsync(JsonElement element, LazyObjectGetter<T> matchTargetGetter)
    {
        try
        {
            var matchTarget = await matchTargetGetter.GetAsync();
            if (element.MatchLt(this, matchTarget))
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

    public FilterException? Match(JsonElement element, T matchTarget)
    {
        if (element.MatchLt(this, matchTarget))
        {
            return null;
        }

        return this.CreateNotMatchException(matchTarget, element.ToString());
    }
}