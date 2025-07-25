﻿using System;
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
    /// Whether to allow overwriting SceneFilter with the same key;
    /// if not allowed, adding a SceneFilter with an existing key will throw a FilterException
    /// </summary>
    public bool IsSceneFilterOverrideAllowed { get; set; } = false;

    /// <summary>
    /// Methods to obtain dynamic filters' values from match target according to the filter key
    /// </summary>
    public delegate ValueTask<bool> GetDynamicBoolSceneFilterValueAsync(T? matchTarget, string filterKey);
    public delegate ValueTask<double> GetDynamicDoubleSceneFilterValueAsync(T? matchTarget, string filterKey);
    public delegate ValueTask<long> GetDynamicLongSceneFilterValueAsync(T? matchTarget, string filterKey);
    public delegate ValueTask<string> GetDynamicStringSceneFilterValueAsync(T? matchTarget, string filterKey);
    public delegate ValueTask<Version> GetDynamicVersionSceneFilterValueAsync(T? matchTarget, string filterKey);

    public GetDynamicBoolSceneFilterValueAsync? DynamicBoolSceneFilterValueGetter { get; set; } = null;
    public GetDynamicDoubleSceneFilterValueAsync? DynamicDoubleSceneFilterValueGetter { get; set; } = null;
    public GetDynamicLongSceneFilterValueAsync? DynamicLongSceneFilterValueGetter { get; set; } = null;
    public GetDynamicStringSceneFilterValueAsync? DynamicStringSceneFilterValueGetter { get; set; } = null;
    public GetDynamicVersionSceneFilterValueAsync? DynamicVersionSceneFilterValueGetter { get; set; } = null;

    /// <summary>
    /// Method to obtain dynamic filters
    /// </summary>
    public delegate ValueTask<DynamicFilter<T>[]> GetDynamicFiltersAsync();
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
               filterOption.DynamicDoubleSceneFilterValueGetter is not null ||
               filterOption.DynamicLongSceneFilterValueGetter is not null ||
               filterOption.DynamicStringSceneFilterValueGetter is not null ||
               filterOption.DynamicVersionSceneFilterValueGetter is not null;
    }
}
