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

    public (ILogicFilter<T>, FilterException?) Get(string key)
    {
        if (_logicFilters.TryGetValue(key, out var logicFilter))
        {
            return (logicFilter, null);
        }

        return (null!, this.CreateSubFilterNotFoundException(key));
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
    public FilterException? Valid(JsonElement element)
    {
        var checkResult = element.AssertIsValidFilterObjectArray(this, property =>
        {
            var (filter, getResult) = sceneFilterFactory.Get(property.Name);
            return getResult ?? filter.Valid(property.Value);
        });
        return checkResult?.PrependFailedKey(GetKey());
    }

    public async ValueTask<FilterException?> LazyMatchAsync(FilterArray filterArray, LazyObjectGetter<T> matchTargetGetter)
    {
        foreach (var filterObject in filterArray.FilterObjects)
        {
            // 只要有一个filter不匹配，就抛异常
            var (filter, getResult) = sceneFilterFactory.Get(filterObject.Key);
            if (getResult is not null)
            {
                return getResult.PrependFailedKey(GetKey());
            }
            var filterResult = await filter.LazyMatchAsync(filterObject.FilterKv!.Value, matchTargetGetter);
            if (filterResult is not null)
            {
                return filterResult.PrependFailedKey(GetKey());
            }
        }

        return null;
    }

    public async ValueTask<FilterException?> MatchAsync(FilterArray filterArray, T matchTarget)
    {
        foreach (var filterObject in filterArray.FilterObjects)
        {
            // 只要有一个filter不匹配，就抛异常
            var (filter, getResult) = sceneFilterFactory.Get(filterObject.Key);
            if (getResult is not null)
            {
                return getResult.PrependFailedKey(GetKey());
            }
            var filterResult = await filter.MatchAsync(filterObject.FilterKv!.Value, matchTarget);
            if (filterResult is not null)
            {
                return filterResult.PrependFailedKey(GetKey());
            }
        }

        return null;
    }
}

[FilterLabel("任意一个 filter 匹配")]
[FilterKey("$or")]
public class OrFilter<T>(SceneFilterFactory<T> sceneFilterFactory) : Filter<T>, ILogicFilter<T>
{
    public FilterException? Valid(JsonElement element)
    {
        var checkResult = element.AssertIsValidFilterObjectArray(this, property =>
        {
            var (filter, getResult) = sceneFilterFactory.Get(property.Name);
            return getResult ?? filter.Valid(property.Value);
        });
        return checkResult?.PrependFailedKey(GetKey());
    }

    public async ValueTask<FilterException?> LazyMatchAsync(FilterArray filterArray, LazyObjectGetter<T> matchTargetGetter)
    {
        var failedKeyPath = new List<Tree<string>>();
        foreach (var filterObject in filterArray.FilterObjects)
        {
            var (filter, getResult) = sceneFilterFactory.Get(filterObject.Key);
            if (getResult is not null)
            {
                return getResult.PrependFailedKey(GetKey());
            }
            var filterResult = await filter.LazyMatchAsync(filterObject.FilterKv!.Value, matchTargetGetter);
            if (filterResult is not null)
            {
                failedKeyPath.Add(filterResult.FailedKeyPath);
                continue;
            }

            // 只要有一个filter匹配，就匹配成功，不抛异常
            return null;
        }

        return new FilterException(FilterStatusCode.NotMatched, "no filters match $or", GetKey()).AppendFailedKeys(failedKeyPath);
    }

    public async ValueTask<FilterException?> MatchAsync(FilterArray filterArray, T matchTarget)
    {
        var failedKeyPath = new List<Tree<string>>();
        foreach (var filterObject in filterArray.FilterObjects)
        {
            var (filter, getResult) = sceneFilterFactory.Get(filterObject.Key);
            if (getResult is not null)
            {
                return getResult.PrependFailedKey(GetKey());
            }
            var filterResult = await filter.MatchAsync(filterObject.FilterKv!.Value, matchTarget);
            if (filterResult is not null)
            {
                failedKeyPath.Add(filterResult.FailedKeyPath);
                continue;
            }

            // 只要有一个filter匹配，就匹配成功，不抛异常
            return null;
        }

        return new FilterException(FilterStatusCode.NotMatched, "no filters match $or", GetKey()).AppendFailedKeys(failedKeyPath);
    }
}
