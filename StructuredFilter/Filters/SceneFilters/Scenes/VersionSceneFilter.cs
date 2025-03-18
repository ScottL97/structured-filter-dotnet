using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using StructuredFilter.Filters.Common;
using StructuredFilter.Filters.Common.FilterTypes;
using StructuredFilter.Utils;

namespace StructuredFilter.Filters.SceneFilters.Scenes;

[FilterType(FilterBasicType.Version)]
public abstract class VersionSceneFilter<T>(FilterFactory<T> filterFactory, VersionSceneFilter<T>.VersionValueGetter versionValueGetter, IFilterResultCache<T>? cache=null) : SceneFilter<T>(cache)
{
    protected delegate Task<Version> VersionValueGetter(T? matchTarget);

    public override void Valid(JsonElement filterElement)
    {
        try
        {
            filterElement.AssertIsValidObject(this, property =>
            {
                var filter = filterFactory.VersionFilterFactory.Get(property.Name);
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
            var filter = filterFactory.VersionFilterFactory.Get(filterKv.Key);
            await filter.LazyMatchAsync(filterKv.Value, new LazyObjectGetter<Version>(async _ =>
            {
                var matchTarget = await matchTargetGetter.GetAsync();
                return (await versionValueGetter(matchTarget), true);
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
            var filter = filterFactory.VersionFilterFactory.Get(filterKv.Key);
            await filter.MatchAsync(filterKv.Value, await versionValueGetter(matchTarget));
        }
        catch (FilterException e)
        {
            throw e.PrependFailedKey(GetKey());
        }
    }
}

public class DynamicVersionSceneFilter<T> : VersionSceneFilter<T>
{
    public DynamicVersionSceneFilter(FilterFactory<T> filterFactory,
        FilterOption<T>.GetDynamicVersionSceneFilterValueAsync dynamicSceneFilterValueGetter,
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
