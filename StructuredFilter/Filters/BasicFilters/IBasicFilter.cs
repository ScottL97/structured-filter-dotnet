using System;
using StructuredFilter.Filters.Common;

namespace StructuredFilter.Filters.BasicFilters;

public interface IDoubleFilter : IBasicFilter<double>;

public interface ILongFilter : IBasicFilter<long>;

public interface IStringFilter : IBasicFilter<string>;

public interface IVersionFilter : IBasicFilter<Version>;

public interface IBoolFilter : IBasicFilter<bool>;