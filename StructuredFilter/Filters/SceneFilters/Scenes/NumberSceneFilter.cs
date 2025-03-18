using System.Linq;
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

    public override void Valid(JsonElement filterElement)
    {
        try
        {
            filterElement.AssertIsValidObject(this, property =>
            {
                var filter = filterFactory.NumberFilterFactory.Get(property.Name);
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
            var filter = filterFactory.NumberFilterFactory.Get(filterKv.Key);
            await filter.LazyMatchAsync(filterKv.Value, new LazyObjectGetter<double>(async _ =>
            {
                var matchTarget = await matchTargetGetter.GetAsync();
                return (await numberValueGetter(matchTarget), true);
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
            var filter = filterFactory.NumberFilterFactory.Get(filterKv.Key);
            await filter.MatchAsync(filterKv.Value, await numberValueGetter(matchTarget));
        }
        catch (FilterException e)
        {
            throw e.PrependFailedKey(GetKey());
        }
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
