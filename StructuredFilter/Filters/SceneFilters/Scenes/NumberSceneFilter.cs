using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using StructuredFilter.Filters.Common;
using StructuredFilter.Filters.Common.FilterTypes;

namespace StructuredFilter.Filters.SceneFilters.Scenes;

[FilterType("NUMBER")]
public abstract class NumberSceneFilter<T>(FilterFactory<T> filterFactory, NumberSceneFilter<T>.NumberValueGetter numberValueGetter) : Filter<T>
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

    public override async Task LazyMatchAsync(JsonElement filterElement, IFilter<T>.MatchTargetGetter targetGetter, Dictionary<string, object>? args)
    {
        var kv = filterElement.EnumerateObject().ToArray()[0];

        try
        {
            var filter = filterFactory.NumberFilterFactory.Get(kv.Name);
            await filter.LazyMatchAsync(kv.Value, async a =>
            {
                var (matchTarget, isExists) = await targetGetter(args);
                return isExists ? (numberValueGetter(matchTarget), true) : (0, false);
            }, args);
        }
        catch (FilterException e)
        {
            throw e.PrependFailedKey(GetKey());
        }
    }

    public override void Match(JsonElement filterElement, T matchTarget)
    {
        var kv = filterElement.EnumerateObject().ToArray()[0];

        try
        {
            var filter = filterFactory.NumberFilterFactory.Get(kv.Name);
            filter.Match(kv.Value, numberValueGetter(matchTarget));
        }
        catch (FilterException e)
        {
            throw e.PrependFailedKey(GetKey());
        }
    }
}