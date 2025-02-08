using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using StructuredFilter.Filters.Common;
using StructuredFilter.Filters.Common.FilterTypes;
using StructuredFilter.Utils;

namespace StructuredFilter.Filters.SceneFilters.Scenes;

[FilterType("NUMBER")]
public abstract class NumberSceneFilter<T>(FilterFactory<T> filterFactory, NumberSceneFilter<T>.NumberValueGetter numberValueGetter, IFilterResultCache<T>? cache=null) : SceneFilter<T>(cache)
{
    protected delegate double NumberValueGetter(T? matchTarget);

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

    protected override async Task LazyMatchInternalAsync(JsonElement filterElement, LazyObjectGetter<T> matchTargetGetter)
    {
        var kv = filterElement.EnumerateObject().ToArray()[0];

        try
        {
            var filter = filterFactory.NumberFilterFactory.Get(kv.Name);
            await filter.LazyMatchAsync(kv.Value, new LazyObjectGetter<double>(async _ =>
            {
                var matchTarget = await matchTargetGetter.GetAsync();
                return (numberValueGetter(matchTarget), true);
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
            var filter = filterFactory.NumberFilterFactory.Get(kv.Name);
            await filter.MatchAsync(kv.Value, numberValueGetter(matchTarget));
        }
        catch (FilterException e)
        {
            throw e.PrependFailedKey(GetKey());
        }
    }
}