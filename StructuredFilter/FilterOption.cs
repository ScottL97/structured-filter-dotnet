using System;
using System.Threading.Tasks;
using StructuredFilter.Filters.SceneFilters;

namespace StructuredFilter;

public class FilterOption<T>
{
    /// <summary>
    /// Whether to cache FilterDocument
    /// </summary>
    public bool EnableFilterDocumentCache { get; set; } = true;

    /// <summary>
    /// Methods to obtain dynamic filters' values from match target according to the filter key
    /// </summary>
    public delegate Task<bool> GetDynamicBoolSceneFilterValueAsync(T? matchTarget, string filterKey);
    public delegate Task<double> GetDynamicNumberSceneFilterValueAsync(T? matchTarget, string filterKey);
    public delegate Task<string> GetDynamicStringSceneFilterValueAsync(T? matchTarget, string filterKey);
    public delegate Task<Version> GetDynamicVersionSceneFilterValueAsync(T? matchTarget, string filterKey);

    public GetDynamicBoolSceneFilterValueAsync? DynamicBoolSceneFilterValueGetter { get; set; } = null;
    public GetDynamicNumberSceneFilterValueAsync? DynamicNumberSceneFilterValueGetter { get; set; } = null;
    public GetDynamicStringSceneFilterValueAsync? DynamicStringSceneFilterValueGetter { get; set; } = null;
    public GetDynamicVersionSceneFilterValueAsync? DynamicVersionSceneFilterValueGetter { get; set; } = null;

    /// <summary>
    /// Method to obtain dynamic filters
    /// </summary>
    public delegate Task<DynamicFilter[]> GetDynamicFiltersAsync();
    public GetDynamicFiltersAsync? DynamicFiltersGetter { get; set; } = null;
}

public static class FilterOptionsExtension
{
    public static bool IsDynamicFiltersGetterConfigured<T>(this FilterOption<T> filterOption)
    {
        return filterOption.DynamicFiltersGetter is not null;
    }

    public static bool IsDynamicSceneFilterValueGetterConfigured<T>(this FilterOption<T> filterOption)
    {
        return filterOption.DynamicBoolSceneFilterValueGetter is not null ||
               filterOption.DynamicNumberSceneFilterValueGetter is not null ||
               filterOption.DynamicStringSceneFilterValueGetter is not null ||
               filterOption.DynamicVersionSceneFilterValueGetter is not null;
    }
}
