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
    Task<FilterException?> LazyMatchAsync(JsonElement jsonElement, LazyObjectGetter<T> matchTargetGetter);

    FilterException? Match(JsonElement jsonElement, T matchTarget);
}

public interface ISceneFilter<T> : IFilter<T>
{
    Task<FilterException?> LazyMatchAsync(FilterKv filterKv, LazyObjectGetter<T> matchTargetGetter);

    /// <summary>
    /// Needs to be asynchronous as cache may need to be loaded asynchronously
    /// </summary>
    /// <param name="filterKv"></param>
    /// <param name="matchTarget"></param>
    Task<FilterException?> MatchAsync(FilterKv filterKv, T matchTarget);
}

public interface ILogicFilter<T> : IFilter<T>
{
    Task<FilterException?> LazyMatchAsync(FilterArray filterArray, LazyObjectGetter<T> matchTargetGetter);

    /// <summary>
    /// Needs to be asynchronous as cache may need to be loaded asynchronously
    /// </summary>
    /// <param name="filterArray"></param>
    /// <param name="matchTarget"></param>
    Task<FilterException?> MatchAsync(FilterArray filterArray, T matchTarget);
}
