using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace StructuredFilter.Filters.Common;

public readonly record struct FilterObject(
    string Key,
    FilterKv? FilterKv,
    FilterArray? FilterArray);

public readonly record struct FilterArray(
    IEnumerable<FilterObject> FilterObjects);

public readonly record struct FilterKv(string Key, JsonElement Value);

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

        var kvCount = root.EnumerateObject().Count();
        if (kvCount != 1)
        {
            throw new FilterException(FilterStatusCode.Invalid, $"对象键值对数需要为 1，但 filter 根节点对象 {root} 有 {kvCount} 对键值对", "<UNKNOWN>");
        }

        var rootObject = root.EnumerateObject().First();
        if (rootObject.Value.ValueKind == JsonValueKind.Array)
        {
            var filter = filterFactory.GetLogicFilter(rootObject.Name);
            filter.Valid(rootObject.Value);
        }
        else
        {
            var filter = filterFactory.GetSceneFilter(rootObject.Name);
            filter.Valid(rootObject.Value);
        }

        var filterTree = new FilterTree();
        
        if (rootObject.Value.ValueKind == JsonValueKind.Array)
        {
            filterTree.Root = new FilterObject(rootObject.Name, null, ParseFilterArray(rootObject));
        }
        else if (rootObject.Value.ValueKind == JsonValueKind.Object)
        {
            filterTree.Root = new FilterObject(rootObject.Name, ParseFilterKv(rootObject), null);
        }
        else
        {
            throw new FilterException(FilterStatusCode.Invalid, $"无效的 filter 根节点值类型：{rootObject.Value.ValueKind}", "<UNKNOWN>");
        }

        return filterTree;
    }

    private static FilterArray ParseFilterArray(JsonProperty rootObject)
    {
        return new FilterArray(rootObject.Value.EnumerateArray().Select(element =>
        {
            var filterObject = element.EnumerateObject().First();
            var filterKv = filterObject.Value.EnumerateObject().First();
            return new FilterObject(filterObject.Name, new FilterKv(filterKv.Name, filterKv.Value), null);
        }));
    }

    private static FilterKv ParseFilterKv(JsonProperty rootObject)
    {
        var filterKv = rootObject.Value.EnumerateObject().First();
        return new FilterKv(filterKv.Name, filterKv.Value);
    }
}
