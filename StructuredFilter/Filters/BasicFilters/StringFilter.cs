using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using StructuredFilter.Filters.BasicFilters.Common;
using StructuredFilter.Filters.Common;
using StructuredFilter.Filters.Common.FilterTypes;
using StructuredFilter.Utils;

namespace StructuredFilter.Filters.BasicFilters;

public class StringFilterFactory : IBasicFilterFactory<string>
{
    private readonly FrozenDictionary<string, IStringFilter> _stringFilters;

    public StringFilterFactory(IEnumerable<IStringFilter> stringFilters)
    {
        _stringFilters = stringFilters
            .ToDictionary(stringFilter => stringFilter.GetKey())
            .ToFrozenDictionary();
    }

    public (IBasicFilter<string>, FilterException?) Get(string key)
    {
        if (_stringFilters.TryGetValue(key, out var stringFilter))
        {
            return (stringFilter, null);
        }

        return (null, this.CreateSubFilterNotFoundException(key));
    }

    public Dictionary<string, IFilter<string>> GetAll()
    {
        return _stringFilters.ToDictionary(k => k.Key, IFilter<string> (v) => v.Value);
    }
}

internal class StringInFilter : InFilter<string>, IStringFilter;

internal class StringEqFilter : EqFilter<string>, IStringFilter;

internal class StringNeFilter : NeFilter<string>, IStringFilter;

internal class StringRangeFilter : RangeFilter<string>, IStringFilter;

[FilterLabel("匹配正则表达式")]
[FilterKey("$regex")]
internal class StringRegexFilter : Filter<string>, IStringFilter
{
    public FilterException? Valid(JsonElement element)
    {
        return element.AssertIsValidRegex(this);
    }

    public async Task<FilterException?> LazyMatchAsync(JsonElement element, LazyObjectGetter<string> matchTargetGetter)
    {
        try
        {
            var matchTarget = await matchTargetGetter.GetAsync();
            if (Regex.IsMatch(matchTarget, element.GetString()!, RegexOptions.None))
            {
                return null;
            }

            return this.CreateNotMatchException(matchTarget, element.ToString());
        }
        catch (LazyObjectGetException)
        {
            return this.CreateMatchTargetGetFailedException(matchTargetGetter.Args);
        }
    }

    public FilterException? Match(JsonElement element, string matchTarget)
    {
        if (Regex.IsMatch(matchTarget, element.GetString()!, RegexOptions.None))
        {
            return null;
        }

        return this.CreateNotMatchException(matchTarget, element.ToString());
    }
}
