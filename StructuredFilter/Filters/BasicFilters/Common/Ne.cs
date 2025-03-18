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

    public async Task<FilterException?> LazyMatchAsync(JsonElement element, LazyObjectGetter<T> matchTargetGetter)
    {
        try
        {
            var matchTarget = await matchTargetGetter.GetAsync();
            if (element.MatchNe(this, matchTarget))
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
        if (element.MatchNe(this, matchTarget))
        {
            return Task.FromResult<FilterException?>(null);
        }

        return Task.FromResult(this.CreateNotMatchException(matchTarget, element.ToString()));
    }
}