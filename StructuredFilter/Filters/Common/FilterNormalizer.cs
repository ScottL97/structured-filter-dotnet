using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using JsonException = System.Text.Json.JsonException;

namespace StructuredFilter.Filters.Common;

public static class FilterNormalizer
{
    public static string Normalize(string filter)
    {
        try
        {
            filter = ValuesTransformToObjects(filter);
            return filter;
        }
        catch (JsonException e)
        {
            throw new FilterException(e, FilterStatusCode.Invalid, "Filter 不是有效的 JSON");
        }
    }

    private static string ValuesTransformToObjects(string filter)
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

    private static void ValuesTransformToObjects(JsonArray jsonArray)
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

    private static void ValuesTransformToObjects(JsonObject jsonObject)
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