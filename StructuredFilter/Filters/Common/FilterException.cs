using System;
using System.Collections.Generic;
using System.Text.Json;
using StructuredFilter.Utils;

namespace StructuredFilter.Filters.Common;

public enum FilterStatusCode
{
    Ok, // 匹配成功
    MatchError, // 匹配代码异常
    NotMatched, // 不匹配
    Invalid // 不合法
}

public class FilterException: Exception
{
    public Exception? InnerException { get; set; }
    public FilterStatusCode StatusCode { get; set; }
    public string Message { get; set; }

    public Tree<string> FailedKeyPath { get; set; }

    public FilterException(Exception? innerException, FilterStatusCode statusCode, string message, string? failedKey=null)
    {
        InnerException = innerException;
        StatusCode = statusCode;
        Message = message;
        FailedKeyPath = new Tree<string>(failedKey);
    }

    public FilterException(FilterStatusCode statusCode, string message, string? failedKey=null)
    {
        StatusCode = statusCode;
        Message = message;
        FailedKeyPath = new Tree<string>(failedKey);
    }

    public FilterException PrependFailedKey(string failedKey)
    {
        FailedKeyPath.AddLevelAboveRoot(failedKey);
        return this;
    }

    public FilterException AppendFailedKeys(IEnumerable<Tree<string>> failedKeys)
    {
        foreach (var failedKeyTree in failedKeys)
        {
            if (failedKeyTree.Root is not null)
            {
                FailedKeyPath.Root.AddChild(failedKeyTree.Root);
            }
        }

        return this;
    }
}

public static class FilterExceptionExtensions
{
    public static void ThrowMatchTargetGetFailedException<T>(this IFilter<T> filter, Dictionary<string, object>? args)
    {
        throw new FilterException(FilterStatusCode.MatchError, $"matchTarget of type {typeof(T)} get failed, args: {JsonSerializer.Serialize(args)}", filter.GetKey());
    }

    public static void ThrowNotMatchException<T>(this IFilter<T> filter, T matchTarget, string filterValue)
    {
        throw new FilterException(FilterStatusCode.NotMatched, $"matchTarget {matchTarget} of type {typeof(T)} not match {{{filter.GetKey()}: {filterValue}}}", filter.GetKey());
    }
    
    public static void ThrowCacheNotMatchException<T>(this IFilter<T> filter, T matchTarget, string filterValue)
    {
        throw new FilterException(FilterStatusCode.NotMatched, $"matchTarget {matchTarget} of type {typeof(T)} not match {{{filter.GetKey()}: {filterValue}}} according to cache", filter.GetKey());
    }

    public static void ThrowWrongFilterValueTypeException<T>(this IFilter<T> filter, JsonElement element, JsonValueKind expectedType)
    {
        throw new FilterException(FilterStatusCode.Invalid,
            $"{typeof(IFilter<T>)} value type is {element.ValueKind}, not expected {expectedType}", filter.GetKey());
    }

    public static void ThrowSubFilterNotFoundException<T>(this IFilterFactory<T> filterFactory, string filterKey)
    {
        throw new FilterException(FilterStatusCode.Invalid, $"FilterFactory of type {typeof(T)} 包含无效子 filter {filterKey}", filterKey);
    }

    public static void ThrowFilterValueTypeMismatchException<T>(this IFilter<T> filter, JsonElement element, JsonValueKind expectedType, bool prependFailedKey=true)
    {
        var e = new FilterException(FilterStatusCode.Invalid, $"filter 值 {element} 类型为 {element.ValueKind}，期望类型为 {expectedType.ToString()}");
        if (prependFailedKey)
        {
            throw e.PrependFailedKey(filter.GetKey());
        }

        throw e;
    }

    public static void ThrowFilterValueTypeMismatchException<T>(this IFilter<T> filter, JsonElement element, JsonValueKind[] expectedTypes)
    {
        throw new FilterException(FilterStatusCode.Invalid, $"filter 值 {element} 类型为 {element.ValueKind}，期望类型为 {string.Join('/', expectedTypes)}", filter.GetKey());
    }
}
