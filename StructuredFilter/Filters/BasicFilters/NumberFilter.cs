using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using StructuredFilter.Filters.BasicFilters.Common;
using StructuredFilter.Filters.Common;

namespace StructuredFilter.Filters.BasicFilters;

public class NumberFilterFactory : IBasicFilterFactory<double>
{
    private readonly FrozenDictionary<string, INumberFilter> _numberFilters;

    public NumberFilterFactory(IEnumerable<INumberFilter> numberFilters)
    {
        _numberFilters = numberFilters
            .ToDictionary(numberFilter => numberFilter.GetKey())
            .ToFrozenDictionary();
    }

    public IBasicFilter<double> Get(string key)
    {
        if (_numberFilters.TryGetValue(key, out var numberFilter))
        {
            return numberFilter;
        }

        this.ThrowSubFilterNotFoundException(key);
        return null;
    }

    public Dictionary<string, IFilter<double>> GetAll()
    {
        return _numberFilters.ToDictionary(k => k.Key, IFilter<double> (v) => v.Value);
    }
}

internal class NumberInFilter : InFilter<double>, INumberFilter;

internal class NumberEqFilter : EqFilter<double>, INumberFilter;

internal class NumberNeFilter : NeFilter<double>, INumberFilter;

internal class GreaterThanFilter : GtFilter<double>, INumberFilter;

internal class GreaterOrEqualFilter : GeFilter<double>, INumberFilter;

internal class LessThanFilter : LtFilter<double>, INumberFilter;

internal class LessOrEqualFilter : LeFilter<double>, INumberFilter;

internal class NumberRangeFilter : RangeFilter<double>, INumberFilter;
