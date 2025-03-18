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
    Invalid, // 不合法
    OptionError, // Filter 配置错误
}

public class FilterException: Exception
{
    public static readonly FilterException Ok = new (FilterStatusCode.Ok, string.Empty);
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
    public static FilterException CreateMatchTargetGetFailedException<T>(this IFilter<T> filter, Dictionary<string, object>? args)
    {
        return new FilterException(FilterStatusCode.MatchError, $"matchTarget of type {typeof(T)} get failed, args: {JsonSerializer.Serialize(args)}", filter.GetKey());
    }

    public static FilterException CreateNotMatchException<T>(this IBasicFilter<T> filter, T matchTarget, string filterValue)
    {
        return new FilterException(FilterStatusCode.NotMatched, $"matchTarget {matchTarget} of type {typeof(T)} not match {{{filter.GetKey()}: {filterValue}}}", filter.GetKey());
    }

    public static FilterException CreateCacheNotMatchException<T>(this IFilter<T> filter, T matchTarget, string filterValue)
    {
        return new FilterException(FilterStatusCode.NotMatched, $"matchTarget {matchTarget} of type {typeof(T)} not match {{{filter.GetKey()}: {filterValue}}} according to cache", filter.GetKey());
    }

    public static FilterException CreateWrongFilterValueTypeException<T>(this IFilter<T> filter, JsonElement element, JsonValueKind expectedType)
    {
        return new FilterException(FilterStatusCode.Invalid,
            $"{typeof(IFilter<T>)} value type is {element.ValueKind}, not expected {expectedType}", filter.GetKey());
    }

    public static FilterException CreateSubFilterNotFoundException<T>(this IFilterFactory<T> filterFactory, string filterKey)
    {
        return new FilterException(FilterStatusCode.Invalid, $"FilterFactory of type {typeof(T)} 子 filter {filterKey} 不存在", filterKey);
    }

    public static FilterException CreateFilterValueTypeMismatchException<T>(this IFilter<T> filter, JsonElement element, JsonValueKind expectedType, bool prependFailedKey=true)
    {
        var e = new FilterException(FilterStatusCode.Invalid, $"filter 值 {element} 类型为 {element.ValueKind}，期望类型为 {expectedType.ToString()}");
        if (prependFailedKey)
        {
            return e.PrependFailedKey(filter.GetKey());
        }

        return e;
    }

    public static FilterException CreateFilterValueTypeMismatchException<T>(this IFilter<T> filter, JsonElement element, JsonValueKind[] expectedTypes)
    {
        return new FilterException(FilterStatusCode.Invalid, $"filter 值 {element} 类型为 {element.ValueKind}，期望类型为 {string.Join('/', expectedTypes)}", filter.GetKey());
    }
}
