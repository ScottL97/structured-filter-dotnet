﻿using System.Text.Json;
using System.Threading.Tasks;
using StructuredFilter.Filters.Common;
using StructuredFilter.Filters.Common.FilterTypes;
using StructuredFilter.Utils;

namespace StructuredFilter.Filters.BasicFilters.Common;

[FilterLabel("不等于")]
[FilterKey("$ne")]
internal class NeFilter<T>: Filter<T>, IBasicFilter<T>
{
    public FilterException? Valid(JsonElement element)
    {
        return element.AssertIsRightElementType(this);
    }

    public async ValueTask<FilterException?> LazyMatchAsync(FilterValue filterValue, LazyObjectGetter<T> matchTargetGetter)
    {
        try
        {
            var matchTarget = await matchTargetGetter.GetAsync();
            if (filterValue.MatchNe(this, matchTarget))
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
        if (filterValue.MatchNe(this, matchTarget))
        {
            return null;
        }

        return this.CreateNotMatchException(matchTarget, filterValue.ToString());
    }
}