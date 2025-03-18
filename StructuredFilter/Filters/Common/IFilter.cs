using System.Text.Json;
using System.Threading.Tasks;
using StructuredFilter.Utils;

namespace StructuredFilter.Filters.Common;

public interface IFilter<T>
{
    string GetKey();
    string GetLabel();
    string GetBasicType();
    void Valid(JsonElement element);
}

public interface IBasicFilter<T> : IFilter<T>
{
    Task LazyMatchAsync(JsonElement jsonElement, LazyObjectGetter<T> matchTargetGetter);

    /// <summary>
    /// Needs to be asynchronous as cache may need to be loaded asynchronously
    /// </summary>
    /// <param name="jsonElement"></param>
    /// <param name="matchTarget"></param>
    Task MatchAsync(JsonElement jsonElement, T matchTarget);
}

public interface ISceneFilter<T> : IFilter<T>
{
    Task LazyMatchAsync(FilterKv filterKv, LazyObjectGetter<T> matchTargetGetter);

    /// <summary>
    /// Needs to be asynchronous as cache may need to be loaded asynchronously
    /// </summary>
    /// <param name="filterKv"></param>
    /// <param name="matchTarget"></param>
    Task MatchAsync(FilterKv filterKv, T matchTarget);
}

public interface ILogicFilter<T> : IFilter<T>
{
    Task LazyMatchAsync(FilterArray filterArray, LazyObjectGetter<T> matchTargetGetter);

    /// <summary>
    /// Needs to be asynchronous as cache may need to be loaded asynchronously
    /// </summary>
    /// <param name="filterArray"></param>
    /// <param name="matchTarget"></param>
    Task MatchAsync(FilterArray filterArray, T matchTarget);
}
