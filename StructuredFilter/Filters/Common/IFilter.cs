using System.Text.Json;
using System.Threading.Tasks;
using StructuredFilter.Utils;

namespace StructuredFilter.Filters.Common;

public interface IFilter<T>
{
    string GetKey();
    string GetLabel();
    string GetBasicType();
    FilterException? Valid(JsonElement element);
}

public interface IBasicFilter<T> : IFilter<T>
{
    ValueTask<FilterException?> LazyMatchAsync(FilterValue filterValue, LazyObjectGetter<T> matchTargetGetter);

    FilterException? Match(FilterValue filterValue, T matchTarget);
}

public interface ISceneFilter<T> : IFilter<T>
{
    ValueTask<FilterException?> LazyMatchAsync(FilterKv filterKv, LazyObjectGetter<T> matchTargetGetter);

    /// <summary>
    /// Needs to be asynchronous as cache may need to be loaded asynchronously
    /// </summary>
    /// <param name="filterKv"></param>
    /// <param name="matchTarget"></param>
    ValueTask<FilterException?> MatchAsync(FilterKv filterKv, T matchTarget);
}

public interface ILogicFilter<T> : IFilter<T>
{
    ValueTask<FilterException?> LazyMatchAsync(FilterArray filterArray, LazyObjectGetter<T> matchTargetGetter);

    /// <summary>
    /// Needs to be asynchronous as cache may need to be loaded asynchronously
    /// </summary>
    /// <param name="filterArray"></param>
    /// <param name="matchTarget"></param>
    ValueTask<FilterException?> MatchAsync(FilterArray filterArray, T matchTarget);
}
