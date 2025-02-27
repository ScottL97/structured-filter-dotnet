using StructuredFilter.Builder.Builders.Common;

namespace StructuredFilter.Builder.Builders.BasicBuilders.Common.SingleValueBuilders;

[FilterBuilderKey("$eq")]
public class EqBuilder<T>(T value) : SingleValueBuilder<T>(value);
