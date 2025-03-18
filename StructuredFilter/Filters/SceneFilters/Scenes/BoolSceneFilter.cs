using System.Text.Json;
using System.Threading.Tasks;
using StructuredFilter.Filters.Common;
using StructuredFilter.Filters.Common.FilterTypes;
using StructuredFilter.Utils;

namespace StructuredFilter.Filters.SceneFilters.Scenes;

[FilterType(FilterBasicType.Bool)]
public abstract class BoolSceneFilter<T>(FilterFactory<T> filterFactory, BoolSceneFilter<T>.BoolValueGetter boolValueGetter, IFilterResultCache<T>? cache=null) : SceneFilter<T>(cache)
{
    protected delegate Task<bool> BoolValueGetter(T? matchTarget);

    public override FilterException? Valid(JsonElement filterElement)
    {
        var checkResult = filterElement.AssertIsValidObject(this, property =>
        {
            var (filter, getResult) = filterFactory.BoolFilterFactory.Get(property.Name);
            return getResult ?? filter.Valid(property.Value);
        });
        return checkResult?.PrependFailedKey(GetKey());
    }

    protected override async Task<FilterException?> LazyMatchInternalAsync(FilterKv filterKv, LazyObjectGetter<T> matchTargetGetter)
    {
        var (filter, getResult) = filterFactory.BoolFilterFactory.Get(filterKv.Key);
        if (getResult is not null)
        {
            return getResult.PrependFailedKey(GetKey());
        }
        var filterResult = await filter.LazyMatchAsync(filterKv.Value, new LazyObjectGetter<bool>(async _ =>
        {
            var matchTarget = await matchTargetGetter.GetAsync();
            return (await boolValueGetter(matchTarget), true);
        }, matchTargetGetter.Args));
        return filterResult?.PrependFailedKey(GetKey());
    }

    protected override async Task<FilterException?> MatchInternalAsync(FilterKv filterKv, T matchTarget)
    {
        var (filter, getResult) = filterFactory.BoolFilterFactory.Get(filterKv.Key);
        if (getResult is not null)
        {
            return getResult.PrependFailedKey(GetKey());
        }
        var filterResult = filter.Match(filterKv.Value, await boolValueGetter(matchTarget));
        return filterResult?.PrependFailedKey(GetKey());
    }
}

public class DynamicBoolSceneFilter<T> : BoolSceneFilter<T>
{
    public DynamicBoolSceneFilter(FilterFactory<T> filterFactory,
        FilterOption<T>.GetDynamicBoolSceneFilterValueAsync dynamicSceneFilterValueGetter,
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
