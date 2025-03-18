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
    /// <param name="filterKv">filter KV</param>
    /// <returns>
    /// The first value of the tuple returns the filter result when the second value of the tuple returns true.
    /// The second value indicates whether the cache exists. If the cache does not exist,
    /// it returns false and the first return value needs to be ignored.
    /// </returns>
    Task<Tuple<bool, bool>> GetFilterResultCacheAsync(T matchTarget, FilterKv filterKv);
    Task SetFilterResultCacheAsync(T matchTarget, FilterKv filterKv, bool result);
}
