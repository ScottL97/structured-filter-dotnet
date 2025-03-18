using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using StructuredFilter.Filters.Common;
using StructuredFilter.Filters.Common.FilterTypes;
using StructuredFilter.Filters.SceneFilters;
using StructuredFilter.Utils;

namespace StructuredFilter.Filters.LogicFilters;

public class LogicFilterFactory<T> : ILogicFilterFactory<T>
{
    private readonly Dictionary<string, ILogicFilter<T>> _logicFilters;

    public LogicFilterFactory(IEnumerable<ILogicFilter<T>> logicFilters)
    {
        _logicFilters = new Dictionary<string, ILogicFilter<T>>();
        foreach (var logicFilter in logicFilters)
        {
            _logicFilters.Add(logicFilter.GetKey(), logicFilter);
        }
    }

    public ILogicFilter<T> Get(string key)
    {
        if (_logicFilters.TryGetValue(key, out var logicFilter))
        {
            return logicFilter;
        }

        this.ThrowSubFilterNotFoundException(key);
        return null;
    }

    public Dictionary<string, IFilter<T>> GetAll()
    {
        return _logicFilters.ToDictionary(kv => kv.Key, IFilter<T> (kv) => kv.Value);
    }

    public void AddFilter(ILogicFilter<T> filter)
    {
        _logicFilters.Add(filter.GetKey(), filter);
    }
}

[FilterLabel("所有 filter 均匹配")]
[FilterKey("$and")]
public class AndFilter<T>(SceneFilterFactory<T> sceneFilterFactory) : Filter<T>, ILogicFilter<T>
{
    public override void Valid(JsonElement element)
    {
        try
        {
            element.AssertIsValidFilterObjectArray(this, property =>
            {
                var filter = sceneFilterFactory.Get(property.Name);
                filter.Valid(property.Value);
            });
        }
        catch (FilterException e)
        {
            throw e.PrependFailedKey(GetKey());
        }
    }

    public async Task LazyMatchAsync(FilterArray filterArray, LazyObjectGetter<T> matchTargetGetter)
    {
        try
        {
            foreach (var filterObject in filterArray.FilterObjects)
            {
                // 只要有一个filter不匹配，就抛异常
                var filter = sceneFilterFactory.Get(filterObject.Key);
                await filter.LazyMatchAsync(filterObject.FilterKv!.Value, matchTargetGetter);
            }
        }
        catch (FilterException e)
        {
            throw e.PrependFailedKey(GetKey());
        }
    }

    public async Task MatchAsync(FilterArray filterArray, T matchTarget)
    {
        try
        {
            foreach (var filterObject in filterArray.FilterObjects)
            {
                // 只要有一个filter不匹配，就抛异常
                var filter = sceneFilterFactory.Get(filterObject.Key);
                await filter.MatchAsync(filterObject.FilterKv!.Value, matchTarget);
            }
        }
        catch (FilterException e)
        {
            throw e.PrependFailedKey(GetKey());
        }
    }
}

[FilterLabel("任意一个 filter 匹配")]
[FilterKey("$or")]
public class OrFilter<T>(SceneFilterFactory<T> sceneFilterFactory) : Filter<T>, ILogicFilter<T>
{
    public override void Valid(JsonElement element)
    {
        try
        {
            element.AssertIsValidFilterObjectArray(this, property =>
            {
                var filter = sceneFilterFactory.Get(property.Name);
                filter.Valid(property.Value);
            });
        }
        catch (FilterException e)
        {
            throw e.PrependFailedKey(GetKey());
        }
    }

    public async Task LazyMatchAsync(FilterArray filterArray, LazyObjectGetter<T> matchTargetGetter)
    {
        var failedKeyPath = new List<Tree<string>>();
        foreach (var filterObject in filterArray.FilterObjects)
        {
            try
            {
                var filter = sceneFilterFactory.Get(filterObject.Key);
                await filter.LazyMatchAsync(filterObject.FilterKv!.Value, matchTargetGetter);
            }
            catch (FilterException e)
            {
                failedKeyPath.Add(e.FailedKeyPath);
                continue;
            }

            // 只要有一个filter匹配，就匹配成功，不抛异常
            return;
        }

        throw new FilterException(FilterStatusCode.NotMatched, "no filters match $or", GetKey()).AppendFailedKeys(failedKeyPath);
    }

    public async Task MatchAsync(FilterArray filterArray, T matchTarget)
    {
        var failedKeyPath = new List<Tree<string>>();
        foreach (var filterObject in filterArray.FilterObjects)
        {
            try
            {
                var filter = sceneFilterFactory.Get(filterObject.Key);
                await filter.MatchAsync(filterObject.FilterKv!.Value, matchTarget);
            }
            catch (FilterException e)
            {
                failedKeyPath.Add(e.FailedKeyPath);
                continue;
            }
            // 只要有一个filter匹配，就匹配成功，不抛异常
            return;
        }

        throw new FilterException(FilterStatusCode.NotMatched, "no filters match $or", GetKey()).AppendFailedKeys(failedKeyPath);
    }
}
