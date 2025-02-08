using System.Text.Json;
using StructuredFilter.Filters;
using StructuredFilter.Filters.Common;
using StructuredFilter.Filters.SceneFilters;
using StructuredFilter.Filters.SceneFilters.Scenes;
using StructuredFilter.Test.Models;

namespace StructuredFilter.Test.Scenes.CacheableScenes;

[FilterLabel("玩家 ID")]
[FilterKey("pid")]
[Cacheable]
public class PidFilter(FilterFactory<Player> filterFactory)
    : NumberSceneFilter<Player>(filterFactory, player => Task.FromResult((double)player.Pid), new PidFilterCache());

public class PidFilterCache : IFilterResultCache<Player>
{
    private readonly Dictionary<string, bool> _cacheData = new ();
    public Task<Tuple<bool, bool>> GetFilterResultCacheAsync(Player matchTarget, JsonElement filterElement)
    {
        return Task.FromResult(_cacheData.TryGetValue(GetCacheKey(filterElement, matchTarget.Pid), out var filterResult) ?
            new Tuple<bool, bool>(filterResult, true) : new Tuple<bool, bool>(false, false));
    }

    public Task SetFilterResultCacheAsync(Player matchTarget, JsonElement filterElement, bool result)
    {
        _cacheData[GetCacheKey(filterElement, matchTarget.Pid)] = result;
        return Task.CompletedTask;
    }

    private string GetCacheKey(JsonElement filterElement, long pid)
    {
        return $"{filterElement}:{pid}";
    }
}
