﻿using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using StructuredFilter.Filters.Common;
using StructuredFilter.Filters.Common.FilterTypes;
using StructuredFilter.Filters.SceneFilters;
using StructuredFilter.Utils;

namespace StructuredFilter.Filters.LogicFilters;

public class LogicFilterFactory<T> : IFilterFactory<T>
{
    private readonly Dictionary<string, IFilter<T>> _logicFilters;

    public LogicFilterFactory(IEnumerable<IFilter<T>> logicFilters)
    {
        _logicFilters = new Dictionary<string, IFilter<T>>();
        foreach (var logicFilter in logicFilters)
        {
            _logicFilters.Add(logicFilter.GetKey(), logicFilter);
        }
    }

    public IFilter<T> Get(string key)
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
        return _logicFilters;
    }

    public void AddFilter(IFilter<T> filter)
    {
        _logicFilters.Add(filter.GetKey(), filter);
    }
}

[FilterLabel("所有 filter 均匹配")]
[FilterKey("$and")]
public class AndFilter<T>(SceneFilterFactory<T> sceneFilterFactory) : Filter<T>
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

    public override async Task LazyMatchAsync(JsonElement element, LazyObjectGetter<T> matchTargetGetter)
    {
        try
        {
            foreach (var filterObject in element.EnumerateArray())
            {
                // 只要有一个filter不匹配，就抛异常
                foreach (var property in filterObject.EnumerateObject())
                {
                    
                        if (property.IsJsonPathFilter())
                        {
                            await sceneFilterFactory.Get(Consts.JsonPathFilterKey).LazyMatchAsync(filterObject, matchTargetGetter);
                            continue;
                        }
                        var filter = sceneFilterFactory.Get(property.Name);
                        await filter.LazyMatchAsync(property.Value, matchTargetGetter);
                }
            }
        }
        catch (FilterException e)
        {
            throw e.PrependFailedKey(GetKey());
        }
    }

    public override async Task MatchAsync(JsonElement element, T matchTarget)
    {
        try
        {
            foreach (var filterObject in element.EnumerateArray())
            {
                // 只要有一个filter不匹配，就抛异常
                foreach (var property in filterObject.EnumerateObject())
                {
                    
                        if (property.IsJsonPathFilter())
                        {
                            await sceneFilterFactory.Get(Consts.JsonPathFilterKey).MatchAsync(filterObject, matchTarget);
                            continue;
                        }
                        var filter = sceneFilterFactory.Get(property.Name);
                        await filter.MatchAsync(property.Value, matchTarget);
                    
                }
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
public class OrFilter<T>(SceneFilterFactory<T> sceneFilterFactory) : Filter<T>
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

    public override async Task LazyMatchAsync(JsonElement element, LazyObjectGetter<T> matchTargetGetter)
    {
        var failedKeyPath = new List<Tree<string>>();
        foreach (var filterObject in element.EnumerateArray())
        {
            foreach (var property in filterObject.EnumerateObject())
            {
                try
                {
                    if (property.IsJsonPathFilter())
                    {
                        await sceneFilterFactory.Get(Consts.JsonPathFilterKey).LazyMatchAsync(filterObject, matchTargetGetter);
                        continue;
                    }
                    var filter = sceneFilterFactory.Get(property.Name);
                    await filter.LazyMatchAsync(property.Value, matchTargetGetter);
                }
                catch (FilterException e)
                {
                    failedKeyPath.Add(e.FailedKeyPath);
                    continue;
                }

                // 只要有一个filter匹配，就匹配成功，不抛异常
                return;
            }
        }

        throw new FilterException(FilterStatusCode.NotMatched, "no filters match $or", GetKey()).AppendFailedKeys(failedKeyPath);
    }

    public override async Task MatchAsync(JsonElement element, T matchTarget)
    {
        var failedKeyPath = new List<Tree<string>>();
        foreach (var filterObject in element.EnumerateArray())
        {
            foreach (var property in filterObject.EnumerateObject())
            {
                try
                {
                    if (property.IsJsonPathFilter())
                    {
                        await sceneFilterFactory.Get(Consts.JsonPathFilterKey).MatchAsync(filterObject, matchTarget);
                        continue;
                    }
                    var filter = sceneFilterFactory.Get(property.Name);
                    await filter.MatchAsync(property.Value, matchTarget);
                }
                catch (FilterException e)
                {
                    failedKeyPath.Add(e.FailedKeyPath);
                    continue;
                }
                // 只要有一个filter匹配，就匹配成功，不抛异常
                return;
            }
        }

        throw new FilterException(FilterStatusCode.NotMatched, "no filters match $or", GetKey()).AppendFailedKeys(failedKeyPath);
    }
}
