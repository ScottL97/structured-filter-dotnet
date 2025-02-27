using System.Text.Json;

namespace StructuredFilter.Filters.Common;

public class FilterDocument<T>
{
    private string RawFilter { get; set; }
    public JsonDocument Document { get; set; }

    public FilterDocument(string rawFilter, FilterFactory<T> filterFactory)
    {
        if (string.IsNullOrWhiteSpace(rawFilter))
        {
            throw new FilterException(FilterStatusCode.Invalid, "Filter cannot be empty");
        }

        RawFilter = FilterNormalizer.Normalize(rawFilter);
        Document = JsonDocument.Parse(RawFilter);

        FilterValidator.MustValid(Document, filterFactory);
    }
}
