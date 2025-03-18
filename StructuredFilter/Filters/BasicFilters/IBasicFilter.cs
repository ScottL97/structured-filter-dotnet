using System;
using StructuredFilter.Filters.Common;

namespace StructuredFilter.Filters.BasicFilters;

public interface INumberFilter : IBasicFilter<double>;

public interface IStringFilter : IBasicFilter<string>;

public interface IVersionFilter : IBasicFilter<Version>;

public interface IBoolFilter : IBasicFilter<bool>;