using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StructuredFilter.Filters.BasicFilters;
using StructuredFilter.Filters.Common;
using StructuredFilter.Filters.Common.FilterTypes;
using StructuredFilter.Filters.LogicFilters;
using StructuredFilter.Filters.SceneFilters;
using StructuredFilter.Filters.SceneFilters.Scenes;

namespace StructuredFilter.Filters;

public interface IFilterFactory<T>
{
    Dictionary<string, IFilter<T>> GetAll();
}

public interface IRootFilterFactory<T> : IFilterFactory<T>
{
    (ISceneFilter<T>, FilterException?) GetSceneFilter(string key);
    (ILogicFilter<T>, FilterException?) GetLogicFilter(string key);
}

public interface ILogicFilterFactory<T> : IFilterFactory<T>
{
    (ILogicFilter<T>, FilterException?) Get(string key);
}

public interface ISceneFilterFactory<T> : IFilterFactory<T>
{
    (ISceneFilter<T>, FilterException?) Get(string key);
}

public interface IBasicFilterFactory<T> : IFilterFactory<T>
{
    (IBasicFilter<T>, FilterException?) Get(string key);
}

public class FilterFactory<T> : IRootFilterFactory<T>
{
    public readonly NumberFilterFactory NumberFilterFactory;
    public readonly BoolFilterFactory BoolFilterFactory;
    public readonly StringFilterFactory StringFilterFactory;
    public readonly VersionFilterFactory VersionFilterFactory;

    private readonly SceneFilterFactory<T> _sceneFilterFactory;
    private readonly LogicFilterFactory<T> _logicFilterFactory;

    public FilterFactory()
    {
        _sceneFilterFactory = new SceneFilterFactory<T>([]);
        _logicFilterFactory = new LogicFilterFactory<T>([
            new AndFilter<T>(_sceneFilterFactory),
            new OrFilter<T>(_sceneFilterFactory)
        ]);
        NumberFilterFactory = new NumberFilterFactory([
            new NumberInFilter(),
            new NumberEqFilter(),
            new NumberNeFilter(),
            new GreaterOrEqualFilter(),
            new LessOrEqualFilter(),
            new GreaterThanFilter(),
            new LessThanFilter(),
            new NumberRangeFilter()
        ]);
        BoolFilterFactory = new BoolFilterFactory([
            new BoolEqFilter(),
            new BoolNeFilter()
        ]);
        StringFilterFactory = new StringFilterFactory([
            new StringEqFilter(),
            new StringNeFilter(),
            new StringInFilter(),
            new StringRegexFilter(),
            new StringRangeFilter()
        ]);
        VersionFilterFactory = new VersionFilterFactory([
            new VersionEqFilter(),
            new VersionNeFilter(),
            new VersionInFilter(),
            new VersionGreaterOrEqualFilter(),
            new VersionLessOrEqualFilter(),
            new VersionGreaterThanFilter(),
            new VersionLessThanFilter(),
            new VersionRangeFilter()
        ]);
    }

    public FilterFactory<T> WithLogicFilters(IEnumerable<ILogicFilter<T>> logicFilters)
    {
        foreach (var logicFilter in logicFilters)
        {
            _logicFilterFactory.AddFilter(logicFilter);
        }

        return this;
    }

    public FilterFactory<T> WithDynamicFilter(DynamicFilter<T> df,
        bool enableOverride = false,
        Func<T?, Task<bool>>? boolValueGetter = null,
        Func<T?, Task<double>>? numberValueGetter = null,
        Func<T?, Task<string>>? stringValueGetter = null,
        Func<T?, Task<Version>>? versionValueGetter = null)
    {
        if (!FilterBasicType.IsValidFilterBasicType(df.BasicType))
        {
            throw new FilterException(FilterStatusCode.Invalid, $"found invalid dynamic filter type {df.BasicType} with key {df.Key}");
        }

        try
        {
            var filter = df.BasicType switch
            {
                FilterBasicType.Bool => (ISceneFilter<T>)new DynamicBoolSceneFilter<T>(this,
                    (player, filterKey) => boolValueGetter(player), df.Key, df.Cacheable, df.Label, df.Cache),
                FilterBasicType.Number => (ISceneFilter<T>)new DynamicNumberSceneFilter<T>(this,
                    (player, filterKey) => numberValueGetter(player), df.Key, df.Cacheable, df.Label, df.Cache),
                FilterBasicType.String => (ISceneFilter<T>)new DynamicStringSceneFilter<T>(this,
                    (player, filterKey) => stringValueGetter(player), df.Key, df.Cacheable, df.Label, df.Cache),
                FilterBasicType.Version => (ISceneFilter<T>)new DynamicVersionSceneFilter<T>(this,
                    (player, filterKey) => versionValueGetter(player), df.Key, df.Cacheable, df.Label, df.Cache),
                _ => throw new FilterException(FilterStatusCode.OptionError, $"unknown filter basic type {df.BasicType} with key {df.Key}")
            };

            return WithSceneFilter(filter, enableOverride);
        }
        catch (NullReferenceException e)
        {
            throw new FilterException(FilterStatusCode.Invalid, $"valueGetter for {df.BasicType} is null");
        }
    }

