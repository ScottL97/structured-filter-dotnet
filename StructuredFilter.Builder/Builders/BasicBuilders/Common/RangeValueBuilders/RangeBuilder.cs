using StructuredFilter.Builder.Builders.Common;

namespace StructuredFilter.Builder.Builders.BasicBuilders.Common.RangeValueBuilders;

[FilterBuilderKey("$range")]
public class RangeBuilder<T>(T leftValue, T rightValue) : RangeValueBuilder<T>(leftValue, rightValue);
