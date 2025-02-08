using StructuredFilter.Filters.Common.FilterTypes;
using StructuredFilter.Filters.SceneFilters.Scenes;

namespace StructuredFilter.Filters.SceneFilters;

// TODO: 支持多种基本类型 filter，当前仅支持 STRING 类型
public class DynamicSceneFilter<T> : StringSceneFilter<T>
{
    public DynamicSceneFilter(FilterFactory<T> filterFactory,
        FilterOption<T>.GetDynamicSceneFilterValueAsync dynamicSceneFilterValueGetter,
        string key,
        string basicType,
        string? label = null) : base(filterFactory, matchTarget => dynamicSceneFilterValueGetter(matchTarget, key))
    {
        KeyOverride = key;
        BasicTypeOverride = basicType;
        LabelOverride = label;
    }
}

public record DynamicFilter(string Key, string BasicType=FilterBasicType.String, string? Label=null);
