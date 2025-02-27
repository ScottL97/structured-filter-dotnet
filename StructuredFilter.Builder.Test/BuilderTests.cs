using StructuredFilter.Builder.Builders.Common;
using StructuredFilter.Builder.Builders.LogicBuilders;
using StructuredFilter.Builder.Test.SceneBuilders;

namespace StructuredFilter.Builder.Test;

public class BuilderTests
{
    [Test]
    public void TestBuild()
    {
        var pidFilterBuilder = new PidFilterBuilder();
        var nameFilterBuilder = new UserNameFilterBuilder();
        var isMaleFilterBuilder = new IsMaleFilterBuilder();
        var playerGameVersionFilterBuilder = new PlayerGameVersionFilterBuilder();

        string rawFilter;
        var e = Assert.Catch<FilterBuilderException>(() => rawFilter = pidFilterBuilder.Build());
        Assert.That(e.Message, Is.EqualTo("Cannot build a SceneBuilder without BasicBuilder set"));

        rawFilter = nameFilterBuilder.Ne("Tom").Build();
        Assert.That(rawFilter, Is.EqualTo("{\"userName\":{\"$ne\":\"Tom\"}}"));
        Console.WriteLine(rawFilter);

        rawFilter = isMaleFilterBuilder.Eq(true).Build();
        Assert.That(rawFilter, Is.EqualTo("{\"isMale\":{\"$eq\":true}}"));
        Console.WriteLine(rawFilter);
        
        rawFilter = playerGameVersionFilterBuilder.Range(new Version("0.0.1"), new Version("0.1.5")).Build();
        Assert.That(rawFilter, Is.EqualTo("{\"playerGameVersion\":{\"$range\":[\"0.0.1\",\"0.1.5\"]}}"));
        Console.WriteLine(rawFilter);

        rawFilter = pidFilterBuilder.Eq(1000).Build();
        Assert.That(rawFilter, Is.EqualTo("{\"pid\":{\"$eq\":1000}}"));
        Console.WriteLine(rawFilter);

        rawFilter = pidFilterBuilder.Eq(1000).Build();
        Assert.That(rawFilter, Is.EqualTo("{\"pid\":{\"$eq\":1000}}"));
        Console.WriteLine(rawFilter);

        rawFilter = pidFilterBuilder.Eq(1000).Eq(2000).Build();
        Assert.That(rawFilter, Is.EqualTo("{\"pid\":{\"$eq\":2000}}"));
        Console.WriteLine(rawFilter);

        rawFilter = new AndLogicFilterBuilder([pidFilterBuilder.Eq(1000), nameFilterBuilder.Eq("Scott")]).Build();
        Assert.That(rawFilter, Is.EqualTo("{\"$and\":[{\"pid\":{\"$eq\":1000}},{\"userName\":{\"$eq\":\"Scott\"}}]}"));
        Console.WriteLine(rawFilter);

        rawFilter = new AndLogicFilterBuilder([pidFilterBuilder.Eq(1000), nameFilterBuilder.Eq("Scott")]).Build();
        Assert.That(rawFilter, Is.EqualTo("{\"$and\":[{\"pid\":{\"$eq\":1000}},{\"userName\":{\"$eq\":\"Scott\"}}]}"));
        Console.WriteLine(rawFilter);
    }
}