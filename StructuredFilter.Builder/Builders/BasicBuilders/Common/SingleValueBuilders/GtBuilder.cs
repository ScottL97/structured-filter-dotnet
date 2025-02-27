using StructuredFilter.Builder.Builders.Common;

namespace StructuredFilter.Builder.Builders.BasicBuilders.Common.SingleValueBuilders;

[FilterBuilderKey("$gt")]
public class GtBuilder<T>(T value) : SingleValueBuilder<T>(value);
