using System.Collections.Generic;
using StructuredFilter.Builder.Builders.BasicBuilders.Common.ArrayValueBuilders;
using StructuredFilter.Builder.Builders.BasicBuilders.Common.RangeValueBuilders;
using StructuredFilter.Builder.Builders.BasicBuilders.Common.SingleValueBuilders;

namespace StructuredFilter.Builder.Builders.BasicBuilders;

public class NumberInBuilder(IEnumerable<double> values) : InBuilder<double>(values);

public class NumberEqBuilder(double value) : EqBuilder<double>(value);

public class NumberNeBuilder(double value) : NeBuilder<double>(value);

public class NumberGtBuilder(double value) : GtBuilder<double>(value);

public class NumberGeBuilder(double value) : GeBuilder<double>(value);

public class NumberLtBuilder(double value) : LtBuilder<double>(value);

public class NumberLeBuilder(double value) : LeBuilder<double>(value);

public class NumberRangeBuilder(double leftValue, double rightValue) : RangeBuilder<double>(leftValue, rightValue);
