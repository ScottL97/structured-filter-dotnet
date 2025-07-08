using System.Text.Json;
using System.Threading.Tasks;
using StructuredFilter.Filters.Common;
using StructuredFilter.Filters.Common.FilterTypes;
using StructuredFilter.Utils;

namespace StructuredFilter.Filters.SceneFilters.Scenes;

[FilterType(FilterBasicType.Double)]
public abstract class DoubleSceneFilter<T>(FilterFactory<T> filterFactory, DoubleSceneFilter<T>.DoubleValueGetter doubleValueGetter, IFilterResultCache<T>? cache=null) : SceneFilter<T>(cache)
{
    protected delegate ValueTask<double> DoubleValueGetter(T? matchTarget);

    public override FilterException? Valid(JsonElement filterElement)
    {
        var checkResult = filterElement.AssertIsValidObject(this, property =>
        {
            var (filter, getResult) = filterFactory.DoubleFilterFactory.Get(property.Name);
            return getResult ?? filter.Valid(property.Value);
        });
        return checkResult?.PrependFailedKey(GetKey());
    }

    protected override async ValueTask<FilterException?> LazyMatchInternalAsync(FilterKv filterKv, LazyObjectGetter<T> matchTargetGetter)
    {
        var (filter, getResult) = filterFactory.DoubleFilterFactory.Get(filterKv.Key);
        if (getResult is not null)
        {
            return getResult.PrependFailedKey(GetKey());
        }
        var filterResult = await filter.LazyMatchAsync(filterKv.Value, new LazyObjectGetter<double>(async _ =>
        {
            var matchTarget = await matchTargetGetter.GetAsync();
            return (await doubleValueGetter(matchTarget), true);
        }, matchTargetGetter.Args));
        return filterResult?.PrependFailedKey(GetKey());
    }

    protected override async ValueTask<FilterException?> MatchInternalAsync(FilterKv filterKv, T matchTarget)
    {
        var (filter, getResult) = filterFactory.DoubleFilterFactory.Get(filterKv.Key);
        if (getResult is not null)
        {
            return getResult.PrependFailedKey(GetKey());
        }
        var filterResult = filter.Match(filterKv.Value, await doubleValueGetter(matchTarget));
        return filterResult?.PrependFailedKey(GetKey());
    }
}

public class DynamicDoubleSceneFilter<T> : DoubleSceneFilter<T>
{
    public DynamicDoubleSceneFilter(FilterFactory<T> filterFactory,
        FilterOption<T>.GetDynamicDoubleSceneFilterValueAsync dynamicSceneFilterValueGetter,
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
