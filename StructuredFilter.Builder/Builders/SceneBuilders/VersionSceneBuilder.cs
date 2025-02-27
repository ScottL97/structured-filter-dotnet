using System;
using System.Collections.Generic;
using StructuredFilter.Builder.Builders.BasicBuilders;

namespace StructuredFilter.Builder.Builders.SceneBuilders;

public class VersionSceneBuilder : SceneBuilder
{
    public VersionSceneBuilder In(IEnumerable<Version> values)
    {
        SetBasicBuilder(new VersionInBuilder(values));
        return this;
    }

    public VersionSceneBuilder Eq(Version value)
    {
        SetBasicBuilder(new VersionEqBuilder(value));
        return this;
    }

    public VersionSceneBuilder Ne(Version value)
    {
        SetBasicBuilder(new VersionNeBuilder(value));
        return this;
    }

    public VersionSceneBuilder Gt(Version value)
    {
        SetBasicBuilder(new VersionGtBuilder(value));
        return this;
    }

    public VersionSceneBuilder Ge(Version value)
    {
        SetBasicBuilder(new VersionGeBuilder(value));
        return this;
    }

    public VersionSceneBuilder Lt(Version value)
    {
        SetBasicBuilder(new VersionLtBuilder(value));
        return this;
    }

    public VersionSceneBuilder Le(Version value)
    {
        SetBasicBuilder(new VersionLeBuilder(value));
        return this;
    }

    public VersionSceneBuilder Range(Version leftValue, Version rightValue)
    {
        SetBasicBuilder(new VersionRangeBuilder(leftValue, rightValue));
        return this;
    }
}
