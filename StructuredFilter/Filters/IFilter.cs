using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace StructuredFilter.Filters;

public interface IFilter<T>
{
    delegate Task<(T, bool)> MatchTargetGetter(Dictionary<string, object>? args);
    string GetKey();
    string GetLabel();
    string GetBasicType();
    void Valid(JsonElement element);
    Task LazyMatchAsync(JsonElement element, MatchTargetGetter targetGetter, Dictionary<string, object>? args);
    void Match(JsonElement element, T matchTarget);
}
