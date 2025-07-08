using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using StructuredFilter.Filters.Common;
using StructuredFilter.Utils;

namespace StructuredFilter.Filters.SceneFilters;

public class SceneFilterFactory<T> : ISceneFilterFactory<T>
{
    private readonly Dictionary<string, ISceneFilter<T>> _sceneFilters;

    public SceneFilterFactory(
        IEnumerable<ISceneFilter<T>> sceneFilters)
    {
        _sceneFilters = new Dictionary<string, ISceneFilter<T>>();
        foreach (var sceneFilter in sceneFilters)
        {
            _sceneFilters.Add(sceneFilter.GetKey(), sceneFilter);
        }
    }

    public (ISceneFilter<T>, FilterException?) Get(string key)
    {
        if (_sceneFilters.TryGetValue(key, out var sceneFilter))
        {
            return (sceneFilter, null);
        }

        return (null, this.CreateSubFilterNotFoundException(key));
    }

    public Dictionary<string, IFilter<T>> GetAll()
    {
        return _sceneFilters.ToDictionary(kv => kv.Key, IFilter<T> (kv) => kv.Value);
    }

    public void AddFilter(ISceneFilter<T> filter, bool enableOverride = false)
    {
        if (enableOverride)
        {
            _sceneFilters.TryAdd(filter.GetKey(), filter);
        }
        else
        {
            try
            {
                _sceneFilters.Add(filter.GetKey(), filter);
            }
            catch (ArgumentException)
            {
                throw new FilterException(FilterStatusCode.Invalid, $"filter {filter.GetKey()} has already been added");
            }
        }
    }
}

public class SceneFilterInfo
{
    [JsonPropertyName("label")]
    public string Label { get; set; }

    [JsonPropertyName("logics")]
    public OperatorInfo[] Logics { get; set; }
    
    [JsonPropertyName("type")]
    public string Type { get; set; }
}

public class OperatorInfo
{
    [JsonPropertyName("label")]
    public string Label { get; set; }
    
    [JsonPropertyName("value")]
    public string Value { get; set; }
}

public abstract class SceneFilter<T> : Filter<T>, ISceneFilter<T>
{
    private bool IsCacheable { get; set; } = false;
    private IFilterResultCache<T>? _cache;

    protected SceneFilter(IFilterResultCache<T>? cache)
    {
        ConfigureCache(cache);
    }

    private void ConfigureCache(IFilterResultCache<T>? cache)
    {
        if (ConfigureIsCacheable())
        {
            SetCache(cache);
        }
    }

    protected bool SetIsCacheable(bool isCacheable)
    {
        IsCacheable = isCacheable;
        return IsCacheable;
    }

    protected void SetCache(IFilterResultCache<T>? cache)
    {
        _cache = cache ?? throw new FilterException(FilterStatusCode.Invalid,
            $"type {GetType()} is set Cacheable but IFilterResultCache is not provided", $"<{GetType()}>");
    }

    private bool ConfigureIsCacheable()
    {
        if (Attribute.GetCustomAttribute(GetType(), typeof(Cacheable)) is not null)
        {
            SetIsCacheable(true);
        }

        return IsCacheable;
    }

    public async ValueTask<FilterException?> LazyMatchAsync(FilterKv filterKv, LazyObjectGetter<T> matchTargetGetter)
    {
        try
        {
            if (IsCacheable)
            {
                var matchTarget = await matchTargetGetter.GetAsync();
                var (isMatched, isExists) = await _cache!.GetFilterResultCacheAsync(matchTarget, GetKey(), filterKv);
                if (isExists)
                {
                    if (isMatched)
                    {
                        return null;
                    }

                    return this.CreateCacheNotMatchException(matchTarget, filterKv.ToString());
                }
            }

            // there is no matching result in the cache, normal matching
            var filterResult = await LazyMatchInternalAsync(filterKv, matchTargetGetter);
            if (IsCacheable)
            {
                if (filterResult is null)
                {
                    var matchTarget = await matchTargetGetter.GetAsync();
                    await _cache!.SetFilterResultCacheAsync(matchTarget, GetKey(), filterKv, true);
                }
                else if (filterResult.StatusCode == FilterStatusCode.NotMatched)
                {
                    var matchTarget = await matchTargetGetter.GetAsync();
                    await _cache!.SetFilterResultCacheAsync(matchTarget, GetKey(), filterKv, false);
                }
            }

            return filterResult;
        }
        catch (LazyObjectGetException)
        {
            return this.CreateMatchTargetGetFailedException(matchTargetGetter.Args);
        }
    }

    public async ValueTask<FilterException?> MatchAsync(FilterKv filterKv, T matchTarget)
    {
        if (IsCacheable)
        {
            var (isMatched, isExists) = await _cache!.GetFilterResultCacheAsync(matchTarget, GetKey(), filterKv);
            if (isExists)
            {
                if (isMatched)
                {
                    return null;
                }

                return this.CreateCacheNotMatchException(matchTarget, filterKv.ToString());
            }
        }

        // there is no matching result in the cache, normal matching
        var filterResult = await MatchInternalAsync(filterKv, matchTarget);
        if (IsCacheable)
        {
            if (filterResult is null)
            {
                await _cache!.SetFilterResultCacheAsync(matchTarget, GetKey(), filterKv, true);
            }
            else if (filterResult.StatusCode == FilterStatusCode.NotMatched)
            {
                await _cache!.SetFilterResultCacheAsync(matchTarget, GetKey(), filterKv, false);
            }
        }

        return filterResult;
    }

    protected abstract ValueTask<FilterException?> LazyMatchInternalAsync(FilterKv filterKv, LazyObjectGetter<T> matchTargetGetter);
    protected abstract ValueTask<FilterException?> MatchInternalAsync(FilterKv filterKv, T matchTarget);
    public abstract FilterException? Valid(JsonElement element);
}
