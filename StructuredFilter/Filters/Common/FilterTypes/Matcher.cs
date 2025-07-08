using System;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace StructuredFilter.Filters.Common.FilterTypes;

public static class Matcher
{
    public static bool MatchEq<T>(this JsonElement element, IFilter<T> filter, T marchTarget)
    {
        if (typeof(T) == typeof(bool))
        {
            return element.GetBoolean() == Unsafe.As<T, bool>(ref marchTarget);
        }

        if (typeof(T) == typeof(double))
        {
            return element.GetDouble() - Unsafe.As<T, double>(ref marchTarget) == 0;
        }

        if (typeof(T) == typeof(string))
        {
            return element.ValueEquals(Unsafe.As<T, string>(ref marchTarget));
        }

        if (typeof(T) == typeof(Version))
        {
            return Version.Parse(element.GetString()!).Equals(marchTarget);
        }

        throw new FilterException(FilterStatusCode.Invalid, $"unsupported filter basic type {typeof(T)} for $eq", filter.GetKey());
    }
    
    public static bool MatchGe<T>(this JsonElement element, IFilter<T> filter, T marchTarget)
    {
        if (typeof(T) == typeof(double))
        {
            return Unsafe.As<T, double>(ref marchTarget) >= element.GetDouble();
        }

        if (typeof(T) == typeof(Version))
        {
            return Version.Parse(element.GetString()!).CompareTo(marchTarget) <= 0;
        }

        throw new FilterException(FilterStatusCode.Invalid, $"unsupported filter basic type {typeof(T)} for $ge", filter.GetKey());
    }

    public static bool MatchGt<T>(this JsonElement element, IFilter<T> filter, T marchTarget)
    {
        if (typeof(T) == typeof(double))
        {
            return Unsafe.As<T, double>(ref marchTarget) > element.GetDouble();
        }

        if (typeof(T) == typeof(Version))
        {
            return Version.Parse(element.GetString()!).CompareTo(marchTarget) < 0;
        }

        throw new FilterException(FilterStatusCode.Invalid, $"unsupported filter basic type {typeof(T)} for $gt", filter.GetKey());
    }

    public static bool MatchLe<T>(this JsonElement element, IFilter<T> filter, T marchTarget)
    {
        if (typeof(T) == typeof(double))
        {
            return Unsafe.As<T, double>(ref marchTarget) <= element.GetDouble();
        }

        if (typeof(T) == typeof(Version))
        {
            return Version.Parse(element.GetString()!).CompareTo(marchTarget) >= 0;
        }

        throw new FilterException(FilterStatusCode.Invalid, $"unsupported filter basic type {typeof(T)} for $le", filter.GetKey());
    }

    public static bool MatchLt<T>(this JsonElement element, IFilter<T> filter, T marchTarget)
    {
        if (typeof(T) == typeof(double))
        {
            return Unsafe.As<T, double>(ref marchTarget) < element.GetDouble();
        }

        if (typeof(T) == typeof(Version))
        {
            return Version.Parse(element.GetString()!).CompareTo(marchTarget) > 0;
        }

        throw new FilterException(FilterStatusCode.Invalid, $"unsupported filter basic type {typeof(T)} for $lt", filter.GetKey());
    }

    public static bool MatchNe<T>(this JsonElement element, IFilter<T> filter, T marchTarget)
    {
        if (typeof(T) == typeof(bool))
        {
            return element.GetBoolean() != Unsafe.As<T, bool>(ref marchTarget);
        }

        if (typeof(T) == typeof(double))
        {
            return element.GetDouble() - Unsafe.As<T, double>(ref marchTarget) != 0;
        }

        if (typeof(T) == typeof(string))
        {
            return !element.ValueEquals(Unsafe.As<T, string>(ref marchTarget));
        }

        if (typeof(T) == typeof(Version))
        {
            return !Version.Parse(element.GetString()!).Equals(marchTarget);
        }

        throw new FilterException(FilterStatusCode.Invalid, $"unsupported filter basic type {typeof(T)}", filter.GetKey());
    }
}