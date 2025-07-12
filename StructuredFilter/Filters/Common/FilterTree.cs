using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using static StructuredFilter.Filters.Common.FilterValue;

namespace StructuredFilter.Filters.Common;

public readonly record struct FilterObject(
    string Key,
    FilterKv? FilterKv,
    FilterArray? FilterArray);

public readonly record struct FilterArray(
    Lazy<FilterObject[]> FilterObjects);

public readonly record struct FilterKv(string Key, FilterValue Value);

public class FilterTree
{
    public FilterObject Root;
    public static FilterTree Parse<T>(string rawFilter, FilterFactory<T> filterFactory)
    {
        var document = JsonDocument.Parse(rawFilter);
        var root = document.RootElement;
        if (root.ValueKind != JsonValueKind.Object)
        {
            throw new FilterException(FilterStatusCode.Invalid, $"无效的 filter 根节点类型：{root.ValueKind}", "<UNKNOWN>");
        }

        var kvCount = root.GetPropertyCount();
        if (kvCount != 1)
        {
            throw new FilterException(FilterStatusCode.Invalid, $"对象键值对数需要为 1，但 filter 根节点对象 {root} 有 {kvCount} 对键值对", "<UNKNOWN>");
        }

        var rootObject = root.EnumerateObject().First();
        if (rootObject.Value.ValueKind == JsonValueKind.Array)
        {
            var (filter, getResult) = filterFactory.GetLogicFilter(rootObject.Name);
            if (getResult is not null)
            {
                throw getResult;
            }
            var checkResult = filter.Valid(rootObject.Value);
            if (checkResult is not null)
            {
                throw checkResult;
            }
        }
        else
        {
            var (filter, getResult) = filterFactory.GetSceneFilter(rootObject.Name);
            if (getResult is not null)
            {
                throw getResult;
            }
            var checkResult = filter.Valid(rootObject.Value);
            if (checkResult is not null)
            {
                throw checkResult;
            }
        }

        var filterTree = new FilterTree();
        
        if (rootObject.Value.ValueKind == JsonValueKind.Array)
        {
            filterTree.Root = new FilterObject(rootObject.Name, null, ParseFilterArray(rootObject, filterFactory));
        }
        else if (rootObject.Value.ValueKind == JsonValueKind.Object)
        {
            filterTree.Root = new FilterObject(rootObject.Name, ParseFilterKv(rootObject, filterFactory), null);
        }
        else
        {
            throw new FilterException(FilterStatusCode.Invalid, $"无效的 filter 根节点值类型：{rootObject.Value.ValueKind}", "<UNKNOWN>");
        }

        return filterTree;
    }

    private static FilterArray ParseFilterArray<T>(JsonProperty rootObject, FilterFactory<T> filterFactory)
    {
        return new FilterArray(new Lazy<FilterObject[]>(() =>
            [.. rootObject.Value.EnumerateArray().Select(element =>
            {
                var filterObject = element.EnumerateObject().First();
                var filterKv = filterObject.Value.EnumerateObject().First();

                var filterValue = CreateFilterValueWithTypedArray(filterKv.Value, filterObject.Name, filterKv.Name, filterFactory);
                return new FilterObject(filterObject.Name, new FilterKv(filterKv.Name, filterValue), null);
            })]));
    }

    private static FilterKv ParseFilterKv<T>(JsonProperty rootObject, FilterFactory<T> filterFactory)
    {
        var filterKv = rootObject.Value.EnumerateObject().First();

        var filterValue = CreateFilterValueWithTypedArray(filterKv.Value, rootObject.Name, filterKv.Name, filterFactory);
        return new FilterKv(filterKv.Name, filterValue);
    }

    private static FilterValueKind GetTargetFilterValueKind<T>(JsonElement element, string sceneFilterName, string basicFilterName, FilterFactory<T> filterFactory)
    {
        if (element.ValueKind == JsonValueKind.Array)
        {
            return FilterValueKind.Array;
        }

        if (element.ValueKind == JsonValueKind.Object)
        {
            return FilterValueKind.Object;
        }

        if (element.ValueKind == JsonValueKind.Null)
        {
            return FilterValueKind.Null;
        }

        // For primitive types, try to use the scene filter's basic type for better type accuracy
        var (sceneFilter, getResult) = filterFactory.GetSceneFilter(sceneFilterName);
        if (getResult is null)
        {
            var basicType = sceneFilter.GetBasicType();
            return FilterValue.BasicTypeToValueKind(basicType);
        }

        throw new FilterException(FilterStatusCode.Invalid, $"无法获取场景过滤器 {sceneFilterName} 的基本类型: {getResult.Message}", sceneFilterName);
    }

    private static FilterValue CreateFilterValueWithTypedArray<T>(JsonElement element, string sceneFilterName, string basicFilterName, FilterFactory<T> filterFactory)
    {
        if (element.ValueKind == JsonValueKind.Array)
        {
            // Get the element type from the scene filter
            var (sceneFilter, getResult) = filterFactory.GetSceneFilter(sceneFilterName);
            if (getResult is not null)
            {
                throw new FilterException(FilterStatusCode.Invalid, $"无法获取场景过滤器 {sceneFilterName} 的基本类型: {getResult.Message}", sceneFilterName);
            }

            var elementBasicType = sceneFilter.GetBasicType();
            var elementTargetKind = FilterValue.BasicTypeToValueKind(elementBasicType);

            // Convert each array element with the correct type
            var typedElements = element.EnumerateArray()
                .Select(arrayElement => FilterValue.FromJsonElement(arrayElement, elementTargetKind))
                .ToArray();

            return FilterValue.FromArray(typedElements);
        }

        // For non-array elements, use the regular method
        var targetKind = GetTargetFilterValueKind(element, sceneFilterName, basicFilterName, filterFactory);
        return FilterValue.FromJsonElement(element, targetKind);
    }
}