    public FilterFactory<T> WithSceneFilter(ISceneFilter<T> sceneFilter, bool enableOverride = false)
    {
        _sceneFilterFactory.AddFilter(sceneFilter, enableOverride);

        return this;
    }

    public FilterFactory<T> WithSceneFilters(IEnumerable<ISceneFilter<T>> sceneFilters, bool enableOverride = false)
    {
        foreach (var sceneFilter in sceneFilters)
        {
            WithSceneFilter(sceneFilter, enableOverride);
        }

        return this;
    }

    public async Task<FilterFactory<T>> LoadDynamicSceneFiltersAsync(FilterOption<T> filterOption)
    {
        if (filterOption.IsDynamicFiltersGetterConfigured() && !filterOption.IsDynamicSceneFilterValueGetterConfigured())
        {
            throw new FilterException(FilterStatusCode.OptionError,
                "DynamicFiltersGetter is configured but no DynamicSceneFilterGetters are configured");
        }

        var filters = await filterOption.DynamicFiltersGetter!();
        WithSceneFilters(filters.Select(f =>
        {
            if (!FilterBasicType.IsValidFilterBasicType(f.BasicType))
            {
                throw new FilterException(FilterStatusCode.OptionError, $"found invalid dynamic filter type {f.BasicType} with key {f.Key}");
            }

            return f.BasicType switch
            {
                FilterBasicType.Bool => (ISceneFilter<T>)new DynamicBoolSceneFilter<T>(this,
                    filterOption.DynamicBoolSceneFilterValueGetter!, f.Key, f.Cacheable, f.Label, f.Cache),
                FilterBasicType.Number => (ISceneFilter<T>)new DynamicNumberSceneFilter<T>(this,
                    filterOption.DynamicNumberSceneFilterValueGetter!, f.Key, f.Cacheable, f.Label, f.Cache),
                FilterBasicType.String => (ISceneFilter<T>)new DynamicStringSceneFilter<T>(this,
                    filterOption.DynamicStringSceneFilterValueGetter!, f.Key, f.Cacheable, f.Label, f.Cache),
                FilterBasicType.Version => (ISceneFilter<T>)new DynamicVersionSceneFilter<T>(this,
                    filterOption.DynamicVersionSceneFilterValueGetter!, f.Key, f.Cacheable, f.Label, f.Cache),
                _ => throw new FilterException(FilterStatusCode.OptionError, $"unknown filter basic type {f.BasicType} with key {f.Key}")
            };
        }), true);

        return this;
    }

    public (ISceneFilter<T>, FilterException?) GetSceneFilter(string key)
    {
        return _sceneFilterFactory.Get(key);
    }

    public (ILogicFilter<T>, FilterException?) GetLogicFilter(string key)
    {
        return _logicFilterFactory.Get(key);
    }

    public Dictionary<string, IFilter<T>> GetAll()
    {
        return _sceneFilterFactory.GetAll();
    }

    public Dictionary<string, SceneFilterInfo> GetSceneFilterInfos()
    {
        return _sceneFilterFactory.GetAll().ToDictionary(kv => kv.Key, kv =>
        {
            var sceneFilter = kv.Value;
            var sceneFilterInfo = new SceneFilterInfo
            {
                Label = sceneFilter.GetLabel(),
                Type = sceneFilter.GetBasicType()
            };

            sceneFilterInfo.Logics = sceneFilterInfo.Type switch
            {
                FilterBasicType.Bool => BoolFilterFactory.GetAll()
                    .Select(kv =>
                    {
                        var boolFilter = kv.Value;
                        return new OperatorInfo { Label = boolFilter.GetLabel(), Value = boolFilter.GetKey() };
                    })
                    .ToArray(),
                FilterBasicType.Number => NumberFilterFactory.GetAll()
                    .Select(kv =>
                    {
                        var numberFilter = kv.Value;
                        return new OperatorInfo { Label = numberFilter.GetLabel(), Value = numberFilter.GetKey() };
                    })
                    .ToArray(),
                FilterBasicType.String => StringFilterFactory.GetAll()
                    .Select(kv =>
                    {
                        var stringFilter = kv.Value;
                        return new OperatorInfo { Label = stringFilter.GetLabel(), Value = stringFilter.GetKey() };
                    })
                    .ToArray(),
                FilterBasicType.Version => VersionFilterFactory.GetAll()
                    .Select(kv =>
                    {
                        var versionFilter = kv.Value;
                        return new OperatorInfo { Label = versionFilter.GetLabel(), Value = versionFilter.GetKey() };
                    })
                    .ToArray(),
                _ => []
            };

            return sceneFilterInfo;
        });
    }
}