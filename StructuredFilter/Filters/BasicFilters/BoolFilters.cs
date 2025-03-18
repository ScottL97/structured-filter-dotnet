using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using StructuredFilter.Filters.BasicFilters.Common;
using StructuredFilter.Filters.Common;

namespace StructuredFilter.Filters.BasicFilters;

public class BoolFilterFactory : IBasicFilterFactory<bool>
{
    private readonly FrozenDictionary<string, IBoolFilter> _boolFilters;

    public BoolFilterFactory(IEnumerable<IBoolFilter> boolFilters)
    {
        _boolFilters = boolFilters
            .ToDictionary(boolFilter => boolFilter.GetKey())
            .ToFrozenDictionary();
    }

    public (IBasicFilter<bool>, FilterException?) Get(string key)
    {
        if (_boolFilters.TryGetValue(key, out var boolFilter))
        {
            return (boolFilter, null);
        }

        return (null, this.CreateSubFilterNotFoundException(key));
    }

    public Dictionary<string, IFilter<bool>> GetAll()
    {
        return _boolFilters.ToDictionary(kv => kv.Key, IFilter<bool> (kv) => kv.Value);
    }
}

internal class BoolEqFilter : EqFilter<bool>, IBoolFilter;

internal class BoolNeFilter : NeFilter<bool>, IBoolFilter;
