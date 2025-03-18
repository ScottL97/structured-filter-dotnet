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

    public override FilterException? Valid(JsonElement filterElement)
    {
        try
        {
            var checkResult = filterElement.AssertIsValidObject(this, property =>
            {
                var (filter, getResult) = filterFactory.StringFilterFactory.Get(property.Name);
                return getResult ?? filter.Valid(property.Value);
            });
            return checkResult?.PrependFailedKey(GetKey());
        }
        catch (FilterException e)
        {
            throw e.PrependFailedKey(GetKey());
        }
    }

    protected override async Task<FilterException?> LazyMatchInternalAsync(FilterKv filterKv, LazyObjectGetter<T> matchTargetGetter)
    {
        var (filter, getResult) = filterFactory.StringFilterFactory.Get(filterKv.Key);
        if (getResult is not null)
        {
            return getResult.PrependFailedKey(GetKey());
        }
        var filterResult = await filter.LazyMatchAsync(filterKv.Value, new LazyObjectGetter<string>(async _ =>
        {
            var matchTarget = await matchTargetGetter.GetAsync();
            return (await stringValueGetter(matchTarget), true);
        }, matchTargetGetter.Args));
        return filterResult?.PrependFailedKey(GetKey());
    }

    protected override async Task<FilterException?> MatchInternalAsync(FilterKv filterKv, T matchTarget)
    {
        var (filter, getResult) = filterFactory.StringFilterFactory.Get(filterKv.Key);
        if (getResult is not null)
        {
            return getResult.PrependFailedKey(GetKey());
        }
        var filterResult = filter.Match(filterKv.Value, await stringValueGetter(matchTarget));
        return filterResult?.PrependFailedKey(GetKey());
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
