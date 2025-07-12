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
    public FilterException? Valid(JsonElement element)
    {
        return element.AssertIsValidRange(this);
    }

    public async ValueTask<FilterException?> LazyMatchAsync(FilterValue filterValue, LazyObjectGetter<T> matchTargetGetter)
    {
        try
        {
            var matchTarget = await matchTargetGetter.GetAsync();
            var (rightCompareResult, checkResult1) = filterValue[1].CompareTo(this, matchTarget);
            if (checkResult1 is not null)
            {
                return checkResult1;
            }
            var (leftCompareResult, checkResult2) = filterValue[0].CompareTo(this, matchTarget);
            if (checkResult2 is not null)
            {
                return checkResult2;
            }

            if (rightCompareResult >= 0 && leftCompareResult <= 0)
            {
                return null;
            }

            return this.CreateNotMatchException(matchTarget, filterValue.ToString());
        }
        catch (LazyObjectGetException)
        {
            return this.CreateMatchTargetGetFailedException(matchTargetGetter.Args);
        }
    }

    public FilterException? Match(FilterValue filterValue, T matchTarget)
    {
        var (rightCompareResult, checkResult1) = filterValue[1].CompareTo(this, matchTarget);
        if (checkResult1 is not null)
        {
            return checkResult1;
        }
        var (leftCompareResult, checkResult2) = filterValue[0].CompareTo(this, matchTarget);
        if (checkResult2 is not null)
        {
            return checkResult2;
        }

        if (rightCompareResult >= 0 && leftCompareResult <= 0)
        {
            return null;
        }

        return this.CreateNotMatchException(matchTarget, filterValue.ToString());
    }
}