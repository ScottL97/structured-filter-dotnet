namespace StructuredFilter.Filters.SceneFilters;

public record DynamicFilter(string Key, string BasicType, bool Cacheable=false, string? Label=null);
