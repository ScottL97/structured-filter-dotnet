using StructuredFilter.Builder.Builders.Common;

namespace StructuredFilter.Builder.Builders.BasicBuilders.Common.SingleValueBuilders;

[FilterBuilderKey("$lt")]
public class LtBuilder<T>(T value) : SingleValueBuilder<T>(value);
