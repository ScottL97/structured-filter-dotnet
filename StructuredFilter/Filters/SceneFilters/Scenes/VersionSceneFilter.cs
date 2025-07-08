using System;
using System.Text.Json;
using System.Threading.Tasks;
using StructuredFilter.Filters.Common;
using StructuredFilter.Filters.Common.FilterTypes;
using StructuredFilter.Utils;

namespace StructuredFilter.Filters.SceneFilters.Scenes;

[FilterType(FilterBasicType.Version)]
public abstract class VersionSceneFilter<T>(FilterFactory<T> filterFactory, VersionSceneFilter<T>.VersionValueGetter versionValueGetter, IFilterResultCache<T>? cache=null) : SceneFilter<T>(cache)
{
    protected delegate ValueTask<Version> VersionValueGetter(T? matchTarget);

    public override FilterException? Valid(JsonElement filterElement)
    {
        try
        {
            var checkResult = filterElement.AssertIsValidObject(this, property =>
            {
                var (filter, getResult) = filterFactory.VersionFilterFactory.Get(property.Name);
                return getResult ?? filter.Valid(property.Value);
            });
            return checkResult?.PrependFailedKey(GetKey());
        }
        catch (FilterException e)
        {
            throw e.PrependFailedKey(GetKey());
        }
    }

    protected override async ValueTask<FilterException?> LazyMatchInternalAsync(FilterKv filterKv, LazyObjectGetter<T> matchTargetGetter)
    {
        var (filter, getResult) = filterFactory.VersionFilterFactory.Get(filterKv.Key);
        if (getResult is not null)
        {
            return getResult.PrependFailedKey(GetKey());
        }
        var filterResult = await filter.LazyMatchAsync(filterKv.Value, new LazyObjectGetter<Version>(async _ =>
        {
            var matchTarget = await matchTargetGetter.GetAsync();
            return (await versionValueGetter(matchTarget), true);
        }, matchTargetGetter.Args));
        return filterResult?.PrependFailedKey(GetKey());
    }

    protected override async ValueTask<FilterException?> MatchInternalAsync(FilterKv filterKv, T matchTarget)
    {
        var (filter, getResult) = filterFactory.VersionFilterFactory.Get(filterKv.Key);
        if (getResult is not null)
        {
            return getResult.PrependFailedKey(GetKey());
        }
        var filterResult = filter.Match(filterKv.Value, await versionValueGetter(matchTarget));
        return filterResult?.PrependFailedKey(GetKey());
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
