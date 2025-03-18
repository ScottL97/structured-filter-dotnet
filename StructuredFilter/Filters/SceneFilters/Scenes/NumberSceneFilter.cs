using System.Text.Json;
using System.Threading.Tasks;
using StructuredFilter.Filters.Common;
using StructuredFilter.Filters.Common.FilterTypes;
using StructuredFilter.Utils;

namespace StructuredFilter.Filters.SceneFilters.Scenes;

[FilterType(FilterBasicType.Number)]
public abstract class NumberSceneFilter<T>(FilterFactory<T> filterFactory, NumberSceneFilter<T>.NumberValueGetter numberValueGetter, IFilterResultCache<T>? cache=null) : SceneFilter<T>(cache)
{
    protected delegate Task<double> NumberValueGetter(T? matchTarget);

    public override FilterException? Valid(JsonElement filterElement)
    {
        var checkResult = filterElement.AssertIsValidObject(this, property =>
        {
            var (filter, getResult) = filterFactory.NumberFilterFactory.Get(property.Name);
            return getResult ?? filter.Valid(property.Value);
        });
        return checkResult?.PrependFailedKey(GetKey());
    }

    protected override async Task<FilterException?> LazyMatchInternalAsync(FilterKv filterKv, LazyObjectGetter<T> matchTargetGetter)
    {
        var (filter, getResult) = filterFactory.NumberFilterFactory.Get(filterKv.Key);
        if (getResult is not null)
        {
            return getResult.PrependFailedKey(GetKey());
        }
        var filterResult = await filter.LazyMatchAsync(filterKv.Value, new LazyObjectGetter<double>(async _ =>
        {
            var matchTarget = await matchTargetGetter.GetAsync();
            return (await numberValueGetter(matchTarget), true);
        }, matchTargetGetter.Args));
        return filterResult?.PrependFailedKey(GetKey());
    }

    protected override async Task<FilterException?> MatchInternalAsync(FilterKv filterKv, T matchTarget)
    {
        var (filter, getResult) = filterFactory.NumberFilterFactory.Get(filterKv.Key);
        if (getResult is not null)
        {
            return getResult.PrependFailedKey(GetKey());
        }
        var filterResult = filter.Match(filterKv.Value, await numberValueGetter(matchTarget));
        return filterResult?.PrependFailedKey(GetKey());
    }
}

public class DynamicNumberSceneFilter<T> : NumberSceneFilter<T>
{
    public DynamicNumberSceneFilter(FilterFactory<T> filterFactory,
        FilterOption<T>.GetDynamicNumberSceneFilterValueAsync dynamicSceneFilterValueGetter,
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
