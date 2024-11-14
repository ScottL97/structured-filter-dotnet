using System;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace StructuredFilter.Filters.Common.FilterTypes;

public static class FilterTypeChecker
{
    public delegate void JsonPropertyChecker(JsonProperty jsonProperty);
    public static void AssertIsValidObject<T>(this JsonElement element, IFilter<T> filter, JsonPropertyChecker jsonPropertyChecker)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            filter.ThrowFilterValueTypeMismatchException(element, JsonValueKind.Object);
            return;
        }

        var kvCount = element.EnumerateObject().Count();
        if (kvCount != 1)
        {
            throw new FilterException(FilterStatusCode.Invalid, $"对象键值对数需要为 1，{element} 有 {kvCount} 对键值对");
        }

        foreach (var property in element.EnumerateObject())
        {
            jsonPropertyChecker(property);
        }
    }

    public static void AssertIsValidArray<T>(this JsonElement element, IFilter<T> filter, int? assertKvCount = null)
    {
        if (element.ValueKind != JsonValueKind.Array)
        {
            filter.ThrowWrongFilterValueTypeException(element, JsonValueKind.Array);
        }

        var kvCount = element.EnumerateArray().Count();
        if (kvCount == 0)
        {
            throw new FilterException(FilterStatusCode.Invalid, $"array elements count should be more than 0, {typeof(IFilter<T>)} value {element.ToString()} has 0", filter.GetKey());
        }

        if (assertKvCount != null)
        {
            if (assertKvCount.Value != kvCount)
            {
                throw new FilterException(FilterStatusCode.Invalid, $"array elements count should be {assertKvCount.Value}, {typeof(IFilter<T>)} value {element.ToString()} has {kvCount}", filter.GetKey());
            }
        }

        foreach (var e in element.EnumerateArray())
        {
            e.AssertIsRightElementType(filter);
        }
    }

    public static void AssertIsValidRange<T>(this JsonElement element, IFilter<T> filter)
    {
        element.AssertIsValidArray(filter, 2);

        if (element[1].CompareTo(filter, element[0]) < 0)
        {
            throw new FilterException(FilterStatusCode.Invalid,
                $"the second element of the range {element[1]} is not >= the first element {element[0]}", filter.GetKey());
        }
    }

    private static int CompareTo<T>(this JsonElement element, IFilter<T> filter, JsonElement other)
    {
        return typeof(T) switch
        {
            { } stringType when stringType == typeof(string) => string.CompareOrdinal(element.GetString(),
                other.ToString()),
            { } doubleType when doubleType == typeof(double) => element.GetDouble().CompareTo(other.GetDouble()),
            { } versionType when versionType == typeof(Version) => Version.Parse(element.GetString()!)
                .CompareTo(Version.Parse(element.GetString()!)),
            _ => throw new FilterException(FilterStatusCode.Invalid,
                $"unsupported filter basic type {typeof(T)} for compare", filter.GetKey())
        };
    }

    public static int CompareTo<T>(this JsonElement element, IFilter<T> filter, T matchTarget)
    {
        return typeof(T) switch
        {
            { } stringType when stringType == typeof(string) => element.GetString()!.CompareTo(matchTarget),
            { } doubleType when doubleType == typeof(double) => element.GetDouble().CompareTo(matchTarget),
            { } versionType when versionType == typeof(Version) => Version.Parse(element.GetString()!).CompareTo(matchTarget),
            _ => throw new FilterException(FilterStatusCode.Invalid,
                $"unsupported filter basic type {typeof(T)} for compare", filter.GetKey())
        };
    }

    public static void AssertIsValidFilterObjectArray<T>(this JsonElement element, IFilter<T> filter, JsonPropertyChecker jsonPropertyChecker)
    {
        if (element.ValueKind != JsonValueKind.Array)
        {
            filter.ThrowFilterValueTypeMismatchException(element, JsonValueKind.Array, false);
            return;
        }

        var kvCount = element.EnumerateArray().Count();
        if (kvCount == 0)
        {
            throw new FilterException(FilterStatusCode.Invalid, $"{element} 数组元素个数不能为0");
        }

        foreach (var e in element.EnumerateArray())
        {
            e.AssertIsValidObject(filter, jsonPropertyChecker);
        }
    }

    private static void AssertIsValidNumber<T>(this JsonElement element, IFilter<T> filter)
    {
        if (element.ValueKind != JsonValueKind.Number)
        {
            filter.ThrowFilterValueTypeMismatchException(element, JsonValueKind.Number);
            return;
        }

        if (!element.TryGetDouble(out _))
        {
            throw new FilterException(FilterStatusCode.Invalid,
                $"{filter.GetKey()} 的值 {element} tried get double value failed", filter.GetKey());
        }
    }

    private static void AssertIsValidVersion<T>(this JsonElement element, IFilter<T> filter)
    {
        AssertIsValidString(element, filter);

        if (element.GetString() == null)
        {
            throw new FilterException(FilterStatusCode.Invalid,
                $"{filter.GetKey()} 的值 {element} 为 null", filter.GetKey());
        }

        if (!Version.TryParse(element.GetString()!, out _))
        {
            throw new FilterException(FilterStatusCode.Invalid,
                $"{filter.GetKey()} 的值 {element} tried parse Version failed", filter.GetKey());
        }
    }

    private static void AssertIsValidString<T>(this JsonElement element, IFilter<T> filter)
    {
        if (element.ValueKind != JsonValueKind.String)
        {
            filter.ThrowFilterValueTypeMismatchException(element, JsonValueKind.String);
        }
    }

    private static void AssertIsValidBool<T>(this JsonElement element, IFilter<T> filter)
    {
        if (element.ValueKind != JsonValueKind.True && element.ValueKind != JsonValueKind.False)
        {
            filter.ThrowFilterValueTypeMismatchException(element, [JsonValueKind.True, JsonValueKind.False]);
        }
    }

    public static void AssertIsRightElementType<T>(this JsonElement element, IFilter<T> filter)
    {
        if (typeof(T) == typeof(bool))
        {
            element.AssertIsValidBool(filter);
        }
        else if (typeof(T) == typeof(double))
        {
            element.AssertIsValidNumber(filter);
        }
        else if (typeof(T) == typeof(string))
        {
            element.AssertIsValidString(filter);
        }
        else if (typeof(T) == typeof(Version))
        {
            element.AssertIsValidVersion(filter);
        }
        else
        {
            throw new FilterException(FilterStatusCode.Invalid, $"unsupported filter basic type {typeof(T)} for {filter}", filter.GetKey());
        }
    }

    public static void AssertIsValidRegex<T>(this JsonElement element, IFilter<T> filter)
    {
        AssertIsValidString(element, filter);

        if (element.GetString() == null)
        {
            throw new FilterException(FilterStatusCode.Invalid,
                $"{filter.GetKey()} 的值 {element} 为 null", filter.GetKey());
        }

        try
        {
            _ = new Regex(element.GetString()!);
        }
        catch (ArgumentException e)
        {
            throw new FilterException(e, FilterStatusCode.Invalid, $"{filter.GetKey()} 的值 {element} 不是有效的正则表达式", filter.GetKey());
        }
    }
}