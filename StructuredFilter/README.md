# StructuredFilter

A general-purpose, business-agnostic structured filter library.

## How to use

* Suppose you need to filter out Players that meet certain conditions from a Player list.

```csharp
public class User
{
    public long Uid { get; set; }
    public string Name { get; set; }
}

public class Player
{
    public User User { get; set; }
    public long Pid { get; set; }
    public string Name { get; set; }
    public Version GameVersion { get; set; }
}

var players = new[]
{
    new Player
    {
        Name = "Dark Knight",
        Pid = 1000,
        User = new User { Name = "Scott", Uid = 1000000 },
        GameVersion = new Version("1.0.1")
    },
    new Player
    {
        Name = "Joker",
        Pid = 1050,
        User = new User { Name = "Tom", Uid = 990000 },
        GameVersion = new Version("1.0.2")
    }
};
```

* Define Scene filters for the object members you expect to match.

```csharp
[FilterLabel("玩家 ID")]
[FilterKey("pid")]
public class PidFilter(FilterFactory<Player> filterFactory)
    : NumberSceneFilter<Player>(filterFactory, player => player.Pid);

[FilterLabel("用户名")]
[FilterKey("userName")]
public class UserNameFilter(FilterFactory<Player> filterFactory)
    : StringSceneFilter<Player>(filterFactory, player => player.User.Name);

[FilterLabel("玩家游戏版本")]
[FilterKey("playerGameVersion")]
public class PlayerGameVersionFilter(FilterFactory<Player> filterFactory)
    : VersionSceneFilter<Player>(filterFactory, player => player.GameVersion);
```

* Create your `FilterService<Player>` and register the just defined filters.

```csharp
var filterService = new FilterService<Player>().WithSceneFilters([
    f => new PidFilter(f),
    f => new UserNameFilter(f),
    f => new PlayerGameVersionFilter(f)
]);
```

