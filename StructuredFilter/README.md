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
  * long
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

#### Double filters

| Key    | Value Type   | Description                                                                                                                                                                                   | Filter Examples                  |
|--------|--------------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|----------------------------------|
| $eq    | double       | Match successfully when the value of the matching object and filter value are equal                                                                                                           | `{"score": {"$eq": 20.5}}`       |
| $ne    | double       | Match successfully when the value of the matching object and filter value are not equal                                                                                                       | `{"score": {"$ne": 20.5}}`       |
| $in    | double Array | Match successfully when the value of the matching object is equal to any of the filter values                                                                                                 | `{"score": {"$in": [20.5, 21.0, 22.5]}}` |
| $lt    | double       | Match successfully when the value of the matching object is less than the filter value                                                                                                        | `{"score": {"$lt": 20.5}}`       |
| $gt    | double       | Match successfully when the value of the matching object is greater than the filter value                                                                                                     | `{"score": {"$gt": 20.5}}`       |
| $le    | double       | Match successfully when the value of the matching object is less than or equal to the filter value                                                                                            | `{"score": {"$le": 20.5}}`       |
| $ge    | double       | Match successfully when the value of the matching object is greater than or equal to the filter value                                                                                         | `{"score": {"$ge": 20.5}}`       |
| $range | double Range | Match successfully when the value of the matching object is greater than or equal to the first element in the filter values and less than or equal to the second element in the filter values | `{"score": {"$range": [20.0, 30.0]}}` |

#### Long filters

| Key    | Value Type | Description                                                                                                                                                                                   | Filter Examples                  |
|--------|------------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|----------------------------------|
| $eq    | long       | Match successfully when the value of the matching object and filter value are equal                                                                                                           | `{"pid": {"$eq": 1000}}`         |
| $ne    | long       | Match successfully when the value of the matching object and filter value are not equal                                                                                                       | `{"pid": {"$ne": 1000}}`         |
| $in    | long Array | Match successfully when the value of the matching object is equal to any of the filter values                                                                                                 | `{"pid": {"$in": [1000, 1001, 1002]}}` |
| $lt    | long       | Match successfully when the value of the matching object is less than the filter value                                                                                                        | `{"pid": {"$lt": 1000}}`         |
| $gt    | long       | Match successfully when the value of the matching object is greater than the filter value                                                                                                     | `{"pid": {"$gt": 1000}}`         |
| $le    | long       | Match successfully when the value of the matching object is less than or equal to the filter value                                                                                            | `{"pid": {"$le": 1000}}`         |
| $ge    | long       | Match successfully when the value of the matching object is greater than or equal to the filter value                                                                                         | `{"pid": {"$ge": 1000}}`         |
| $range | long Range | Match successfully when the value of the matching object is greater than or equal to the first element in the filter values and less than or equal to the second element in the filter values | `{"pid": {"$range": [1000, 2000]}}` |

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
  * DoubleSceneFilter
  * LongSceneFilter
  * StringSceneFilter
  * VersionSceneFilter

## Features

### Cacheable SceneFilter

* If a filter result is cacheable, you can integrate caching in the following ways. First add Cacheable attribute:

```csharp
[FilterLabel("玩家 ID")]
[FilterKey("pid")]
[Cacheable]
public class PidFilter(FilterFactory<Player> filterFactory)
    : LongSceneFilter<Player>(filterFactory, player => Task.FromResult(player.Pid));
```

* Implement IFilterResultCache:

```csharp
public class PlayerFilterCache : IFilterResultCache<Player>
{
    private readonly Dictionary<string, bool> _cacheData = new ();
    public int HitCount = 0;

    public Task<Tuple<bool, bool>> GetFilterResultCacheAsync(Player matchTarget, string filterKey, FilterKv filterKv)
    {
        if (_cacheData.TryGetValue(GetCacheKey(filterKey, filterKv, matchTarget.Pid), out var filterResult))
        {
            Interlocked.Add(ref HitCount, 1);
            return Task.FromResult(new Tuple<bool, bool>(filterResult, true));
        }
        return Task.FromResult(new Tuple<bool, bool>(false, false));
    }

    public Task SetFilterResultCacheAsync(Player matchTarget, string filterKey, FilterKv filterKv, bool result)
    {
        _cacheData[GetCacheKey(filterKey, filterKv, matchTarget.Pid)] = result;
        return Task.CompletedTask;
    }

    private string GetCacheKey(string filterKey, FilterKv filterKv, long pid)
    {
        return $"{filterKey}:{filterKv.Key}:{filterKv.Value}:{pid}";
    }
}
```

* Pass in the IFilterResultCache implementation in the constructor:

```csharp
[FilterLabel("玩家 ID")]
[FilterKey("pid")]
[Cacheable]
public class PidFilter(FilterFactory<Player> filterFactory)
    : LongSceneFilter<Player>(filterFactory, player => Task.FromResult(player.Pid), new PlayerFilterCache());
```

## FilterValidator

* FilterValidator is used to verify whether a JSON string or a System.Text.Json.JsonDocument is a valid filter:

```csharp
try
{
    // Valid filters:
    FilterValidator.MustValid("{\"pid\": 5000}", filterFactory);
    FilterValidator.MustValid("{\"pid\": {\"$ne\": 2000}}", filterFactory);
    // Invalid filters:
    FilterValidator.MustValid("", filterFactory); // Treat empty string as invalid filter
    FilterValidator.MustValid("{\"pid\": \"5000\"}", filterFactory); // Wrong value type for pid filter
    FilterValidator.MustValid("{\"pid\": 5000", filterFactory); // Invalid JSON format
catch (FilterException e)
{
    Console.WriteLine($"invalid filter: {e}");
    throw;
}
```

* Validate with FilterService:

```csharp
try
{
    _filterService.MustValidFilter("{\"pid\": 5000"); // Invalid JSON format
}
catch (FilterException e)
{
    Console.WriteLine($"invalid filter: {e}");
    throw;
}
```
