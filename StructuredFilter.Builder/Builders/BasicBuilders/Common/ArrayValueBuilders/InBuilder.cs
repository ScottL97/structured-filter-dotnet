using System.Collections.Generic;
using StructuredFilter.Builder.Builders.Common;

namespace StructuredFilter.Builder.Builders.BasicBuilders.Common.ArrayValueBuilders;

[FilterBuilderKey("$in")]
public class InBuilder<T>(IEnumerable<T> values) : ArrayValueBuilder<T>(values);
