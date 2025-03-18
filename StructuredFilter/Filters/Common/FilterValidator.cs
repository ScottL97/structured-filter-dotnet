namespace StructuredFilter.Filters.Common;

public static class FilterValidator
{
    public static void MustValid<T>(string rawFilter, FilterFactory<T> filterFactory)
    {
        if (string.IsNullOrWhiteSpace(rawFilter))
        {
            throw new FilterException(FilterStatusCode.Invalid, "Filter cannot be empty");
        }

        rawFilter = FilterNormalizer.Normalize(rawFilter);
        FilterTree.Parse(rawFilter, filterFactory);
    }
}
