using System;

namespace StructuredFilter.Filters.BasicFilters;

public interface INumberFilter : IFilter<double>;

public interface IStringFilter : IFilter<string>;

public interface IVersionFilter : IFilter<Version>;

public interface IBoolFilter : IFilter<bool>;