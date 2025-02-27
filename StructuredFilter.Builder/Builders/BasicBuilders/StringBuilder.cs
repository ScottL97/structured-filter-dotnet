using System.Collections.Generic;
using StructuredFilter.Builder.Builders.BasicBuilders.Common;
using StructuredFilter.Builder.Builders.BasicBuilders.Common.ArrayValueBuilders;
using StructuredFilter.Builder.Builders.BasicBuilders.Common.RangeValueBuilders;
using StructuredFilter.Builder.Builders.BasicBuilders.Common.SingleValueBuilders;
using StructuredFilter.Builder.Builders.Common;

namespace StructuredFilter.Builder.Builders.BasicBuilders;

public class StringInBuilder(IEnumerable<string> values) : InBuilder<string>(values);

public class StringEqBuilder(string value) : EqBuilder<string>(value);

public class StringNeBuilder(string value) : NeBuilder<string>(value);

public class StringRangeBuilder(string leftValue, string rightValue) : RangeBuilder<string>(leftValue, rightValue);

[FilterBuilderKey("$regex")]
public class StringRegexBuilder(string regex) : SingleValueBuilder<string>(regex);
