using System;
using System.Threading.Tasks;
using StructuredFilter.Filters.Common;

namespace StructuredFilter.Filters.SceneFilters;

public interface IFilterResultCache<in T>
{
    /// <summary>
    /// Get the filter result from cache
    /// </summary>
    /// <param name="matchTarget">match target</param>
    /// <param name="filterKey">filter key, like $player_id</param>
    /// <param name="filterKv">filter KV, like {"$eq": 1000}</param>
    /// <returns>
    /// The first value of the tuple returns the filter result when the second value of the tuple returns true.
    /// The second value indicates whether the cache exists. If the cache does not exist,
    /// it returns false and the first return value needs to be ignored.
    /// </returns>
    Task<Tuple<bool, bool>> GetFilterResultCacheAsync(T matchTarget, string filterKey, FilterKv filterKv);
    Task SetFilterResultCacheAsync(T matchTarget, string filterKey, FilterKv filterKv, bool result);
}
