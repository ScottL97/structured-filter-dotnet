using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using StructuredFilter.Filters.BasicFilters.Common;
using StructuredFilter.Filters.Common;

namespace StructuredFilter.Filters.BasicFilters;

public class LongFilterFactory : IBasicFilterFactory<long>
{
    private readonly FrozenDictionary<string, ILongFilter> _longFilters;

    public LongFilterFactory(IEnumerable<ILongFilter> longFilters)
    {
        _longFilters = longFilters
            .ToDictionary(longFilter => longFilter.GetKey())
            .ToFrozenDictionary();
    }

    public (IBasicFilter<long>, FilterException?) Get(string key)
    {
        if (_longFilters.TryGetValue(key, out var longFilter))
        {
            return (longFilter, null);
        }

        return (null!, this.CreateSubFilterNotFoundException(key));
    }

    public Dictionary<string, IFilter<long>> GetAll()
    {
        return _longFilters.ToDictionary(k => k.Key, IFilter<long> (v) => v.Value);
    }
}

internal class LongInFilter : InFilter<long>, ILongFilter;

internal class LongEqFilter : EqFilter<long>, ILongFilter;

internal class LongNeFilter : NeFilter<long>, ILongFilter;

internal class LongGreaterThanFilter : GtFilter<long>, ILongFilter;

internal class LongGreaterOrEqualFilter : GeFilter<long>, ILongFilter;

internal class LongLessThanFilter : LtFilter<long>, ILongFilter;

internal class LongLessOrEqualFilter : LeFilter<long>, ILongFilter;

internal class LongRangeFilter : RangeFilter<long>, ILongFilter;
