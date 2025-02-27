using System.Linq;
using System.Text.Json;

namespace StructuredFilter.Filters.Common;

public static class FilterValidator
{
    public static void MustValid<T>(string rawFilter, FilterFactory<T> filterFactory)
    {
        if (string.IsNullOrWhiteSpace(rawFilter))
        {
            throw new FilterException(FilterStatusCode.Invalid, "Filter cannot be empty");
        }

        rawFilter = FilterNormalizer.Normalize(rawFilter);
        var document = JsonDocument.Parse(rawFilter);

        MustValid(document, filterFactory);
    }

    public static void MustValid<T>(JsonDocument document, FilterFactory<T> filterFactory)
    {
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

        foreach (var property in root.EnumerateObject())
        {
            var filter = filterFactory.Get(property.Name);
            filter.Valid(property.Value);
        }
    }
}
