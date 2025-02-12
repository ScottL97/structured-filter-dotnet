namespace StructuredFilter.Filters.SceneFilters;

public record DynamicFilter<T>(
    string Key,
    string BasicType,
    bool Cacheable=false,
    string? Label=null,
    IFilterResultCache<T>? Cache=null);