* Use filter JSON string to filter your object list. You can also match an object lazily or non-lazily, refer to the [demo](https://github.com/ScottL97/structured-filter-dotnet/blob/main/Demo/Program.cs) or [tests](https://github.com/ScottL97/structured-filter-dotnet/blob/main/StructuredFilter.Test/StructuredFilterTests.cs).

```csharp
var filterJson = "{\"pid\": {\"$range\": [1000, 1010]}}";
var filteredList = filterService.FilterOut(filterJson, players);
foreach (var filteredPlayer in filteredList)
{
    Console.WriteLine($"player {JsonSerializer.Serialize(filteredPlayer)} match filter {filterJson} successfully");
}
// player "Dark Knight" with Pid 1000 is in filteredList since he matches the range [1000, 1010]
// player "Joker" with Pid 1050 isn't in filteredList since he doesn't match the range [1000, 1010]
```

## Data types for StructuredFilter

### Object

* Object is a JSON object but allows exactly one key-value pair.
* Examples: `{"userName": "Scott"}`, `{"userName": {"$eq": "Scott"}}`
* Invalid: `{"userName": "Scott", "age": 20}`, `{"userName": {"$eq": "Scott"}, "age": {"$eq": 20}}`

### Array

* Array is a JSON array containing at least one element, and all elements need to be of the same type.
* Examples: `[{"userName": "Scott"}, {"age": 20}]`, `[1, 2, 3]`, `["a", "b", "c"]`
* Invalid: `[]`, `[{"userName": "Scott"}, 20]`, `[1, 2, "c"]`

### Range

* Range is a StructuredFilter Array containing two elements, the first element of the Array is less than or equal to the second element, and the elements need to be one of the following types:
  * string
  * double
  * Version
* The value of Range uses a closed interval, that is, the match is successful when it is greater than or equal to the first element and less than or equal to the second element.
* Examples: `[1, 2]`、`["a", "z"]`, `[Version.Parse("1.0.0"), Version.Parse("1.6.0")]`
* Invalid: `[]`, `[1]`, `[1, 2, 3]`, `[3, 1]`

## Filter types

### Logic filters

* Logic filters currently does not support nested Logic filters.

| Key  | Value Type                 | Description                                                          | Filter Examples                                                      |
|------|----------------------------|----------------------------------------------------------------------|----------------------------------------------------------------------|
| $and | Array with Object Elements | Match successfully when all elements of the array match successfully | `{"$and": [{"pid": {"$lt": 1000}}, {"userName": {"$eq": "Scott"}}]}` |
| $or  | Array with Object Elements | Match successfully when any element of the array match successfully  | `{"$or": [{"pid": {"$lt": 1000}}, {"userName": {"$eq": "Scott"}}]}`  |

### Basic filters

#### Bool filters

| Key | Value Type | Description                                                                             | Filter Examples             |
|-----|------------|-----------------------------------------------------------------------------------------|-----------------------------|
| $eq | bool       | Match successfully when the value of the matching object and filter value are equal     | `{"isMale": {"$eq": true}}` |
| $ne | bool       | Match successfully when the value of the matching object and filter value are not equal | `{"isMale": {"$ne": true}}` |

#### Number filters

| Key    | Value Type   | Description                                                                                                                                                                                   | Filter Examples                  |
|--------|--------------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|----------------------------------|
| $eq    | double       | Match successfully when the value of the matching object and filter value are equal                                                                                                           | `{"age": {"$eq": 20}}`           |
| $ne    | double       | Match successfully when the value of the matching object and filter value are not equal                                                                                                       | `{"age": {"$ne": 20}}`           |
| $in    | double Array | Match successfully when the value of the matching object is equal to any of the filter values                                                                                                 | `{"age": {"$in": [20, 21, 22]}}` |
| $lt    | double       | Match successfully when the value of the matching object is less than the filter value                                                                                                        | `{"age": {"$lt": 20}}`           |
| $gt    | double       | Match successfully when the value of the matching object is greater than the filter value                                                                                                     | `{"age": {"$gt": 20}}`           |
| $le    | double       | Match successfully when the value of the matching object is less than or equal to the filter value                                                                                            | `{"age": {"$le": 20}}`           |
| $ge    | double       | Match successfully when the value of the matching object is greater than or equal to the filter value                                                                                         | `{"age": {"$ge": 20}}`           |
| $range | double Range | Match successfully when the value of the matching object is greater than or equal to the first element in the filter values and less than or equal to the second element in the filter values | `{"age": {"$range": [20, 30]}}`  |

#### String filters

| Key    | Value Type                 | Description                                                                                                                                                                                   | Filter Examples                                              |
|--------|----------------------------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|--------------------------------------------------------------|
| $eq    | string                     | Match successfully when the value of the matching object and filter value are equal                                                                                                           | `{"userName": {"$eq": "Scott"}}`                             |
| $ne    | string                     | Match successfully when the value of the matching object and filter value are not equal                                                                                                       | `{"userName": {"$ne": "Scott"}}`                             |
| $in    | string Array               | Match successfully when the value of the matching object is equal to any of the filter values                                                                                                 | `{"userName": {"$in": ["Scott", "Tom", "Bob"]}}`             |
| $range | string Range               | Match successfully when the value of the matching object is greater than or equal to the first element in the filter values and less than or equal to the second element in the filter values | `{"serialNumber": {"$range": ["abcde00001", "abcde99999"]}}` |
| $regex | string(regular expression) | Match successfully when the value of the matching object match filter value as regular expression                                                                                             | `{"userName": {"$regex": "^S"}}`                             |

#### Version filters

| Key    | Value Type    | Description                                                                                                                                                                                   | Filter Examples                                         |
|--------|---------------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|---------------------------------------------------------|
| $eq    | Version       | Match successfully when the value of the matching object and filter value are equal                                                                                                           | `{"gameVersion": {"$eq": "1.0.0"}}`                     |
| $ne    | Version       | Match successfully when the value of the matching object and filter value are not equal                                                                                                       | `{"gameVersion": {"$ne": "1.0.0"}}`                     |
| $in    | Version Array | Match successfully when the value of the matching object is equal to any of the filter values                                                                                                 | `{"gameVersion": {"$in": ["1.0.0", "1.1.0", "1.3.0"]}}` |
| $lt    | Version       | Match successfully when the value of the matching object is less than the filter value                                                                                                        | `{"gameVersion": {"$lt": "1.0.0"}}`                     |
| $gt    | Version       | Match successfully when the value of the matching object is greater than the filter value                                                                                                     | `{"gameVersion": {"$gt": "1.0.0"}}`                     |
| $le    | Version       | Match successfully when the value of the matching object is less than or equal to the filter value                                                                                            | `{"gameVersion": {"$le": "1.0.0"}}`                     |
| $ge    | Version       | Match successfully when the value of the matching object is greater than or equal to the filter value                                                                                         | `{"gameVersion": {"$ge": "1.0.0"}}`                     |
| $range | Version Range | Match successfully when the value of the matching object is greater than or equal to the first element in the filter values and less than or equal to the second element in the filter values | `{"gameVersion": {"$range": ["1.0.0", "1.6.0"]}}`       |

### Scene filters

* Scene filters are defined by the user and can be nested as subordinates of Logic filters or superior of Basic filters.
* The following abstract classes of scene filters are provided to simplify the writing of your scene filters:
  * BoolSceneFilter
  * NumberSceneFilter
  * StringSceneFilter
  * VersionSceneFilter
