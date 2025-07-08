# StructuredFilter.Builder

Filter JSON builder for [StructuredFilter](https://www.nuget.org/packages/StructuredFilter)

## How to use

* Suppose you have these Scene filters:

```csharp
[FilterLabel("玩家 ID")]
[FilterKey("pid")]
public class PidFilter(FilterFactory<Player> filterFactory)
    : LongSceneFilter<Player>(filterFactory, player => player.Pid);

[FilterLabel("用户名")]
[FilterKey("userName")]
public class UserNameFilter(FilterFactory<Player> filterFactory)
    : StringSceneFilter<Player>(filterFactory, player => player.User.Name);

[FilterLabel("玩家游戏版本")]
[FilterKey("playerGameVersion")]
public class PlayerGameVersionFilter(FilterFactory<Player> filterFactory)
    : VersionSceneFilter<Player>(filterFactory, player => player.GameVersion);
```

* Create the FilterBuilders corresponding to the Scene filters.

```csharp
[FilterBuilderKey("pid")]
public class PidFilterBuilder : NumberSceneBuilder;

[FilterBuilderKey("userName")]
public class UserNameFilterBuilder : StringSceneBuilder;

[FilterBuilderKey("playerGameVersion")]
public class PlayerGameVersionFilterBuilder : VersionSceneBuilder;
```

* Build your filter JSONs:

```csharp
var filter1 = new PidFilterBuilder().Eq(1000);
Console.WriteLine(filter1.Build());
// {"pid":{"$eq":1000}}

var filter2 = new UserNameFilterBuilder().Ne("Tom");
Console.WriteLine(filter2.Build());
// {"userName":{"$ne":"Tom"}}

var filter = new AndLogicFilterBuilder([filter1, filter2]);
Console.WriteLine(filter.Build());
// {"$and":[{"pid":{"$eq":1000}},{"userName":{"$ne":"Tom"}}]}
```

## Data types for StructuredFilter.Builder

### Logic filter builders

* AndLogicFilterBuilder
* OrLogicFilterBuilder

### Abstract Scene filter builders

* BoolSceneBuilder
* NumberSceneBuilder
* StringSceneBuilder
* VersionSceneBuilder

### Basic filter builders

* Refer to the basic filter types of [StructuredFilter](https://www.nuget.org/packages/StructuredFilter)
