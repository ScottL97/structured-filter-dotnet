using System.Linq;
using System.Text.Json;
using BenchmarkDotNet.Attributes;
using StructuredFilter.Filters.Common;
using StructuredFilter.Filters.Common.FilterTypes;

namespace Benchmarks;

[MemoryDiagnoser]
[SimpleJob]
public class FilterValueVsJsonElementBenchmark
{
    private readonly JsonElement _jsonElementDouble;
    private readonly JsonElement _jsonElementLong;
    private readonly JsonElement _jsonElementString;
    private readonly JsonElement _jsonElementVersionArray;
    private readonly JsonElement _jsonElementDoubleArray;

    private readonly FilterValue _filterValueDouble;
    private readonly FilterValue _filterValueLong;
    private readonly FilterValue _filterValueString;
    private readonly FilterValue _filterValueVersionArray;
    private readonly FilterValue _filterValueDoubleArray;

    private readonly double _doubleTarget = 42.5;
    private readonly string _stringTarget = "test";

    private readonly TestFilter _testFilter = new TestFilter();

    public FilterValueVsJsonElementBenchmark()
    {
        // Setup JsonElement values
        var jsonDoc1 = JsonDocument.Parse("42.5");
        _jsonElementDouble = jsonDoc1.RootElement;

        var jsonDoc2 = JsonDocument.Parse("1000");
        _jsonElementLong = jsonDoc2.RootElement;

        var jsonDoc3 = JsonDocument.Parse("\"test\"");
        _jsonElementString = jsonDoc3.RootElement;

        var jsonDoc4 = JsonDocument.Parse("[\"1.0.0\", \"1.0.1\", \"1.0.2\"]");
        _jsonElementVersionArray = jsonDoc4.RootElement;

        var jsonDoc6 = JsonDocument.Parse("[40.0, 41.0, 42.5, 43.0, 44.0]");
        _jsonElementDoubleArray = jsonDoc6.RootElement;

        // Setup FilterValue instances
        _filterValueDouble = FilterValue.FromDouble(42.5);
        _filterValueLong = FilterValue.FromLong(1000);
        _filterValueString = FilterValue.FromString("test");
        _filterValueVersionArray = FilterValue.FromArray(_jsonElementVersionArray.EnumerateArray()
            .Select(element => FilterValue.FromJsonElement(element, FilterValue.FilterValueKind.Version))
            .ToArray());
        _filterValueDoubleArray = FilterValue.FromArray(_jsonElementDoubleArray.EnumerateArray()
            .Select(element => FilterValue.FromJsonElement(element, FilterValue.FilterValueKind.Double))
            .ToArray());
    }

    [Benchmark]
    public bool JsonElement_MatchIn_Double()
    {
        return _jsonElementDoubleArray.MatchIn(_testFilter, _doubleTarget);
    }

    [Benchmark]
    public bool FilterValue_MatchIn_Double()
    {
        return _filterValueDoubleArray.MatchIn(_testFilter, _doubleTarget);
    }

    [Benchmark]
    public bool JsonElement_MatchEq_String()
    {
        return _jsonElementString.MatchEq(_testFilter, _stringTarget);
    }

    [Benchmark]
    public bool FilterValue_MatchEq_String()
    {
        return _filterValueString.MatchEq(_testFilter, _stringTarget);
    }

    [Benchmark]
    public long JsonElement_GetInt64()
    {
        return _jsonElementLong.GetInt64();
    }

    [Benchmark]
    public long FilterValue_GetInt64()
    {
        return _filterValueLong.GetInt64();
    }

    // Array enumeration benchmarks
    [Benchmark]
    public int JsonElement_EnumerateArray()
    {
        int count = 0;
        foreach (var element in _jsonElementVersionArray.EnumerateArray())
        {
            count++;
        }
        return count;
    }

    [Benchmark]
    public int FilterValue_EnumerateArray()
    {
        int count = 0;
        foreach (var element in _filterValueVersionArray.EnumerateArray())
        {
            count++;
        }
        return count;
    }

    // ToString benchmarks
    [Benchmark]
    public string JsonElement_ToString_Double()
    {
        return _jsonElementDouble.ToString();
    }

    [Benchmark]
    public string FilterValue_ToString_Double()
    {
        return _filterValueDouble.ToString();
    }

    [Benchmark]
    public string JsonElement_ToString_String()
    {
        return _jsonElementString.ToString();
    }

    [Benchmark]
    public string FilterValue_ToString_String()
    {
        return _filterValueString.ToString();
    }

    [Benchmark]
    public string JsonElement_ToString_Array()
    {
        return _jsonElementVersionArray.ToString();
    }

    [Benchmark]
    public string FilterValue_ToString_Array()
    {
        return _filterValueVersionArray.ToString();
    }
}

// Simple test filter for benchmarking
public class TestFilter : IFilter<double>, IFilter<long>, IFilter<string>, IFilter<bool>
{
    string IFilter<double>.GetKey() => "$test";
    string IFilter<double>.GetLabel() => "Test Filter";
    string IFilter<double>.GetBasicType() => "DOUBLE";
    FilterException? IFilter<double>.Valid(JsonElement element) => null;

    string IFilter<long>.GetKey() => "$test";
    string IFilter<long>.GetLabel() => "Test Filter";
    string IFilter<long>.GetBasicType() => "LONG";
    FilterException? IFilter<long>.Valid(JsonElement element) => null;

    string IFilter<string>.GetKey() => "$test";
    string IFilter<string>.GetLabel() => "Test Filter";
    string IFilter<string>.GetBasicType() => "STRING";
    FilterException? IFilter<string>.Valid(JsonElement element) => null;

    string IFilter<bool>.GetKey() => "$test";
    string IFilter<bool>.GetLabel() => "Test Filter";
    string IFilter<bool>.GetBasicType() => "BOOL";
    FilterException? IFilter<bool>.Valid(JsonElement element) => null;
}
