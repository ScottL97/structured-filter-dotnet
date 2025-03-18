using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using StructuredFilter.Filters.BasicFilters.Common;
using StructuredFilter.Filters.Common;

namespace StructuredFilter.Filters.BasicFilters;

public class VersionFilterFactory : IBasicFilterFactory<Version>
{
    private readonly FrozenDictionary<string, IVersionFilter> _versionFilters;

    public VersionFilterFactory(IEnumerable<IVersionFilter> versionFilters)
    {
        _versionFilters = versionFilters
            .ToDictionary(versionFilter => versionFilter.GetKey())
            .ToFrozenDictionary();
    }

    public IBasicFilter<Version> Get(string key)
    {
        if (_versionFilters.TryGetValue(key, out var versionFilter))
        {
            return versionFilter;
        }

        this.ThrowSubFilterNotFoundException(key);
        return null;
    }

    public Dictionary<string, IFilter<Version>> GetAll()
    {
        return _versionFilters.ToDictionary(k => k.Key, IFilter<Version> (v) => v.Value);
    }
}

internal class VersionInFilter : InFilter<Version>, IVersionFilter;

internal class VersionEqFilter : EqFilter<Version>, IVersionFilter;

internal class VersionNeFilter : NeFilter<Version>, IVersionFilter;

internal class VersionGreaterOrEqualFilter : GeFilter<Version>, IVersionFilter;

internal class VersionLessOrEqualFilter : LeFilter<Version>, IVersionFilter;

internal class VersionGreaterThanFilter : GtFilter<Version>, IVersionFilter;

internal class VersionLessThanFilter : LtFilter<Version>, IVersionFilter;

internal class VersionRangeFilter : RangeFilter<Version>, IVersionFilter;
