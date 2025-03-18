using System;
using System.Collections.Generic;
using System.Linq;
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

    public ISceneFilter<T> Get(string key)
    {
        if (_sceneFilters.TryGetValue(key, out var sceneFilter))
        {
            return sceneFilter;
        }

        this.ThrowSubFilterNotFoundException(key);
        return null;
    }

    public Dictionary<string, IFilter<T>> GetAll()
    {
        return _sceneFilters.ToDictionary(kv => kv.Key, IFilter<T> (kv) => kv.Value);
    }

    public void AddFilter(ISceneFilter<T> filter)
    {
        _sceneFilters.Add(filter.GetKey(), filter);
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

    public async Task LazyMatchAsync(FilterKv filterKv, LazyObjectGetter<T> matchTargetGetter)
    {
        try
        {
            if (IsCacheable)
            {
                var matchTarget = await matchTargetGetter.GetAsync();
                var (isMatched, isExists) = await _cache!.GetFilterResultCacheAsync(matchTarget, filterKv);
                if (isExists)
                {
                    if (isMatched)
                    {
                        return;
                    }

                    this.ThrowCacheNotMatchException(matchTarget, filterKv.ToString());
                }
            }

            // there is no matching result in the cache, normal matching
            try
            {
                await LazyMatchInternalAsync(filterKv, matchTargetGetter);
                if (IsCacheable)
                {
                    var matchTarget = await matchTargetGetter.GetAsync();
                    await _cache!.SetFilterResultCacheAsync(matchTarget, filterKv, true);
                }
            }
            catch (FilterException e) when (e.StatusCode == FilterStatusCode.NotMatched)
            {
                if (IsCacheable)
                {
                    var matchTarget = await matchTargetGetter.GetAsync();
                    await _cache!.SetFilterResultCacheAsync(matchTarget, filterKv, false);
                }
                throw;
            }
        }
        catch (LazyObjectGetException)
        {
            this.ThrowMatchTargetGetFailedException(matchTargetGetter.Args);
        }
    }

    public async Task MatchAsync(FilterKv filterKv, T matchTarget)
    {
        if (IsCacheable)
        {
            var (isMatched, isExists) = await _cache!.GetFilterResultCacheAsync(matchTarget, filterKv);
            if (isExists)
            {
                if (isMatched)
                {
                    return;
                }

                this.ThrowCacheNotMatchException(matchTarget, filterKv.ToString());
            }
        }

        // there is no matching result in the cache, normal matching
        try
        {
            await MatchInternalAsync(filterKv, matchTarget);
            if (IsCacheable)
            {
                await _cache!.SetFilterResultCacheAsync(matchTarget, filterKv, true);
            }
        }
        catch (FilterException e) when (e.StatusCode == FilterStatusCode.NotMatched)
        {
            if (IsCacheable)
            {
                await _cache!.SetFilterResultCacheAsync(matchTarget, filterKv, false);
            }
            throw;
        }
    }

    protected abstract Task LazyMatchInternalAsync(FilterKv filterKv, LazyObjectGetter<T> matchTargetGetter);
    protected abstract Task MatchInternalAsync(FilterKv filterKv, T matchTarget);
}
