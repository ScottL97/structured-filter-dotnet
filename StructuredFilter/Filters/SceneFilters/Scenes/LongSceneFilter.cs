using System.Text.Json;
using System.Threading.Tasks;
using StructuredFilter.Filters.Common;
using StructuredFilter.Filters.Common.FilterTypes;
using StructuredFilter.Utils;

namespace StructuredFilter.Filters.SceneFilters.Scenes;

[FilterType(FilterBasicType.Long)]
public abstract class LongSceneFilter<T>(FilterFactory<T> filterFactory, LongSceneFilter<T>.LongValueGetter longValueGetter, IFilterResultCache<T>? cache=null) : SceneFilter<T>(cache)
{
    protected delegate Task<long> LongValueGetter(T? matchTarget);

    public override FilterException? Valid(JsonElement filterElement)
    {
        var checkResult = filterElement.AssertIsValidObject(this, property =>
        {
            var (filter, getResult) = filterFactory.LongFilterFactory.Get(property.Name);
            return getResult ?? filter.Valid(property.Value);
        });
        return checkResult?.PrependFailedKey(GetKey());
    }

    protected override async ValueTask<FilterException?> LazyMatchInternalAsync(FilterKv filterKv, LazyObjectGetter<T> matchTargetGetter)
    {
        var (filter, getResult) = filterFactory.LongFilterFactory.Get(filterKv.Key);
        if (getResult is not null)
        {
            return getResult.PrependFailedKey(GetKey());
        }
        var filterResult = await filter.LazyMatchAsync(filterKv.Value, new LazyObjectGetter<long>(async _ =>
        {
            var matchTarget = await matchTargetGetter.GetAsync();
            return (await longValueGetter(matchTarget), true);
        }, matchTargetGetter.Args));
        return filterResult?.PrependFailedKey(GetKey());
    }

    protected override async ValueTask<FilterException?> MatchInternalAsync(FilterKv filterKv, T matchTarget)
    {
        var (filter, getResult) = filterFactory.LongFilterFactory.Get(filterKv.Key);
        if (getResult is not null)
        {
            return getResult.PrependFailedKey(GetKey());
        }
        var filterResult = filter.Match(filterKv.Value, await longValueGetter(matchTarget));
        return filterResult?.PrependFailedKey(GetKey());
    }
}

public class DynamicLongSceneFilter<T> : LongSceneFilter<T>
{
    public DynamicLongSceneFilter(FilterFactory<T> filterFactory,
        FilterOption<T>.GetDynamicLongSceneFilterValueAsync dynamicSceneFilterValueGetter,
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
