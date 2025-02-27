using System;
using System.Collections.Generic;
using StructuredFilter.Builder.Builders.BasicBuilders.Common.ArrayValueBuilders;
using StructuredFilter.Builder.Builders.BasicBuilders.Common.RangeValueBuilders;
using StructuredFilter.Builder.Builders.BasicBuilders.Common.SingleValueBuilders;

namespace StructuredFilter.Builder.Builders.BasicBuilders;

public class VersionInBuilder(IEnumerable<Version> values) : InBuilder<Version>(values);

public class VersionEqBuilder(Version value) : EqBuilder<Version>(value);

public class VersionNeBuilder(Version value) : NeBuilder<Version>(value);

public class VersionGtBuilder(Version value) : GtBuilder<Version>(value);

public class VersionGeBuilder(Version value) : GeBuilder<Version>(value);

public class VersionLtBuilder(Version value) : LtBuilder<Version>(value);

public class VersionLeBuilder(Version value) : LeBuilder<Version>(value);

public class VersionRangeBuilder(Version leftValue, Version rightValue) : RangeBuilder<Version>(leftValue, rightValue);
