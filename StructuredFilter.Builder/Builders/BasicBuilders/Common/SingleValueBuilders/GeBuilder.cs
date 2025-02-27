using StructuredFilter.Builder.Builders.Common;

namespace StructuredFilter.Builder.Builders.BasicBuilders.Common.SingleValueBuilders;

[FilterBuilderKey("$ge")]
public class GeBuilder<T>(T value) : SingleValueBuilder<T>(value);
