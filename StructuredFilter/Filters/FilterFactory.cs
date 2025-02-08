using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StructuredFilter.Filters.BasicFilters;
using StructuredFilter.Filters.Common;
using StructuredFilter.Filters.Common.FilterTypes;
using StructuredFilter.Filters.LogicFilters;
using StructuredFilter.Filters.SceneFilters;

namespace StructuredFilter.Filters;

public interface IFilterFactory<T>
{
    IFilter<T> Get(string key);
    Dictionary<string, IFilter<T>> GetAll();
}

public class FilterFactory<T> : IFilterFactory<T>
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

    public FilterFactory<T> WithLogicFilters(IEnumerable<IFilter<T>> logicFilters)
    {
        foreach (var logicFilter in logicFilters)
        {
            _logicFilterFactory.AddFilter(logicFilter);
        }

        return this;
    }

    public FilterFactory<T> WithSceneFilter(IFilter<T> sceneFilter)
    {
        _sceneFilterFactory.AddFilter(sceneFilter);

        return this;
    }

    public FilterFactory<T> WithSceneFilters(IEnumerable<IFilter<T>> sceneFilters)
    {
        foreach (var sceneFilter in sceneFilters)
        {
            WithSceneFilter(sceneFilter);
        }

        return this;
    }

    public async Task<FilterFactory<T>> LoadDynamicSceneFiltersAsync(FilterOption<T>.GetDynamicFiltersAsync dynamicFiltersGetter,
        FilterOption<T>.GetDynamicSceneFilterValueAsync dynamicSceneFilterValueGetter)
    {
        var filters = await dynamicFiltersGetter();
        WithSceneFilters(filters.Select(f =>
        {
            if (!FilterBasicType.IsValidFilterBasicType(f.BasicType))
            {
                throw new FilterException(FilterStatusCode.OptionError, $"found invalid dynamic filter type {f.BasicType} with key {f.Key}");
            }
            return new DynamicSceneFilter<T>(this, dynamicSceneFilterValueGetter, f.Key, f.BasicType, f.Label);
        }));

        return this;
    }

    public IFilter<T> Get(string key)
    {
        if (key.StartsWith("$.") || key.StartsWith("$["))
        {
            return _sceneFilterFactory.Get(Consts.JsonPathFilterKey);
        }
        return _logicFilterFactory.GetAll()
            .Concat(_sceneFilterFactory.GetAll())
            .First(x => x.Key == key).Value;
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