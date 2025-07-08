using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StructuredFilter.Filters;
using StructuredFilter.Filters.Common;
using StructuredFilter.Filters.SceneFilters;
using StructuredFilter.Utils;

namespace StructuredFilter;

public class FilterService<T>(FilterFactory<T>? filterFactor=null, FilterOption<T>? option=null)
{
    public delegate ISceneFilter<T> SceneFilterCreator(FilterFactory<T> filterFactory);

    private readonly ConcurrentDictionary<string, FilterDocument<T>> _filterDocuments = new ();

    private readonly FilterFactory<T> _filterFactory = filterFactor ?? new FilterFactory<T>();
    private readonly FilterOption<T> _filterOption = option ?? new FilterOption<T>();

    public void MustValidFilter(string filter)
    {
        FilterValidator.MustValid(filter, _filterFactory);
    }

    public async Task<FilterService<T>> LoadDynamicSceneFiltersAsync()
    {
        await _filterFactory.LoadDynamicSceneFiltersAsync(_filterOption);

        return this;
    }

    public FilterService<T> WithDynamicFilter(DynamicFilter<T> df,
        Func<T?, ValueTask<bool>>? boolValueGetter = null,
        Func<T?, ValueTask<double>>? doubleValueGetter = null,
        Func<T?, ValueTask<long>>? longValueGetter = null,
        Func<T?, ValueTask<string>>? stringValueGetter = null,
        Func<T?, ValueTask<Version>>? versionValueGetter = null)
    {
        _filterFactory.WithDynamicFilter(df, _filterOption.IsSceneFilterOverrideAllowed, boolValueGetter, doubleValueGetter, longValueGetter, stringValueGetter, versionValueGetter);
        return this;
    }

    public FilterService<T> WithSceneFilter(SceneFilterCreator sceneFilterCreator)
    {
        _filterFactory.WithSceneFilter(sceneFilterCreator(_filterFactory), _filterOption.IsSceneFilterOverrideAllowed);
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

    private static readonly FilterException RawFilterStringEmptyException = new (FilterStatusCode.Invalid,
        "Filter cannot be empty");
    private static readonly FilterException WrongFilterRootTypeException = new (FilterStatusCode.Invalid,
        "filter root is neither a logic filter nor a scene filter");

    // 不匹配时不会抛出异常而是返回匹配结果
    public async ValueTask<FilterException> MatchAsync(string rawFilter, T matchTarget)
    {
        if (string.IsNullOrWhiteSpace(rawFilter))
        {
            return RawFilterStringEmptyException;
        }

        FilterDocument<T> filterDocument;
        try
        {
            filterDocument = GetFilterDocument(rawFilter);
        }
        catch (FilterException e)
        {
            return e;
        }

        if (filterDocument.IsRootLogicFilter())
        {
            var (filter, getResult) = _filterFactory.GetLogicFilter(filterDocument.GetRootKey());
            if (getResult is not null)
            {
                return getResult;
            }
            var filterResult = await filter.MatchAsync(filterDocument.GetRootFilterArray(), matchTarget);
            return filterResult ?? FilterException.Ok;
        }

        if (filterDocument.IsRootSceneFilter())
        {
            var (filter, getResult) = _filterFactory.GetSceneFilter(filterDocument.GetRootKey());
            if (getResult is not null)
            {
                return getResult;
            }
            var filterResult = await filter.MatchAsync(filterDocument.GetRootFilterKv(), matchTarget);
            return filterResult ?? FilterException.Ok;
        }

        return WrongFilterRootTypeException;
    }

    // 不匹配时不会抛出异常而是返回匹配结果
    public async ValueTask<FilterException> LazyMatchAsync(string rawFilter, LazyObjectGetter<T> matchTargetGetter)
    {
        if (string.IsNullOrWhiteSpace(rawFilter))
        {
            return RawFilterStringEmptyException;
        }

        FilterDocument<T> filterDocument;
        try
        {
            filterDocument = GetFilterDocument(rawFilter);
        }
        catch (FilterException e)
        {
            return e;
        }

        if (filterDocument.IsRootLogicFilter())
        {
            var (filter, getResult) = _filterFactory.GetLogicFilter(filterDocument.GetRootKey());
            if (getResult is not null)
            {
                return getResult;
            }
            var filterResult = await filter.LazyMatchAsync(filterDocument.GetRootFilterArray(), matchTargetGetter);
            return filterResult ?? FilterException.Ok;
        }

        if (filterDocument.IsRootSceneFilter())
        {
            var (filter, getResult) = _filterFactory.GetSceneFilter(filterDocument.GetRootKey());
            if (getResult is not null)
            {
                return getResult;
            }
            var filterResult = await filter.LazyMatchAsync(filterDocument.GetRootFilterKv(), matchTargetGetter);
            return filterResult ?? FilterException.Ok;
        }

        return WrongFilterRootTypeException;
    }

    // 不匹配时抛出异常
    public async ValueTask MustMatchAsync(string rawFilter, T matchTarget)
    {
        var filterResult = await MatchAsync(rawFilter, matchTarget);
        if (filterResult.StatusCode == FilterStatusCode.Ok)
        {
            return;
        }

        throw filterResult;
    }

    // 不匹配时抛出异常
    public async ValueTask LazyMustMatchAsync(string rawFilter, LazyObjectGetter<T> matchTargetGetter)
    {
        var filterResult = await LazyMatchAsync(rawFilter, matchTargetGetter);
        if (filterResult.StatusCode == FilterStatusCode.Ok)
        {
            return;
        }

        throw filterResult;
    }

    public FilterTree GetFilterTree(string rawFilter)
    {
        var filterDocument = GetFilterDocument(rawFilter);
        return filterDocument.Tree;
    }

    private FilterDocument<T> GetFilterDocument(string rawFilter)
    {
        if (_filterOption.EnableFilterDocumentCache)
        {
            return _filterDocuments.GetOrAdd(rawFilter, _ => new FilterDocument<T>(rawFilter, _filterFactory));
        }

        return new FilterDocument<T>(rawFilter, _filterFactory);
    }

    public Dictionary<string, SceneFilterInfo> GetSceneFilterInfos()
    {
        return _filterFactory.GetSceneFilterInfos();
    }
}
