using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace StructuredFilter.Filters.SceneFilters;

public interface IFilterResultCache<in T>
{
    /// <summary>
    /// Get the filter result from cache
    /// </summary>
    /// <param name="matchTarget">match target</param>
    /// <param name="filterElement">filter JSON element</param>
    /// <returns>
    /// The first value of the tuple returns the filter result when the second value of the tuple returns true.
    /// The second value indicates whether the cache exists. If the cache does not exist,
    /// it returns false and the first return value needs to be ignored.
    /// </returns>
    Task<Tuple<bool, bool>> GetFilterResultCacheAsync(T matchTarget, JsonElement filterElement);
    Task SetFilterResultCacheAsync(T matchTarget, JsonElement filterElement, bool result);
}
