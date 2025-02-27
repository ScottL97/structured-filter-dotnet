using StructuredFilter.Builder.Builders.Common;

namespace StructuredFilter.Builder.Builders.BasicBuilders.Common.SingleValueBuilders;

[FilterBuilderKey("$le")]
public class LeBuilder<T>(T value) : SingleValueBuilder<T>(value);
