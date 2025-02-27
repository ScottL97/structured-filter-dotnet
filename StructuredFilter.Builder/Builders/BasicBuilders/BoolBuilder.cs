using StructuredFilter.Builder.Builders.BasicBuilders.Common.SingleValueBuilders;

namespace StructuredFilter.Builder.Builders.BasicBuilders;

public class BoolEqBuilder(bool value) : EqBuilder<bool>(value);

public class BoolNeBuilder(bool value) : NeBuilder<bool>(value);
