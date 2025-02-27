using StructuredFilter.Builder.Builders.Common;

namespace StructuredFilter.Builder.Builders.BasicBuilders.Common.SingleValueBuilders;

[FilterBuilderKey("$ne")]
public class NeBuilder<T>(T value) : SingleValueBuilder<T>(value);
