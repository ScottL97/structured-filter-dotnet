namespace StructuredFilter.Filters.Common;

public readonly record struct FilterDocument<T>
{
    private string RawFilter { get; }
    private FilterTree Tree { get; }

    public FilterDocument(string rawFilter, FilterFactory<T> filterFactory)
    {
        if (string.IsNullOrWhiteSpace(rawFilter))
        {
            throw new FilterException(FilterStatusCode.Invalid, "Filter cannot be empty");
        }

        RawFilter = FilterNormalizer.Normalize(rawFilter);
        Tree = FilterTree.Parse(RawFilter, filterFactory);
    }

    public bool IsRootLogicFilter()
    {
        return Tree.Root.FilterArray is not null;
    }

    public FilterArray GetRootFilterArray()
    {
        return Tree.Root.FilterArray!.Value;
    }

    public bool IsRootSceneFilter()
    {
        return Tree.Root.FilterKv is not null;
    }

    public FilterKv GetRootFilterKv()
    {
        return Tree.Root.FilterKv!.Value;
    }

    public string GetRootKey()
    {
        return Tree.Root.Key;
    }
}
