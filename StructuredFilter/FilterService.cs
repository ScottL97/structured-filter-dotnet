using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using StructuredFilter.Filters;
using StructuredFilter.Filters.Common;
using StructuredFilter.Filters.SceneFilters;
using StructuredFilter.Utils;

namespace StructuredFilter;

public class JsonPathFilterService : FilterService<JObject>
{
    public JsonPathFilterService(FilterOption<JObject>? option = null) : base(option)
    {
        WithSceneFilter(f => new JsonPathFilter(f));
    }
}

public class FilterService<T>(FilterOption<T>? option=null)
{
    protected readonly FilterFactory<T> FilterFactory = new();

    private readonly ConcurrentDictionary<string, FilterDocument<T>> _filterDocuments = new ();

    public delegate IFilter<T> SceneFilterCreator(FilterFactory<T> filterFactory);
    
    private readonly FilterOption<T> _filterOption = option ?? new FilterOption<T>();

    public void MustValidFilter(string filter)
    {
        FilterValidator.MustValid(filter, FilterFactory);
    }

    public void MustValidFilter(JsonDocument filterDocument)
    {
        FilterValidator.MustValid(filterDocument, FilterFactory);
    }

    public async Task<FilterService<T>> LoadDynamicSceneFilters()
    {
        await FilterFactory.LoadDynamicSceneFiltersAsync(_filterOption);

        return this;
    }

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
    public async Task<IEnumerable<T>> FilterOutAsync(string rawFilter, IEnumerable<T> matchTargets)
    {
        var tasks = matchTargets.Select(async matchTarget =>
        {
            var filterResult = await MatchAsync(rawFilter, matchTarget);
            return new
            {
                MatchTarget = matchTarget,
                IsMatched = filterResult.StatusCode == FilterStatusCode.Ok
            };
        });
        var results = await Task.WhenAll(tasks);
        return results.Where(r => r.IsMatched).Select(r => r.MatchTarget);
    }

    // 返回 matchTargets 中满足 filter 的子集
    public async Task<IEnumerable<T>> LazyFilterOutAsync(string rawFilter, IEnumerable<LazyObjectGetter<T>> matchTargetsGetters)
    {
        var filteredResults = new List<T>();
        foreach (var matchTargetGetter in matchTargetsGetters)
        {
            var filterResult = await LazyMatchAsync(rawFilter, matchTargetGetter);
            if (filterResult.StatusCode == FilterStatusCode.Ok)
            {
                var matchTarget = await matchTargetGetter.GetAsync();
                filteredResults.Add(matchTarget);
            }
        }

        return filteredResults;
    }

    private static readonly FilterException OkResult = new (FilterStatusCode.Ok, "ok", "");

    // 不匹配时不会抛出异常而是返回匹配结果
    public async Task<FilterException> MatchAsync(string rawFilter, T matchTarget)
    {
        try
        {
            await MustMatchAsync(rawFilter, matchTarget);
        }
        catch (FilterException e)
        {
            return e;
        }

        return OkResult;
    }

    // 不匹配时不会抛出异常而是返回匹配结果
    public async Task<FilterException> LazyMatchAsync(string rawFilter, LazyObjectGetter<T> matchTargetGetter)
    {
        try
        {
            await LazyMustMatchAsync(rawFilter, matchTargetGetter);
        }
        catch (FilterException e)
        {
            return e;
        }

        return OkResult;
    }

    // 不匹配时抛出异常
    public async Task MustMatchAsync(string rawFilter, T matchTarget)
    {
        try
        {
            var filterDocument = GetFilterDocument(rawFilter);
            foreach (var property in filterDocument.Document.RootElement.EnumerateObject())
            {
                var filter = FilterFactory.Get(property.Name);
                if (filter.GetType() == typeof(JsonPathFilter))
                {
                    await filter.MatchAsync(filterDocument.Document.RootElement, matchTarget);
                    continue;
                }
                await filter.MatchAsync(property.Value, matchTarget);
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
    public async Task LazyMustMatchAsync(string rawFilter, LazyObjectGetter<T> matchTargetGetter)
    {
        try
        {
            var filterDocument = GetFilterDocument(rawFilter);
            foreach (var property in filterDocument.Document.RootElement.EnumerateObject())
            {
                var filter = FilterFactory.Get(property.Name);
                if (filter.GetType() == typeof(JsonPathFilter))
                {
                    await filter.LazyMatchAsync(filterDocument.Document.RootElement, matchTargetGetter);
                    continue;
                }
                await filter.LazyMatchAsync(property.Value, matchTargetGetter);
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
        if (_filterOption.EnableFilterDocumentCache)
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
