using StructuredFilter.Filters.Common;
using StructuredFilter.Filters.SceneFilters;
using StructuredFilter.Test.Models;

namespace StructuredFilter.Test.Scenes.CacheableScenes;

public class PlayerFilterCache : IFilterResultCache<Player>
{
    private readonly Dictionary<string, bool> _cacheData = new ();
    public int HitCount = 0;
    public Task<Tuple<bool, bool>> GetFilterResultCacheAsync(Player matchTarget, FilterKv filterKv)
    {
        if (_cacheData.TryGetValue(GetCacheKey(filterKv, matchTarget.Pid), out var filterResult))
        {
            Console.WriteLine("hit filter result cache");
            Interlocked.Add(ref HitCount, 1);
            return Task.FromResult(new Tuple<bool, bool>(filterResult, true));
        }

        return Task.FromResult(new Tuple<bool, bool>(false, false));
    }

    public Task SetFilterResultCacheAsync(Player matchTarget, FilterKv filterKv, bool result)
    {
        _cacheData[GetCacheKey(filterKv, matchTarget.Pid)] = result;
        return Task.CompletedTask;
    }

    private string GetCacheKey(FilterKv filterKv, long pid)
    {
        return $"{filterKv}:{pid}";
    }
}