using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using StructuredFilter.Filters.Common;
using StructuredFilter.Filters.Common.FilterTypes;

namespace StructuredFilter.Filters.SceneFilters.Scenes;

[FilterType("VERSION")]
public abstract class VersionSceneFilter<T>(FilterFactory<T> filterFactory, VersionSceneFilter<T>.VersionValueGetter versionValueGetter) : Filter<T>
{
    protected delegate Version VersionValueGetter(T? matchTarget);
    private static readonly Version DefaultVersion = new (0, 0);

    public override void Valid(JsonElement filterElement)
    {
        try
        {
            filterElement.AssertIsValidObject(this, property =>
            {
                var filter = filterFactory.VersionFilterFactory.Get(property.Name);
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
            var filter = filterFactory.VersionFilterFactory.Get(kv.Name);
            await filter.LazyMatchAsync(kv.Value, async a =>
            {
                var (matchTarget, isExists) = await targetGetter(args);
                return isExists ? (versionValueGetter(matchTarget), true) : (DefaultVersion, false);
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
            var filter = filterFactory.VersionFilterFactory.Get(kv.Name);
            filter.Match(kv.Value, versionValueGetter(matchTarget));
        }
        catch (FilterException e)
        {
            throw e.PrependFailedKey(GetKey());
        }
    }
}
