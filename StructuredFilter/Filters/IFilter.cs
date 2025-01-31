using System.Text.Json;
using System.Threading.Tasks;
using StructuredFilter.Utils;

namespace StructuredFilter.Filters;

public interface IFilter<T>
{
    string GetKey();
    string GetLabel();
    string GetBasicType();
    void Valid(JsonElement element);
    Task LazyMatchAsync(JsonElement element, LazyObjectGetter<T> matchTargetGetter);
    void Match(JsonElement element, T matchTarget);
}
