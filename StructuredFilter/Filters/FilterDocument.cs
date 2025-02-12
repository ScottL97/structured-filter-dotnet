using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using StructuredFilter.Filters.Common;

namespace StructuredFilter.Filters;

public class FilterDocument<T>
{
    private string RawFilter { get; set; }
    public JsonDocument Document { get; set; }

    public FilterDocument(string rawFilter, FilterFactory<T> filterFactory)
    {
        RawFilter = Normalize(rawFilter);
        Document = JsonDocument.Parse(RawFilter);
        var root = Document.RootElement;
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

    private string Normalize(string filter)
    {
        filter = ValuesTransformToObjects(filter);
        return filter;
    }

    private string ValuesTransformToObjects(string filter)
    {
        var jsonNode = JsonNode.Parse(filter);
        switch (jsonNode)
        {
            case JsonArray array:
                ValuesTransformToObjects(array);
                break;
            case JsonObject obj:
                ValuesTransformToObjects(obj);
                break;
            default:
                throw new JsonException("json is invalid");
        }
        return jsonNode.ToJsonString();
    }

    private void ValuesTransformToObjects(JsonArray jsonArray)
    {
        for (int i = 0; i < jsonArray.Count; i++)
        {
            var item = jsonArray[i];

            if (item is JsonObject obj)
            {
                ValuesTransformToObjects(obj);
            }
            else if (item is JsonArray arr)
            {
                ValuesTransformToObjects(arr);
            }
        }
    }

    private void ValuesTransformToObjects(JsonObject jsonObject)
    {
        var itemsToTrans = new Dictionary<string, JsonNode?>();
        foreach (var property in jsonObject)
        {
            if (property.Value is JsonObject nestedObject)
            {
                ValuesTransformToObjects(nestedObject);
            }
            else if (property.Value is JsonArray array)
            {
                ValuesTransformToObjects(array);
            }
            else if (!property.Key.StartsWith('$'))
            {
                itemsToTrans.Add(property.Key, property.Value);
            }
        }

        foreach (var property in itemsToTrans)
        {
            jsonObject.Remove(property.Key);
            if (property.Value is null)
            {
                jsonObject.Add(new KeyValuePair<string, JsonNode?>(property.Key,
                    JsonNode.Parse("{\"$eq\": null}")));
            }
            else if (property.Value.GetValueKind() == JsonValueKind.String)
            {
                jsonObject.Add(new KeyValuePair<string, JsonNode?>(property.Key,
                    JsonNode.Parse($"{{\"$eq\": \"{property.Value}\"}}")));
            }
            else
            {
                jsonObject.Add(new KeyValuePair<string, JsonNode?>(property.Key,
                    JsonNode.Parse($"{{\"$eq\": {property.Value}}}")));
            }
        }
    }
}
