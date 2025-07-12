using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Buffers;
using StructuredFilter.Filters.Common.FilterTypes;

namespace StructuredFilter.Filters.Common;

/// <summary>
/// Lightweight value type to replace JsonElement for memory optimization
/// Reduces memory allocations and GC pressure by storing values directly
/// instead of maintaining references to JSON documents.
/// Uses union-like structure to avoid boxing/unboxing of value types.
/// </summary>
public readonly record struct FilterValue
{
    private readonly FilterValueKind _kind;
    private readonly ValueUnion _union;
    private readonly string? _cachedString;

    [StructLayout(LayoutKind.Explicit)]
    private readonly struct ValueUnion
    {
        [FieldOffset(0)]
        public readonly double DoubleValue;
        [FieldOffset(0)]
        public readonly long LongValue;
        [FieldOffset(0)]
        public readonly bool BoolValue;
        [FieldOffset(8)]
        public readonly object? ObjectValue;

        public ValueUnion(double value)
        {
            Unsafe.SkipInit(out this);
            DoubleValue = value;
        }

        public ValueUnion(long value)
        {
            Unsafe.SkipInit(out this);
            LongValue = value;
        }

        public ValueUnion(bool value)
        {
            Unsafe.SkipInit(out this);
            BoolValue = value;
        }

        public ValueUnion(object? value)
        {
            Unsafe.SkipInit(out this);
            ObjectValue = value;
        }
    }

    public enum FilterValueKind
    {
        String,
        Double,
        Long,
        Version,
        Boolean,
        Null,
        Array,
        Object
    }

    public FilterValueKind ValueKind => _kind;

    private FilterValue(double value, FilterValueKind kind)
    {
        _kind = kind;
        _union = new ValueUnion(value);
        _cachedString = value.ToString();
    }

    private FilterValue(long value, FilterValueKind kind)
    {
        _kind = kind;
        _union = new ValueUnion(value);
        _cachedString = value.ToString();
    }

    private FilterValue(Version value, FilterValueKind kind)
    {
        _kind = kind;
        _union = new ValueUnion(value);
        _cachedString = value.ToString();
    }

    private FilterValue(bool value, FilterValueKind kind)
    {
        _kind = kind;
        _union = new ValueUnion(value);
        _cachedString = value ? "true" : "false";
    }

    private FilterValue(string? value, FilterValueKind kind)
    {
        _kind = kind;
        _union = new ValueUnion(value);
    }

    private FilterValue(object? value, FilterValueKind kind)
    {
        _kind = kind;
        _union = new ValueUnion(value);
        switch (kind)
        {
            case FilterValueKind.Null:
                _cachedString = "null";
                break;
            case FilterValueKind.Array:
                _cachedString = value != null ? $"[{string.Join(",", ((FilterValue[])value).Select(v => v.ToString()))}]" : "[]";
                break;
            case FilterValueKind.Object:
                _cachedString = value != null ? $"{{{string.Join(",", ((Dictionary<string, FilterValue>)value).Select(kv => $"\"{kv.Key}\":{kv.Value}"))}}}" : "{}";
                break;
            default:
                _cachedString = null;
                break;
        }
    }

    /// <summary>
    /// Creates FilterValue from JsonElement with explicit target type
    /// </summary>
    public static FilterValue FromJsonElement(JsonElement element, FilterValueKind targetKind)
    {
        return targetKind switch
        {
            FilterValueKind.String => element.ValueKind == JsonValueKind.String 
                ? new FilterValue(element.GetString(), FilterValueKind.String)
                : throw new FilterException(FilterStatusCode.Invalid, $"Cannot convert {element.ValueKind} to String"),
            FilterValueKind.Double => element.ValueKind == JsonValueKind.Number 
                ? new FilterValue(element.GetDouble(), FilterValueKind.Double)
                : element.ValueKind == JsonValueKind.String 
                    ? new FilterValue(double.Parse(element.GetString() ?? "0"), FilterValueKind.Double)
                    : throw new FilterException(FilterStatusCode.Invalid, $"Cannot convert {element.ValueKind} to Double"),
            FilterValueKind.Long => element.ValueKind == JsonValueKind.Number 
                ? new FilterValue(element.GetInt64(), FilterValueKind.Long)
                : element.ValueKind == JsonValueKind.String 
                    ? new FilterValue(long.Parse(element.GetString() ?? "0"), FilterValueKind.Long)
                    : throw new FilterException(FilterStatusCode.Invalid, $"Cannot convert {element.ValueKind} to Long"),
            FilterValueKind.Version => element.ValueKind == JsonValueKind.String 
                ? new FilterValue(Version.Parse(element.GetString() ?? "0.0.0"), FilterValueKind.Version)
                : throw new FilterException(FilterStatusCode.Invalid, $"Cannot convert {element.ValueKind} to Version"),
            FilterValueKind.Boolean => element.ValueKind switch
            {
                JsonValueKind.True => new FilterValue(true, FilterValueKind.Boolean),
                JsonValueKind.False => new FilterValue(false, FilterValueKind.Boolean),
                JsonValueKind.String => new FilterValue(bool.Parse(element.GetString() ?? "false"), FilterValueKind.Boolean),
                _ => throw new FilterException(FilterStatusCode.Invalid, $"Cannot convert {element.ValueKind} to Boolean")
            },
            FilterValueKind.Null => new FilterValue((object?)null, FilterValueKind.Null),
            _ => throw new FilterException(FilterStatusCode.Invalid, $"Unsupported FilterValueKind: {targetKind}")
        };
    }

    public static FilterValue FromString(string value) => new(value, FilterValueKind.String);
    public static FilterValue FromDouble(double value) => new(value, FilterValueKind.Double);
    public static FilterValue FromLong(long value) => new(value, FilterValueKind.Long);
    public static FilterValue FromVersion(Version value) => new(value, FilterValueKind.Version);
    public static FilterValue FromBoolean(bool value) => new(value, FilterValueKind.Boolean);
    public static FilterValue FromNull() => new((object?)null, FilterValueKind.Null);
    public static FilterValue FromArray(FilterValue[] values) => new(values, FilterValueKind.Array);
    public static FilterValue FromObject(Dictionary<string, FilterValue> values) => new(values, FilterValueKind.Object);

    public string? GetString() => _kind == FilterValueKind.String ? (string?)_union.ObjectValue : null;
    
    public double GetDouble() => _kind == FilterValueKind.Double ? _union.DoubleValue : throw new FilterException(FilterStatusCode.Invalid, "Value is not a double");
    
    public long GetInt64() => _kind == FilterValueKind.Long ? _union.LongValue : throw new FilterException(FilterStatusCode.Invalid, "Value is not a long");
    
    public Version GetVersion() => _kind == FilterValueKind.Version ? (Version)_union.ObjectValue! : throw new FilterException(FilterStatusCode.Invalid, "Value is not a version");
    
    public bool GetBoolean() => _kind == FilterValueKind.Boolean ? _union.BoolValue : throw new FilterException(FilterStatusCode.Invalid, "Value is not a boolean");
    
    public FilterValue[] GetArray() => _kind == FilterValueKind.Array ? (FilterValue[])_union.ObjectValue! : throw new FilterException(FilterStatusCode.Invalid, "Value is not an array");
    
    public Dictionary<string, FilterValue> GetObject() => _kind == FilterValueKind.Object ? (Dictionary<string, FilterValue>)_union.ObjectValue! : throw new FilterException(FilterStatusCode.Invalid, "Value is not an object");

    // Array indexer for compatibility
    public FilterValue this[int index] => GetArray()[index];

    // Object property accessor
    public FilterValue this[string key] => GetObject()[key];

    // Array length
    public int GetArrayLength() => GetArray().Length;

    // Object property count
    public int GetPropertyCount() => GetObject().Count;

    // Enumeration support - optimized with custom enumerators
    public ArrayEnumerator EnumerateArray() => 
        _kind == FilterValueKind.Array ? new ArrayEnumerator((FilterValue[])_union.ObjectValue!) : new ArrayEnumerator();

    public ObjectEnumerator EnumerateObject() => 
        _kind == FilterValueKind.Object ? new ObjectEnumerator((Dictionary<string, FilterValue>)_union.ObjectValue!) : new ObjectEnumerator();

    // High-performance value type enumerators
    public readonly struct ArrayEnumerator
    {
        private readonly FilterValue[]? _array;
        private readonly int _length;

        internal ArrayEnumerator(FilterValue[] array)
        {
            _array = array;
            _length = array.Length;
        }

        public int Count => _length;

        public Enumerator GetEnumerator() => new(_array, _length);

        public struct Enumerator
        {
            private readonly FilterValue[]? _array;
            private readonly int _length;
            private int _index;

            internal Enumerator(FilterValue[]? array, int length)
            {
                _array = array;
                _length = length;
                _index = -1;
            }

            public readonly FilterValue Current => _array![_index];

            public bool MoveNext()
            {
                _index++;
                return _index < _length;
            }
        }
    }

    public readonly struct ObjectEnumerator
    {
        private readonly Dictionary<string, FilterValue>? _dictionary;

        internal ObjectEnumerator(Dictionary<string, FilterValue> dictionary)
        {
            _dictionary = dictionary;
        }

        public Enumerator GetEnumerator() => new(_dictionary);

        public struct Enumerator
        {
            private Dictionary<string, FilterValue>.Enumerator _enumerator;

            internal Enumerator(Dictionary<string, FilterValue>? dictionary)
            {
                _enumerator = dictionary?.GetEnumerator() ?? default;
            }

            public readonly KeyValuePair<string, FilterValue> Current => _enumerator.Current;

            public bool MoveNext() => _enumerator.MoveNext();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool MatchEq<T>(IFilter<T> filter, T matchTarget)
    {
        if (typeof(T) == typeof(bool) && _kind == FilterValueKind.Boolean)
        {
            return _union.BoolValue == Unsafe.As<T, bool>(ref matchTarget);
        }

        if (typeof(T) == typeof(double) && _kind == FilterValueKind.Double)
        {
            return Math.Abs(_union.DoubleValue - Unsafe.As<T, double>(ref matchTarget)) < double.Epsilon;
        }

        if (typeof(T) == typeof(long) && _kind == FilterValueKind.Long)
        {
            return _union.LongValue == Unsafe.As<T, long>(ref matchTarget);
        }

        if (typeof(T) == typeof(long) && _kind == FilterValueKind.Double)
        {
            return (long)_union.DoubleValue == Unsafe.As<T, long>(ref matchTarget);
        }

        if (typeof(T) == typeof(double) && _kind == FilterValueKind.Long)
        {
            return Math.Abs(_union.LongValue - Unsafe.As<T, double>(ref matchTarget)) < double.Epsilon;
        }

        if (typeof(T) == typeof(string) && _kind == FilterValueKind.String)
        {
            return string.Equals((string?)_union.ObjectValue, Unsafe.As<T, string>(ref matchTarget), StringComparison.Ordinal);
        }

        if (typeof(T) == typeof(Version) && _kind == FilterValueKind.Version)
        {
            return ((Version)_union.ObjectValue!).Equals(matchTarget);
        }

        if (typeof(T) == typeof(Version) && _kind == FilterValueKind.String)
        {
            return Version.Parse((string)_union.ObjectValue!).Equals(matchTarget);
        }

        throw new FilterException(FilterStatusCode.Invalid, $"unsupported filter basic type {typeof(T)} for $eq", filter.GetKey());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool MatchNe<T>(IFilter<T> filter, T matchTarget)
    {
        return !MatchEq(filter, matchTarget);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool MatchGt<T>(IFilter<T> filter, T matchTarget)
    {
        if (typeof(T) == typeof(double) && _kind == FilterValueKind.Double)
        {
            return Unsafe.As<T, double>(ref matchTarget) > _union.DoubleValue;
        }

        if (typeof(T) == typeof(long) && _kind == FilterValueKind.Long)
        {
            return Unsafe.As<T, long>(ref matchTarget) > _union.LongValue;
        }

        if (typeof(T) == typeof(long) && _kind == FilterValueKind.Double)
        {
            return Unsafe.As<T, long>(ref matchTarget) > (long)_union.DoubleValue;
        }

        if (typeof(T) == typeof(double) && _kind == FilterValueKind.Long)
        {
            return Unsafe.As<T, double>(ref matchTarget) > _union.LongValue;
        }

        if (typeof(T) == typeof(Version) && _kind == FilterValueKind.Version)
        {
            return ((Version)_union.ObjectValue!).CompareTo(matchTarget) < 0;
        }

        if (typeof(T) == typeof(Version) && _kind == FilterValueKind.String)
        {
            return Version.Parse((string)_union.ObjectValue!).CompareTo(matchTarget) < 0;
        }

        throw new FilterException(FilterStatusCode.Invalid, $"unsupported filter basic type {typeof(T)} for $gt", filter.GetKey());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool MatchGe<T>(IFilter<T> filter, T matchTarget)
    {
        if (typeof(T) == typeof(double) && _kind == FilterValueKind.Double)
        {
            return Unsafe.As<T, double>(ref matchTarget) >= _union.DoubleValue;
        }

        if (typeof(T) == typeof(long) && _kind == FilterValueKind.Long)
        {
            return Unsafe.As<T, long>(ref matchTarget) >= _union.LongValue;
        }

        if (typeof(T) == typeof(long) && _kind == FilterValueKind.Double)
        {
            return Unsafe.As<T, long>(ref matchTarget) >= (long)_union.DoubleValue;
        }

        if (typeof(T) == typeof(double) && _kind == FilterValueKind.Long)
        {
            return Unsafe.As<T, double>(ref matchTarget) >= _union.LongValue;
        }

        if (typeof(T) == typeof(Version) && _kind == FilterValueKind.Version)
        {
            return ((Version)_union.ObjectValue!).CompareTo(matchTarget) <= 0;
        }

        if (typeof(T) == typeof(Version) && _kind == FilterValueKind.String)
        {
            return Version.Parse((string)_union.ObjectValue!).CompareTo(matchTarget) <= 0;
        }

        throw new FilterException(FilterStatusCode.Invalid, $"unsupported filter basic type {typeof(T)} for $ge", filter.GetKey());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool MatchLt<T>(IFilter<T> filter, T matchTarget)
    {
        if (typeof(T) == typeof(double) && _kind == FilterValueKind.Double)
        {
            return Unsafe.As<T, double>(ref matchTarget) < _union.DoubleValue;
        }

        if (typeof(T) == typeof(long) && _kind == FilterValueKind.Long)
        {
            return Unsafe.As<T, long>(ref matchTarget) < _union.LongValue;
        }

        if (typeof(T) == typeof(long) && _kind == FilterValueKind.Double)
        {
            return Unsafe.As<T, long>(ref matchTarget) < (long)_union.DoubleValue;
        }

        if (typeof(T) == typeof(double) && _kind == FilterValueKind.Long)
        {
            return Unsafe.As<T, double>(ref matchTarget) < _union.LongValue;
        }

        if (typeof(T) == typeof(Version) && _kind == FilterValueKind.Version)
        {
            return ((Version)_union.ObjectValue!).CompareTo(matchTarget) > 0;
        }

        if (typeof(T) == typeof(Version) && _kind == FilterValueKind.String)
        {
            return Version.Parse((string)_union.ObjectValue!).CompareTo(matchTarget) > 0;
        }

        throw new FilterException(FilterStatusCode.Invalid, $"unsupported filter basic type {typeof(T)} for $lt", filter.GetKey());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool MatchLe<T>(IFilter<T> filter, T matchTarget)
    {
        if (typeof(T) == typeof(double) && _kind == FilterValueKind.Double)
        {
            return Unsafe.As<T, double>(ref matchTarget) <= _union.DoubleValue;
        }

        if (typeof(T) == typeof(long) && _kind == FilterValueKind.Long)
        {
            return Unsafe.As<T, long>(ref matchTarget) <= _union.LongValue;
        }

        if (typeof(T) == typeof(long) && _kind == FilterValueKind.Double)
        {
            return Unsafe.As<T, long>(ref matchTarget) <= (long)_union.DoubleValue;
        }

        if (typeof(T) == typeof(double) && _kind == FilterValueKind.Long)
        {
            return Unsafe.As<T, double>(ref matchTarget) <= _union.LongValue;
        }

        if (typeof(T) == typeof(Version) && _kind == FilterValueKind.Version)
        {
            return ((Version)_union.ObjectValue!).CompareTo(matchTarget) >= 0;
        }

        if (typeof(T) == typeof(Version) && _kind == FilterValueKind.String)
        {
            return Version.Parse((string)_union.ObjectValue!).CompareTo(matchTarget) >= 0;
        }

        throw new FilterException(FilterStatusCode.Invalid, $"unsupported filter basic type {typeof(T)} for $le", filter.GetKey());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (int, FilterException?) CompareTo<T>(IFilter<T> filter, T matchTarget)
    {
        return typeof(T) switch
        {
            { } stringType when stringType == typeof(string) && _kind == FilterValueKind.String => 
                (string.Compare((string?)_union.ObjectValue, Unsafe.As<T, string>(ref matchTarget), StringComparison.Ordinal), null),
            { } doubleType when doubleType == typeof(double) && _kind == FilterValueKind.Double => 
                (_union.DoubleValue.CompareTo(Unsafe.As<T, double>(ref matchTarget)), null),
            { } longType when longType == typeof(long) && _kind == FilterValueKind.Long => 
                (_union.LongValue.CompareTo(Unsafe.As<T, long>(ref matchTarget)), null),
            { } longType when longType == typeof(long) && _kind == FilterValueKind.Double => 
                (((long)_union.DoubleValue).CompareTo(Unsafe.As<T, long>(ref matchTarget)), null),
            { } doubleType when doubleType == typeof(double) && _kind == FilterValueKind.Long => 
                (((double)_union.LongValue).CompareTo(Unsafe.As<T, double>(ref matchTarget)), null),
            { } versionType when versionType == typeof(Version) && _kind == FilterValueKind.Version => 
                (((Version)_union.ObjectValue!).CompareTo(matchTarget), null),
            { } versionType when versionType == typeof(Version) && _kind == FilterValueKind.String => 
                (Version.Parse((string)_union.ObjectValue!).CompareTo(matchTarget), null),
            _ => (0, new FilterException(FilterStatusCode.Invalid,
                $"unsupported filter basic type {typeof(T)} for compare", filter.GetKey()))
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool MatchIn<T>(IFilter<T> filter, T matchTarget)
    {
        if (_kind != FilterValueKind.Array)
        {
            throw new FilterException(FilterStatusCode.Invalid, "$in operation requires an array", filter.GetKey());
        }

        var array = (FilterValue[])_union.ObjectValue!;
        for (int i = 0; i < array.Length; i++)
        {
            if (array[i].MatchEq(filter, matchTarget))
            {
                return true;
            }
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override string ToString()
    {
        if (_cachedString != null)
        {
            return _cachedString;
        }

        return _kind switch
        {
            FilterValueKind.String => (string?)_union.ObjectValue ?? string.Empty,
            _ => _union.ObjectValue?.ToString() ?? "null"
        };
    }

    /// <summary>
    /// Converts FilterBasicType string to FilterValueKind enum
    /// </summary>
    public static FilterValueKind BasicTypeToValueKind(string basicType)
    {
        return basicType switch
        {
            FilterBasicType.String => FilterValueKind.String,
            FilterBasicType.Double => FilterValueKind.Double,
            FilterBasicType.Long => FilterValueKind.Long,
            FilterBasicType.Version => FilterValueKind.Version,
            FilterBasicType.Bool => FilterValueKind.Boolean,
            _ => throw new FilterException(FilterStatusCode.Invalid, $"Unknown FilterBasicType: {basicType}")
        };
    }
}
