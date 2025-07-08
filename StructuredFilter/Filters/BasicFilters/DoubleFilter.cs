using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using StructuredFilter.Filters.BasicFilters.Common;
using StructuredFilter.Filters.Common;

namespace StructuredFilter.Filters.BasicFilters;

public class DoubleFilterFactory : IBasicFilterFactory<double>
{
    private readonly FrozenDictionary<string, IDoubleFilter> _doubleFilters;

    public DoubleFilterFactory(IEnumerable<IDoubleFilter> doubleFilters)
    {
        _doubleFilters = doubleFilters
            .ToDictionary(doubleFilter => doubleFilter.GetKey())
            .ToFrozenDictionary();
    }

    public (IBasicFilter<double>, FilterException?) Get(string key)
    {
        if (_doubleFilters.TryGetValue(key, out var doubleFilter))
        {
            return (doubleFilter, null);
        }

        return (null!, this.CreateSubFilterNotFoundException(key));
    }

    public Dictionary<string, IFilter<double>> GetAll()
    {
        return _doubleFilters.ToDictionary(k => k.Key, IFilter<double> (v) => v.Value);
    }
}

internal class DoubleInFilter : InFilter<double>, IDoubleFilter;

internal class DoubleEqFilter : EqFilter<double>, IDoubleFilter;

internal class DoubleNeFilter : NeFilter<double>, IDoubleFilter;

internal class DoubleGreaterThanFilter : GtFilter<double>, IDoubleFilter;

internal class DoubleGreaterOrEqualFilter : GeFilter<double>, IDoubleFilter;

internal class DoubleLessThanFilter : LtFilter<double>, IDoubleFilter;

internal class DoubleLessOrEqualFilter : LeFilter<double>, IDoubleFilter;

internal class DoubleRangeFilter : RangeFilter<double>, IDoubleFilter;
