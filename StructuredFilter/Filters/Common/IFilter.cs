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
    Task LazyMatchAsync(JsonElement element, LazyObjectGetter<T> matchTargetGetter);
    /// <summary>
    /// Needs to be asynchronous as cache may need to be loaded asynchronously
    /// </summary>
    /// <param name="element"></param>
    /// <param name="matchTarget"></param>
    Task MatchAsync(JsonElement element, T matchTarget);
}
