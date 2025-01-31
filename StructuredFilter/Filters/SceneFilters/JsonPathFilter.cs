using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using StructuredFilter.Filters.Common;
using StructuredFilter.Utils;

namespace StructuredFilter.Filters;

public static class JsonPathFilterExtension
{
    public static bool IsJsonPathFilter(this JsonProperty property)
    {
        return property.Value.ValueKind != JsonValueKind.Object;
    }
}

[FilterLabel("JsonPath")]
[FilterKey("$jsonPath")] // 实际未使用
public class JsonPathFilter(FilterFactory<JObject> filterFactory) : Filter<JObject>
{
    public override void Valid(JsonElement filterElement)
    {
        if (filterElement.ValueKind != JsonValueKind.String && filterElement.ValueKind != JsonValueKind.Number &&
            filterElement.ValueKind != JsonValueKind.True && filterElement.ValueKind != JsonValueKind.False && filterElement.ValueKind != JsonValueKind.Null)
        {
            throw new FilterException(FilterStatusCode.Invalid, $"目前 JsonPathFilter 只支持匹配 string、number、bool 和 null 值，不支持 {filterElement.ValueKind}", GetKey());
        }
    }

    public override async Task LazyMatchAsync(JsonElement filterElement, LazyObjectGetter<JObject> matchTargetGetter)
    {
        try
        {
            var matchTarget = await matchTargetGetter.GetAsync();
            Match(filterElement, matchTarget);
        }
        catch (LazyObjectGetException)
        {
            this.ThrowMatchTargetGetFailedException(matchTargetGetter.Args);
        }
    }

    public override void Match(JsonElement filterElement, JObject matchTarget)
    {
        var kv = filterElement.EnumerateObject().ToArray()[0];

        var tokens = matchTarget.SelectTokens(kv.Name).ToArray();
        if (tokens.Length == 0)
        {
            throw new FilterException(FilterStatusCode.MatchError,
                $"found no JSONPath {kv.Name} in matchTarget", kv.Name);
        }

        switch (kv.Value.ValueKind)
        {
            case JsonValueKind.String:
                MatchString(tokens, kv);
                break;
            case JsonValueKind.Number:
                MatchNumber(tokens, kv);
                break;
            case JsonValueKind.True:
            case JsonValueKind.False:
                MatchBool(tokens, kv);
                break;
            case JsonValueKind.Null:
                MatchNull(tokens, kv);
                break;
            case JsonValueKind.Undefined:
            case JsonValueKind.Object:
            case JsonValueKind.Array:
            default:
                throw new FilterException(FilterStatusCode.Invalid, $"目前 JsonPathFilter 只支持匹配 string、number、bool 和 null 值，不支持 {filterElement.ValueKind}", GetKey());
        }
    }

    private void MatchString(JToken[] tokens, JsonProperty property)
    {
        if (tokens.Length != 1)
        {
            throw new FilterException(FilterStatusCode.MatchError,
                $"JSONPath {property.Name} matched {tokens.Length} paths, but expected 1", property.Name);
        }

        if (tokens[0].Type != JTokenType.String)
        {
            throw new FilterException(FilterStatusCode.MatchError,
                $"JSONPath {property.Name} value {tokens[0]} expected string, got {tokens[0].Type}", property.Name);
        }

        try
        {
            var filter = filterFactory.StringFilterFactory.Get("$eq");
            filter.Match(property.Value, tokens[0].ToString());
        }
        catch (FilterException e)
        {
            throw e.PrependFailedKey(property.Name);
        }
    }

    private void MatchNumber(JToken[] tokens, JsonProperty property)
    {
        if (tokens.Length != 1)
        {
            throw new FilterException(FilterStatusCode.MatchError,
                $"JSONPath {property.Name} matched {tokens.Length} paths, but expected 1", property.Name);
        }

        if (!double.TryParse(tokens[0].ToString(), out double value))
        {
            throw new FilterException(FilterStatusCode.MatchError,
                $"JSONPath {property.Name} value {tokens[0]} expected double number, got {tokens[0].Type}", property.Name);
        }

        try
        {
            var filter = filterFactory.NumberFilterFactory.Get("$eq");
            filter.Match(property.Value, value);
        }
        catch (FilterException e)
        {
            throw e.PrependFailedKey(property.Name);
        }
    }

    private void MatchBool(JToken[] tokens, JsonProperty property)
    {
        if (tokens.Length != 1)
        {
            throw new FilterException(FilterStatusCode.MatchError,
                $"JSONPath {property.Name} matched {tokens.Length} paths, but expected 1", property.Name);
        }

        if (!bool.TryParse(tokens[0].ToString(), out bool value))
        {
            throw new FilterException(FilterStatusCode.MatchError,
                $"JSONPath {property.Name} value {tokens[0]} expected bool, got {tokens[0].Type}", property.Name);
        }

        try
        {
            var filter = filterFactory.BoolFilterFactory.Get("$eq");
            filter.Match(property.Value, value);
        }
        catch (FilterException e)
        {
            throw e.PrependFailedKey(property.Name);
        }
    }

    private void MatchNull(JToken[] tokens, JsonProperty property)
    {
        if (tokens.Length != 1)
        {
            throw new FilterException(FilterStatusCode.MatchError,
                $"JSONPath {property.Name} matched {tokens.Length} paths, but expected 1", property.Name);
        }

        if (tokens[0].Type != JTokenType.Null)
        {
            throw new FilterException(FilterStatusCode.NotMatched,
                $"JSONPath {property.Name} value {tokens[0]} expected null, got {tokens[0].Type}", property.Name);
        }
    }
}