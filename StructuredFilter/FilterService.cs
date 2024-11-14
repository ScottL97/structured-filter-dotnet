using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using StructuredFilter.Filters;
using StructuredFilter.Filters.Common;
using StructuredFilter.Filters.SceneFilters;

namespace StructuredFilter;

public class JsonPathFilterService : FilterService<JObject>
{
    public JsonPathFilterService(bool withCache = true) : base(withCache)
    {
        WithSceneFilter(f => new JsonPathFilter(f));
    }
}

public class FilterService<T>(bool withCache = true)
{
    protected readonly FilterFactory<T> FilterFactory = new();

    private readonly ConcurrentDictionary<string, FilterDocument<T>> _filterDocuments = new ();

    public delegate IFilter<T> SceneFilterCreator(FilterFactory<T> filterFactory);
    public FilterService<T> WithSceneFilter(SceneFilterCreator sceneFilterCreator)
    {
        FilterFactory.WithSceneFilter(sceneFilterCreator(FilterFactory));
        return this;
    }

    public FilterService<T> WithSceneFilters(IEnumerable<SceneFilterCreator> sceneFilterCreators)
    {
        foreach (var sceneFilterCreator in sceneFilterCreators)
        {
            WithSceneFilter(sceneFilterCreator);
        }
        return this;
    }

    // 返回 matchTargets 中满足 filter 的子集
    public IEnumerable<T> FilterOut(string rawFilter, IEnumerable<T> matchTargets)
    {
        return matchTargets.Where(matchTarget =>
        {
            var filterResult = Match(rawFilter, matchTarget);
            return filterResult.StatusCode == FilterStatusCode.Ok;
        });
    }

    public async Task<IEnumerable<T>> LazyFilterOutAsync(string rawFilter, IFilter<T>.MatchTargetGetter targetGetter, IEnumerable<Dictionary<string, object>> matchTargetsArgs)
    {
        var filteredResults = new List<T>();
        foreach (var args in matchTargetsArgs)
        {
            var filterResult = await LazyMatchAsync(rawFilter, targetGetter, args);
            if (filterResult.StatusCode == FilterStatusCode.Ok)
            {
                // TODO: 这里重复获取 matchTarget，可能产生性能问题及一致性问题
                var (matchTarget, isExists) = await targetGetter(args);
                if (isExists)
                {
                    filteredResults.Add(matchTarget);
                }
            }
        }

        return filteredResults;
    }

    private static readonly FilterException OkResult = new (FilterStatusCode.Ok, "ok", "");

    public FilterException Match(string rawFilter, T matchTarget)
    {
        try
        {
            MustMatch(rawFilter, matchTarget);
        }
        catch (FilterException e)
        {
            return e;
        }

        return OkResult;
    }

    // 不匹配时不会抛出异常而是返回匹配结果
    public async Task<FilterException> LazyMatchAsync(string rawFilter, IFilter<T>.MatchTargetGetter targetGetter, Dictionary<string, object>? args)
    {
        try
        {
            await LazyMustMatchAsync(rawFilter, targetGetter, args);
        }
        catch (FilterException e)
        {
            return e;
        }

        return OkResult;
    }

    public void MustMatch(string rawFilter, T matchTarget)
    {
        if (rawFilter.Length == 0)
        {
            return;
        }

        try
        {
            var filterDocument = GetFilterDocument(rawFilter);
            foreach (var property in filterDocument.Document.RootElement.EnumerateObject())
            {
                var filter = FilterFactory.Get(property.Name);
                if (filter.GetType() == typeof(JsonPathFilter))
                {
                    filter.Match(filterDocument.Document.RootElement, matchTarget);
                    continue;
                }
                filter.Match(property.Value, matchTarget);
            }
        }
        catch (FilterException)
        {
            throw;
        }
        catch (Exception e)
        {
            throw new FilterException(FilterStatusCode.MatchError, e.Message, "<UNKNOWN>");
        }
    }

    // 不匹配时抛出异常
    public async Task LazyMustMatchAsync(string rawFilter, IFilter<T>.MatchTargetGetter targetGetter, Dictionary<string, object>? args)
    {
        if (rawFilter.Length == 0)
        {
            return;
        }

        try
        {
            var filterDocument = GetFilterDocument(rawFilter);
            foreach (var property in filterDocument.Document.RootElement.EnumerateObject())
            {
                var filter = FilterFactory.Get(property.Name);
                if (filter.GetType() == typeof(JsonPathFilter))
                {
                    await filter.LazyMatchAsync(filterDocument.Document.RootElement, targetGetter, args);
                    continue;
                }
                await filter.LazyMatchAsync(property.Value, targetGetter, args);
            }
        }
        catch (FilterException)
        {
            throw;
        }
        catch (Exception e)
        {
            throw new FilterException(FilterStatusCode.MatchError, e.Message, "<UNKNOWN>");
        }
    }

    private FilterDocument<T> GetFilterDocument(string rawFilter)
    {
        if (withCache)
        {
            return _filterDocuments.GetOrAdd(rawFilter, _ => new FilterDocument<T>(rawFilter, FilterFactory));
        }

        return new FilterDocument<T>(rawFilter, FilterFactory);
    }

    public Dictionary<string, SceneFilterInfo> GetSceneFilterInfos()
    {
        return FilterFactory.GetSceneFilterInfos();
    }
}
