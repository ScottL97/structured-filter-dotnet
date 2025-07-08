using System.Collections.Generic;
using StructuredFilter.Builder.Builders.BasicBuilders.Common.ArrayValueBuilders;
using StructuredFilter.Builder.Builders.BasicBuilders.Common.RangeValueBuilders;
using StructuredFilter.Builder.Builders.BasicBuilders.Common.SingleValueBuilders;

namespace StructuredFilter.Builder.Builders.BasicBuilders;

public class LongInBuilder(IEnumerable<long> values) : InBuilder<long>(values);

public class LongEqBuilder(long value) : EqBuilder<long>(value);

public class LongNeBuilder(long value) : NeBuilder<long>(value);

public class LongGtBuilder(long value) : GtBuilder<long>(value);

public class LongGeBuilder(long value) : GeBuilder<long>(value);

public class LongLtBuilder(long value) : LtBuilder<long>(value);

public class LongLeBuilder(long value) : LeBuilder<long>(value);

public class LongRangeBuilder(long leftValue, long rightValue) : RangeBuilder<long>(leftValue, rightValue);
