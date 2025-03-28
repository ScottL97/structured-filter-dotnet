using System.Text.Json;
using Demo;
using Demo.Scenes;
using StructuredFilter;
using StructuredFilter.Filters.Common;
using StructuredFilter.Utils;

FilterService<Player> filterService;
try
{
    filterService = new FilterService<Player>().WithSceneFilters([
        f => new PidFilter(f),
        f => new UserNameFilter(f),
        f => new PlayerGameVersionFilter(f)
    ]);
}
catch (FilterException e)
{
    Console.WriteLine(e.Message);
    throw;
}

Console.WriteLine(JsonSerializer.Serialize(filterService.GetSceneFilterInfos(), new JsonSerializerOptions
{
    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    WriteIndented = true
}));

var player = new Player
{
    Level = 11,
    Name = "Dark Knight",
    Pid = 1000,
    User = new User
    {
        Name = "Scott",
        PhoneNumber = "12000000000",
        Uid = 1000000,
        Address = new Address
        {
            Country = "CN"
        },
        IsMale = true,
    },
    GameVersion = new Version("1.0.1")
};
var playerGetter = new LazyObjectGetter<Player>(_ => Task.FromResult<(Player, bool)>((player, true)), null);
var playerJson = JsonSerializer.Serialize(player);

var filterJson = "{\"userName\": {\"$regex\": \"^S\"}}";
var printFilterException = new Action<string, FilterException>((f, e) => 
{
    Console.WriteLine("**************************************************");
    Console.WriteLine($"【ERROR】 player {playerJson} match filter {f} exception: [{e.StatusCode}] {e.Message}");
    foreach (var filterKey in e.FailedKeyPath.Traverse())
    {
        Console.WriteLine($"failed filter key: {filterKey}");
    }
});
var printMatchSuccessfully = new Action<string, string>((p, f) =>
{
    Console.WriteLine("**************************************************");
    Console.WriteLine($"player {p} match filter {f} successfully");
});

try
{
    await filterService.LazyMustMatchAsync(filterJson, playerGetter);
    printMatchSuccessfully(playerJson, filterJson);
}
catch (FilterException e)
{
    printFilterException(filterJson, e);
}

try
{
    await filterService.MustMatchAsync(filterJson, player);
    printMatchSuccessfully(playerJson, filterJson);
}
catch (FilterException e)
{
    printFilterException(filterJson, e);
}

var filterException = await filterService.LazyMatchAsync(filterJson, playerGetter);
if (filterException.StatusCode == FilterStatusCode.Ok)
{
    printMatchSuccessfully(playerJson, filterJson);
}
else
{
    printFilterException(filterJson, filterException);
}

filterException = await filterService.MatchAsync(filterJson, player);
if (filterException.StatusCode == FilterStatusCode.Ok)
{
    printMatchSuccessfully(playerJson, filterJson);
}
else
{
    printFilterException(filterJson, filterException);
}

var players = new [] { player };
foreach (var filteredPlayer in await filterService.FilterOutAsync(filterJson, players))
{
    printMatchSuccessfully(JsonSerializer.Serialize(filteredPlayer), filterJson);
}

try
{
    filterJson = "{\"userName\": {\"$regex\": \"^A\"}}";
    await filterService.LazyMustMatchAsync(filterJson, playerGetter);
    printMatchSuccessfully(playerJson, filterJson);
}
catch (FilterException e)
{
    printFilterException(filterJson, e);
}
