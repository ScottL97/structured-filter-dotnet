using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using StructuredFilter.Filters.BasicFilters.Common;
using StructuredFilter.Filters.Common;
using StructuredFilter.Filters.Common.FilterTypes;

namespace StructuredFilter.Filters.BasicFilters;

public class StringFilterFactory : IFilterFactory<string>
{
    private readonly FrozenDictionary<string, IStringFilter> _stringFilters;

    public StringFilterFactory(IEnumerable<IStringFilter> stringFilters)
    {
        _stringFilters = stringFilters
            .ToDictionary(stringFilter => stringFilter.GetKey())
            .ToFrozenDictionary();
    }

    public IFilter<string> Get(string key)
    {
        if (_stringFilters.TryGetValue(key, out var stringFilter))
        {
            return stringFilter;
        }

        this.ThrowSubFilterNotFoundException(key);
        return null;
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
    public override void Valid(JsonElement element)
    {
        element.AssertIsValidRegex(this);
    }

    public override async Task LazyMatchAsync(JsonElement element, IFilter<string>.MatchTargetGetter targetGetter, Dictionary<string, object>? args)
    {
        var (matchTarget, isExists) = await targetGetter(args);
        if (!isExists)
        {
            this.ThrowMatchTargetGetFailedException(args);
        }

        if (Regex.IsMatch(matchTarget, element.GetString()!, RegexOptions.None))
        {
            return;
        }

        this.ThrowNotMatchException(matchTarget, element.ToString());
    }

    public override void Match(JsonElement element, string matchTarget)
    {
        if (Regex.IsMatch(matchTarget, element.GetString()!, RegexOptions.None))
        {
            return;
        }

        this.ThrowNotMatchException(matchTarget, element.ToString());
    }
}
