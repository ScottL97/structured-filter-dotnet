using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using StructuredFilter.Filters.Common;
using StructuredFilter.Utils;

namespace StructuredFilter.Filters.SceneFilters;

public class SceneFilterFactory<T> : IFilterFactory<T>
{
    private readonly Dictionary<string, IFilter<T>> _sceneFilters;

    public SceneFilterFactory(
        IEnumerable<IFilter<T>> sceneFilters)
    {
        _sceneFilters = new Dictionary<string, IFilter<T>>();
        foreach (var sceneFilter in sceneFilters)
        {
            _sceneFilters.Add(sceneFilter.GetKey(), sceneFilter);
        }
    }

    public IFilter<T> Get(string key)
    {
        if (key.StartsWith("$.") || key.StartsWith("$["))
        {
            key = Consts.JsonPathFilterKey;
        }
        if (_sceneFilters.TryGetValue(key, out var sceneFilter))
        {
            return sceneFilter;
        }

        this.ThrowSubFilterNotFoundException(key);
        return null;
    }

    public Dictionary<string, IFilter<T>> GetAll()
    {
        return _sceneFilters;
    }

    public void AddFilter(IFilter<T> filter)
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

public abstract class SceneFilter<T> : Filter<T>
{
    private bool IsCacheable { get; init; } = false;
    private readonly IFilterResultCache<T>? _cache;

    protected SceneFilter(IFilterResultCache<T>? cache) : base()
    {
        if (Attribute.GetCustomAttribute(GetType(), typeof(Cacheable)) is null)
        {
            return;
        }

        _cache = cache ?? throw new FilterException(FilterStatusCode.Invalid, $"type {GetType()} Cacheable Attribute is set but IFilterCache is null", $"<{GetType()}>");

        IsCacheable = true;
    }

    public override async Task LazyMatchAsync(JsonElement filterElement, LazyObjectGetter<T> matchTargetGetter)
    {
        try
        {
            if (IsCacheable)
            {
                var matchTarget = await matchTargetGetter.GetAsync();
                var (isMatched, isExists) = await _cache!.GetFilterResultCacheAsync(matchTarget, filterElement);
                if (isExists)
                {
                    if (isMatched)
                    {
                        return;
                    }

                    this.ThrowCacheNotMatchException(matchTarget, filterElement.ToString());
                }
            }

            // there is no matching result in the cache, normal matching
            try
            {
                await LazyMatchInternalAsync(filterElement, matchTargetGetter);
                if (IsCacheable)
                {
                    var matchTarget = await matchTargetGetter.GetAsync();
                    await _cache!.SetFilterResultCacheAsync(matchTarget, filterElement, true);
                }
            }
            catch (FilterException e) when (e.StatusCode == FilterStatusCode.NotMatched)
            {
                if (IsCacheable)
                {
                    var matchTarget = await matchTargetGetter.GetAsync();
                    await _cache!.SetFilterResultCacheAsync(matchTarget, filterElement, false);
                }
                throw;
            }
        }
        catch (LazyObjectGetException)
        {
            this.ThrowMatchTargetGetFailedException(matchTargetGetter.Args);
        }
    }

    public override async Task MatchAsync(JsonElement filterElement, T matchTarget)
    {
        if (IsCacheable)
        {
            var (isMatched, isExists) = await _cache!.GetFilterResultCacheAsync(matchTarget, filterElement);
            if (isExists)
            {
                if (isMatched)
                {
                    return;
                }

                this.ThrowCacheNotMatchException(matchTarget, filterElement.ToString());
            }
        }

        // there is no matching result in the cache, normal matching
        try
        {
            await MatchInternalAsync(filterElement, matchTarget);
            if (IsCacheable)
            {
                await _cache!.SetFilterResultCacheAsync(matchTarget, filterElement, true);
            }
        }
        catch (FilterException e) when (e.StatusCode == FilterStatusCode.NotMatched)
        {
            if (IsCacheable)
            {
                await _cache!.SetFilterResultCacheAsync(matchTarget, filterElement, false);
            }
            throw;
        }
    }

    protected abstract Task LazyMatchInternalAsync(JsonElement element, LazyObjectGetter<T> matchTargetGetter);
    protected abstract Task MatchInternalAsync(JsonElement element, T matchTarget);
}
