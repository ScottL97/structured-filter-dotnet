using System;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace StructuredFilter.Filters.Common.FilterTypes;

public static class FilterBasicType
{
    public const string Bool = "BOOL";
    public const string Double = "DOUBLE";
    public const string Long = "LONG";
    public const string String = "STRING";
    public const string Version = "VERSION";

    public static bool IsValidFilterBasicType(string t)
    {
        return t is Bool or Double or Long or String or Version;
    }
}

public static class FilterTypeChecker
{
    public delegate FilterException? JsonPropertyChecker(JsonProperty jsonProperty);
    public static FilterException? AssertIsValidObject<T>(this JsonElement element, IFilter<T> filter, JsonPropertyChecker jsonPropertyChecker)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            return filter.CreateFilterValueTypeMismatchException(element, JsonValueKind.Object);
        }

        var kvCount = element.GetPropertyCount();
        if (kvCount != 1)
        {
            return new FilterException(FilterStatusCode.Invalid, $"对象键值对数需要为 1，{element} 有 {kvCount} 对键值对");
        }

        foreach (var property in element.EnumerateObject())
        {
            var checkResult = jsonPropertyChecker(property);
            if (checkResult is not null)
            {
                return checkResult;
            }
        }

        return null;
    }

    public static FilterException? AssertIsValidArray<T>(this JsonElement element, IFilter<T> filter, int? assertKvCount = null)
    {
        if (element.ValueKind != JsonValueKind.Array)
        {
            return filter.CreateWrongFilterValueTypeException(element, JsonValueKind.Array);
        }

        var kvCount = element.GetArrayLength();
        if (kvCount == 0)
        {
            return new FilterException(FilterStatusCode.Invalid, $"array elements count should be more than 0, {typeof(IFilter<T>)} value {element.ToString()} has 0", filter.GetKey());
        }

        if (assertKvCount != null)
        {
            if (assertKvCount.Value != kvCount)
            {
                return new FilterException(FilterStatusCode.Invalid, $"array elements count should be {assertKvCount.Value}, {typeof(IFilter<T>)} value {element.ToString()} has {kvCount}", filter.GetKey());
            }
        }

        foreach (var e in element.EnumerateArray())
        {
            var checkResult = e.AssertIsRightElementType(filter);
            if (checkResult is not null)
            {
                return checkResult;
            }
        }

        return null;
    }

    public static FilterException? AssertIsValidRange<T>(this JsonElement element, IFilter<T> filter)
    {
        var checkResult = element.AssertIsValidArray(filter, 2);
        if (checkResult is not null)
        {
            return checkResult;
        }

        var (compareResult, validResult) = element[1].CompareTo(filter, element[0]);
        if (validResult is not null)
        {
            return validResult;
        }
        if (compareResult < 0)
        {
            return new FilterException(FilterStatusCode.Invalid,
                $"the second element of the range {element[1]} is not >= the first element {element[0]}", filter.GetKey());
        }

        return null;
    }

    private static (int, FilterException?) CompareTo<T>(this JsonElement element, IFilter<T> filter, JsonElement other)
    {
        return typeof(T) switch
        {
            { } stringType when stringType == typeof(string) => (string.CompareOrdinal(element.GetString(),
                other.ToString()), null),
            { } doubleType when doubleType == typeof(double) => (element.GetDouble().CompareTo(other.GetDouble()), null),
            { } longType when longType == typeof(long) => (element.GetInt64().CompareTo(other.GetInt64()), null),
            { } versionType when versionType == typeof(Version) => (Version.Parse(element.GetString()!)
                .CompareTo(Version.Parse(element.GetString()!)), null),
            _ => (0, new FilterException(FilterStatusCode.Invalid,
                $"unsupported filter basic type {typeof(T)} for compare", filter.GetKey()))
        };
    }

    public static (int, FilterException?) CompareTo<T>(this JsonElement element, IFilter<T> filter, T matchTarget)
    {
        return typeof(T) switch
        {
            { } stringType when stringType == typeof(string) => (element.GetString()!.CompareTo(matchTarget), null),
            { } doubleType when doubleType == typeof(double) => (element.GetDouble().CompareTo(matchTarget), null),
            { } longType when longType == typeof(long) => (element.GetInt64().CompareTo(matchTarget), null),
            { } versionType when versionType == typeof(Version) => (Version.Parse(element.GetString()!).CompareTo(matchTarget), null),
            _ => (0, new FilterException(FilterStatusCode.Invalid,
                $"unsupported filter basic type {typeof(T)} for compare", filter.GetKey()))
        };
    }

    public static FilterException? AssertIsValidFilterObjectArray<T>(this JsonElement element, IFilter<T> filter, JsonPropertyChecker jsonPropertyChecker)
    {
        if (element.ValueKind != JsonValueKind.Array)
        {
            return filter.CreateFilterValueTypeMismatchException(element, JsonValueKind.Array, false);
        }

        if (element.GetArrayLength() == 0)
        {
            return new FilterException(FilterStatusCode.Invalid, $"{element} 数组元素个数不能为0");
        }

        foreach (var e in element.EnumerateArray())
        {
            var checkResult = e.AssertIsValidObject(filter, jsonPropertyChecker);
            if (checkResult is not null)
            {
                return checkResult;
            }
        }

        return null;
    }

    private static FilterException? AssertIsValidNumber<T>(this JsonElement element, IFilter<T> filter)
    {
        if (element.ValueKind != JsonValueKind.Number)
        {
            return filter.CreateFilterValueTypeMismatchException(element, JsonValueKind.Number);
        }

        if (!element.TryGetDouble(out _))
        {
            return new FilterException(FilterStatusCode.Invalid,
                $"{filter.GetKey()} 的值 {element} tried get double value failed", filter.GetKey());
        }

        return null;
    }

    private static FilterException? AssertIsValidVersion<T>(this JsonElement element, IFilter<T> filter)
    {
        var checkResult = AssertIsValidString(element, filter);
        if (checkResult is not null)
        {
            return checkResult;
        }

        if (element.GetString() == null)
        {
            return new FilterException(FilterStatusCode.Invalid,
                $"{filter.GetKey()} 的值 {element} 为 null", filter.GetKey());
        }

        if (!Version.TryParse(element.GetString()!, out _))
        {
            return new FilterException(FilterStatusCode.Invalid,
                $"{filter.GetKey()} 的值 {element} tried parse Version failed", filter.GetKey());
        }

        return null;
    }

    private static FilterException? AssertIsValidString<T>(this JsonElement element, IFilter<T> filter)
    {
        if (element.ValueKind != JsonValueKind.String)
        {
            return filter.CreateFilterValueTypeMismatchException(element, JsonValueKind.String);
        }

        return null;
    }

    private static FilterException? AssertIsValidBool<T>(this JsonElement element, IFilter<T> filter)
    {
        if (element.ValueKind != JsonValueKind.True && element.ValueKind != JsonValueKind.False)
        {
            return filter.CreateFilterValueTypeMismatchException(element, [JsonValueKind.True, JsonValueKind.False]);
        }

        return null;
    }

    public static FilterException? AssertIsRightElementType<T>(this JsonElement element, IFilter<T> filter)
    {
        if (typeof(T) == typeof(bool))
        {
            return element.AssertIsValidBool(filter);
        }

        if (typeof(T) == typeof(double) || typeof(T) == typeof(long))
        {
            return element.AssertIsValidNumber(filter);
        }

        if (typeof(T) == typeof(string))
        {
            return element.AssertIsValidString(filter);
        }

        if (typeof(T) == typeof(Version))
        {
            return element.AssertIsValidVersion(filter);
        }

        return new FilterException(FilterStatusCode.Invalid, $"unsupported filter basic type {typeof(T)} for {filter}", filter.GetKey());
    }

    public static FilterException? AssertIsValidRegex<T>(this JsonElement element, IFilter<T> filter)
    {
        var checkResult = AssertIsValidString(element, filter);
        if (checkResult is not null)
        {
            return checkResult;
        }

        if (element.GetString() == null)
        {
            return new FilterException(FilterStatusCode.Invalid,
                $"{filter.GetKey()} 的值 {element} 为 null", filter.GetKey());
        }

        try
        {
            _ = new Regex(element.GetString()!);
        }
        catch (ArgumentException e)
        {
            return new FilterException(e, FilterStatusCode.Invalid, $"{filter.GetKey()} 的值 {element} 不是有效的正则表达式", filter.GetKey());
        }

        return null;
    }
}