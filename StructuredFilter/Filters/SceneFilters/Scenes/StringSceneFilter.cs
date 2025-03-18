﻿using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using StructuredFilter.Filters.Common;
using StructuredFilter.Filters.Common.FilterTypes;
using StructuredFilter.Utils;

namespace StructuredFilter.Filters.SceneFilters.Scenes;

[FilterType(FilterBasicType.String)]
public abstract class StringSceneFilter<T>(FilterFactory<T> filterFactory, StringSceneFilter<T>.StringValueGetter stringValueGetter, IFilterResultCache<T>? cache=null) : SceneFilter<T>(cache)
{
    protected delegate Task<string> StringValueGetter(T? matchTarget);

    public override void Valid(JsonElement filterElement)
    {
        try
        {
            filterElement.AssertIsValidObject(this, property =>
            {
                var filter = filterFactory.StringFilterFactory.Get(property.Name);
                filter.Valid(property.Value);
            });
        }
        catch (FilterException e)
        {
            throw e.PrependFailedKey(GetKey());
        }
    }

    protected override async Task LazyMatchInternalAsync(FilterKv filterKv, LazyObjectGetter<T> matchTargetGetter)
    {
        try
        {
            var filter = filterFactory.StringFilterFactory.Get(filterKv.Key);
            await filter.LazyMatchAsync(filterKv.Value, new LazyObjectGetter<string>(async _ =>
            {
                var matchTarget = await matchTargetGetter.GetAsync();
                return (await stringValueGetter(matchTarget), true);
            }, matchTargetGetter.Args));
        }
        catch (FilterException e)
        {
            throw e.PrependFailedKey(GetKey());
        }
    }

    protected override async Task MatchInternalAsync(FilterKv filterKv, T matchTarget)
    {
        try
        {
            var filter = filterFactory.StringFilterFactory.Get(filterKv.Key);
            await filter.MatchAsync(filterKv.Value, await stringValueGetter(matchTarget));
        }
        catch (FilterException e)
        {
            throw e.PrependFailedKey(GetKey());
        }
    }
}

public class DynamicStringSceneFilter<T> : StringSceneFilter<T>
{
    public DynamicStringSceneFilter(FilterFactory<T> filterFactory,
        FilterOption<T>.GetDynamicStringSceneFilterValueAsync dynamicSceneFilterValueGetter,
        string key,
        bool cacheable = false,
        string? label = null,
        IFilterResultCache<T>? cache = null) : base(filterFactory,
        matchTarget => dynamicSceneFilterValueGetter(matchTarget, key))
    {
        KeyOverride = key;
        if (SetIsCacheable(cacheable))
        {
            SetCache(cache);
        }
        LabelOverride = label;
    }
}
