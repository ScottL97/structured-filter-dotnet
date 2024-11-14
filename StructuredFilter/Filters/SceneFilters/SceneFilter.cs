using System.Collections.Generic;
using System.Text.Json.Serialization;
using StructuredFilter.Filters.Common;

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
