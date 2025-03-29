using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SceneFilterModelsExample.Models;
using StructuredFilter.Filters.Common;
using StructuredFilter.Filters.SceneFilters;

namespace SceneFilterModelsExample.Scenes.CacheableScenes;

public class PlayerFilterCache : IFilterResultCache<Player>
{
    private readonly Dictionary<string, bool> _cacheData = new ();
    public int HitCount = 0;
    public Task<Tuple<bool, bool>> GetFilterResultCacheAsync(Player matchTarget, string filterKey, FilterKv filterKv)
    {
        if (_cacheData.TryGetValue(GetCacheKey(filterKey, filterKv, matchTarget.Pid), out var filterResult))
        {
            Interlocked.Add(ref HitCount, 1);
            return Task.FromResult(new Tuple<bool, bool>(filterResult, true));
        }

        return Task.FromResult(new Tuple<bool, bool>(false, false));
    }

    public Task SetFilterResultCacheAsync(Player matchTarget, string filterKey, FilterKv filterKv, bool result)
    {
        _cacheData[GetCacheKey(filterKey, filterKv, matchTarget.Pid)] = result;
        return Task.CompletedTask;
    }

    private string GetCacheKey(string filterKey, FilterKv filterKv, long pid)
    {
        return $"{filterKey}:{filterKv.Key}:{filterKv.Value}:{pid}";
    }
}