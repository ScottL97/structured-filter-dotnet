using System.Collections.Generic;
using StructuredFilter.Builder.Builders.BasicBuilders.Common.ArrayValueBuilders;
using StructuredFilter.Builder.Builders.BasicBuilders.Common.RangeValueBuilders;
using StructuredFilter.Builder.Builders.BasicBuilders.Common.SingleValueBuilders;

namespace StructuredFilter.Builder.Builders.BasicBuilders;

public class DoubleInBuilder(IEnumerable<double> values) : InBuilder<double>(values);

public class DoubleEqBuilder(double value) : EqBuilder<double>(value);

public class DoubleNeBuilder(double value) : NeBuilder<double>(value);

public class DoubleGtBuilder(double value) : GtBuilder<double>(value);

public class DoubleGeBuilder(double value) : GeBuilder<double>(value);

public class DoubleLtBuilder(double value) : LtBuilder<double>(value);

public class DoubleLeBuilder(double value) : LeBuilder<double>(value);

public class DoubleRangeBuilder(double leftValue, double rightValue) : RangeBuilder<double>(leftValue, rightValue);
