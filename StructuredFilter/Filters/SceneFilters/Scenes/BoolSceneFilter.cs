using System.Linq;
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

    public override void Valid(JsonElement filterElement)
    {
        try
        {
            filterElement.AssertIsValidObject(this, property =>
            {
                var filter = filterFactory.BoolFilterFactory.Get(property.Name);
                filter.Valid(property.Value);
            });
        }
        catch (FilterException e)
        {
            throw e.PrependFailedKey(GetKey());
        }
    }

    protected override async Task LazyMatchInternalAsync(JsonElement filterElement, LazyObjectGetter<T> matchTargetGetter)
    {
        var kv = filterElement.EnumerateObject().ToArray()[0];

        try
        {
            var filter = filterFactory.BoolFilterFactory.Get(kv.Name);
            await filter.LazyMatchAsync(kv.Value, new LazyObjectGetter<bool>(async _ =>
            {
                var matchTarget = await matchTargetGetter.GetAsync();
                return (await boolValueGetter(matchTarget), true);
            }, matchTargetGetter.Args));
        }
        catch (FilterException e)
        {
            throw e.PrependFailedKey(GetKey());
        }
    }

    protected override async Task MatchInternalAsync(JsonElement filterElement, T matchTarget)
    {
        var kv = filterElement.EnumerateObject().ToArray()[0];

        try
        {
            var filter = filterFactory.BoolFilterFactory.Get(kv.Name);
            await filter.MatchAsync(kv.Value, await boolValueGetter(matchTarget));
        }
        catch (FilterException e)
        {
            throw e.PrependFailedKey(GetKey());
        }
    }
}

public class DynamicBoolSceneFilter<T> : BoolSceneFilter<T>
{
    public DynamicBoolSceneFilter(FilterFactory<T> filterFactory,
        FilterOption<T>.GetDynamicBoolSceneFilterValueAsync dynamicSceneFilterValueGetter,
        string key,
        bool cacheable = false,
        string? label = null) : base(filterFactory,
        matchTarget => dynamicSceneFilterValueGetter(matchTarget, key))
    {
        KeyOverride = key;
        CacheableOverride = cacheable;
        LabelOverride = label;
    }
}
