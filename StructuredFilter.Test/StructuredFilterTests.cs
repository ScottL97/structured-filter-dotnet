using System.Text.Json;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using StructuredFilter.Filters;
using StructuredFilter.Filters.Common;
using StructuredFilter.Test.Models;
using StructuredFilter.Test.Scenes;

namespace StructuredFilter.Test;

public partial class StructuredFilterTests
{
    private static readonly Player Player1 = new()
    {
        Level = 11,
        Name = "Dark Knight",
        Pid = 1000,
        Pets = ["cat-1", "dog-3"],
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
    private static readonly string Player1Json = JsonSerializer.Serialize(Player1);
    private static readonly IFilter<Player>.MatchTargetGetter Player1Getter = _ => Task.FromResult((Player1, true));
    private static readonly JObject Player1JObject = Newtonsoft.Json.JsonConvert.DeserializeObject<JObject>(Player1Json)!;
    private static readonly IFilter<JObject>.MatchTargetGetter Player1JObjectGetter = _ => Task.FromResult((Player1JObject, true));

    private static readonly Player Player2 = new()
    {
        Level = 13,
        Name = "Joker",
        Pid = 1030,
        Pets = ["cat-5"],
        User = new User
        {
            Name = "Tom",
            PhoneNumber = "12000020000",
            Uid = 1003000,
            Address = new Address
            {
                Country = "US"
            },
            IsMale = false,
        },
        GameVersion = new Version("1.0.2")
    };
    private static readonly string Player2Json = JsonSerializer.Serialize(Player2);
    private static readonly IFilter<Player>.MatchTargetGetter Player2Getter = _ => Task.FromResult((Player2, true));
    private static readonly JObject Player2JObject = Newtonsoft.Json.JsonConvert.DeserializeObject<JObject>(Player2Json)!;
    private static readonly IFilter<JObject>.MatchTargetGetter Player2JObjectGetter = _ => Task.FromResult((Player2JObject, true));

    private static readonly List<Player> Players = [Player1, Player2];
    private static readonly List<IFilter<Player>.MatchTargetGetter> PlayersGetter = [Player1Getter, Player2Getter];

    private readonly FilterService<Player> _filterService = new FilterService<Player>().WithSceneFilters([
        f => new PidFilter(f),
        f => new UserNameFilter(f),
        f => new PlayerGameVersionFilter(f)
    ]);

    private readonly JsonPathFilterService _jsonPathFilterService = new ();

    [Test]
    public void ShouldFailedWhenAddFiltersWithTheSameKey()
    {
        var e = Assert.Throws<ArgumentException>(() =>
        {
            _ = new FilterService<Player>().WithSceneFilters([
                f => new PidFilter(f),
                f => new PidFilter(f)
            ]);
        });

        Assert.That(e.Message, Does.StartWith("An item with the same key has already been added"));
    }

    [GeneratedRegex(@"\s")] private static partial Regex WhitespaceRegex();

    [Test]
    public void GetSceneFilterInfosTest()
    {
        var sceneFilterInfos = JsonSerializer.Serialize(_filterService.GetSceneFilterInfos(), new JsonSerializerOptions
        {
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = true
        });

        Console.WriteLine(sceneFilterInfos);
        Assert.That(WhitespaceRegex().Replace(sceneFilterInfos, ""), Is.EqualTo("{\"pid\":{\"label\":\"玩家ID\",\"logics\":[{\"label\":\"属于\",\"value\":\"$in\"},{\"label\":\"不等于\",\"value\":\"$ne\"},{\"label\":\"等于\",\"value\":\"$eq\"},{\"label\":\"大于\",\"value\":\"$gt\"},{\"label\":\"在此范围（包含两端值）\",\"value\":\"$range\"},{\"label\":\"小于等于\",\"value\":\"$le\"},{\"label\":\"大于等于\",\"value\":\"$ge\"},{\"label\":\"小于\",\"value\":\"$lt\"}],\"type\":\"NUMBER\"},\"userName\":{\"label\":\"用户名\",\"logics\":[{\"label\":\"等于\",\"value\":\"$eq\"},{\"label\":\"不等于\",\"value\":\"$ne\"},{\"label\":\"属于\",\"value\":\"$in\"},{\"label\":\"匹配正则表达式\",\"value\":\"$regex\"},{\"label\":\"在此范围（包含两端值）\",\"value\":\"$range\"}],\"type\":\"STRING\"},\"playerGameVersion\":{\"label\":\"玩家游戏版本\",\"logics\":[{\"label\":\"属于\",\"value\":\"$in\"},{\"label\":\"不等于\",\"value\":\"$ne\"},{\"label\":\"等于\",\"value\":\"$eq\"},{\"label\":\"大于\",\"value\":\"$gt\"},{\"label\":\"在此范围（包含两端值）\",\"value\":\"$range\"},{\"label\":\"小于等于\",\"value\":\"$le\"},{\"label\":\"大于等于\",\"value\":\"$ge\"},{\"label\":\"小于\",\"value\":\"$lt\"}],\"type\":\"VERSION\"}}"));
    }

    [Test]
    public async Task ShouldMatchSuccessfully()
    {
        string[] filterJsons = [
            "{\"pid\": {\"$in\": [1000, 1001]}}",
            string.Empty,
            "{\"$and\": [{\"pid\": {\"$in\": [1000, 1001]}}, {\"userName\": {\"$eq\": \"Scott\"}}]}",
            "{\"$and\": [{\"userName\": {\"$eq\": \"Scott\"}}, {\"playerGameVersion\": {\"$le\": \"1.1.0\"}}]}",
            "{\"$and\": [{\"playerGameVersion\": {\"$lt\": \"2.0.0\"}}, {\"playerGameVersion\": {\"$gt\": \"0.9.0\"}}]}",
            "{\"playerGameVersion\": {\"$range\": [\"1.0.0\", \"2.0.0\"]}}",
            "{\"$or\": [{\"pid\": {\"$lt\": 10000}}, {\"userName\": {\"$in\": [\"Alice\", \"Bob\"]}}]}",
            "{\"$or\": [{\"pid\": {\"$ne\": 1000}}, {\"pid\": {\"$range\": [0, 10000]}}]}",
            "{\"userName\": \"Scott\"}",
            "{\"userName\": {\"$regex\": \"^S\"}}"
        ];

        foreach (var filterJson in filterJsons)
        {
            await _filterService.LazyMustMatchAsync(filterJson, Player1Getter, null);
            Console.WriteLine($"player {Player1Json} lazy must match filter {filterJson} successfully");

            _filterService.MustMatch(filterJson, Player1);
            Console.WriteLine($"player {Player1Json} must match filter {filterJson} successfully");

            var filterException = await _filterService.LazyMatchAsync(filterJson, Player1Getter, null);
            Assert.That(filterException.StatusCode, Is.EqualTo(FilterStatusCode.Ok));
            Console.WriteLine($"player {Player1Json} lazy match filter {filterJson} successfully");

            filterException = _filterService.Match(filterJson, Player1);
            Assert.That(filterException.StatusCode, Is.EqualTo(FilterStatusCode.Ok));
            Console.WriteLine($"player {Player1Json} match filter {filterJson} successfully");
        }
    }

    private record ExceptionExpect
    {
        public string FilterJson { get; set; }
        public FilterStatusCode StatusCode { get; set; }
        public string ErrorMessage { get; set; }
        public List<string> FailedKeyPath { get; set; }
    }

    // TODO: 覆盖所有异常
    [Test]
    public async Task ShouldMatchFailed()
    {
        ExceptionExpect[] expects = [
            new ()
            {
                FilterJson = "{\"userName\": {\"$regex\": \"^A\"}}",
                StatusCode = FilterStatusCode.NotMatched,
                ErrorMessage = "matchTarget Scott of type System.String not match {$regex: ^A}",
                FailedKeyPath = ["userName", "$regex"],
            },
            new ()
            {
                FilterJson = "{\"playerGameVersion\": {\"$eq\": 100}}",
                StatusCode = FilterStatusCode.Invalid,
                ErrorMessage = "filter 值 100 类型为 Number，期望类型为 String",
                FailedKeyPath = ["playerGameVersion", "$eq"],
            },
            new ()
            {
                FilterJson = "{\"playerGameVersion\": {\"wrong_key\": 100}}",
                StatusCode = FilterStatusCode.Invalid,
                ErrorMessage = "FilterFactory of type System.Version 包含无效子 filter wrong_key",
                FailedKeyPath = ["playerGameVersion", "wrong_key"],
            },
            new ()
            {
                FilterJson = "{\"$or\": [{\"userName\": {\"$regex\": \"^A\"}}, {\"playerGameVersion\": {\"$eq\": 100}}]}",
                StatusCode = FilterStatusCode.Invalid,
                ErrorMessage = "filter 值 100 类型为 Number，期望类型为 String",
                FailedKeyPath = ["$or", "playerGameVersion", "$eq"],
            },
            new ()
            {
                FilterJson = "{\"pid\": {\"$ne\": 1000}}",
                StatusCode = FilterStatusCode.NotMatched,
                ErrorMessage = "matchTarget 1000 of type System.Double not match {$ne: 1000}",
                FailedKeyPath = ["pid", "$ne"]
            },
            new ()
            {
                FilterJson = "{\"pid\": {\"$range\": [0, 1]}}",
                StatusCode = FilterStatusCode.NotMatched,
                ErrorMessage = "matchTarget 1000 of type System.Double not match {$range: [0,1]}",
                FailedKeyPath = ["pid", "$range"],
            },
            new ()
            {
                FilterJson = "{\"$or\": [{\"pid\": {\"$ne\": 1000}}, {\"pid\": {\"$range\": [0, 1]}}]}",
                StatusCode = FilterStatusCode.NotMatched,
                ErrorMessage = "no filters match $or",
                FailedKeyPath = ["$or", "pid", "$ne", "pid", "$range"],
            },
            new ()
            {
                FilterJson = "{\"$and\": [{\"pid\": {\"$ne\": 1000}}, {\"pid\": {\"$range\": [0, 10000]}}]}",
                StatusCode = FilterStatusCode.NotMatched,
                ErrorMessage = "matchTarget 1000 of type System.Double not match {$ne: 1000}",
                FailedKeyPath = ["$and", "pid", "$ne"],
            },
            new ()
            {
                FilterJson = "{\"pid\": {\"$range\": [1001, 1000]}}",
                StatusCode = FilterStatusCode.Invalid,
                ErrorMessage = "the second element of the range 1000 is not >= the first element 1001",
                FailedKeyPath = ["pid", "$range"],
            },
            new ()
            {
                FilterJson = "{\"userName\":{}}",
                StatusCode = FilterStatusCode.Invalid,
                ErrorMessage = "对象键值对数需要为 1，{} 有 0 对键值对",
                FailedKeyPath = ["userName"],
            },
            new ()
            {
                FilterJson = "{\"$and\":{}}",
                StatusCode = FilterStatusCode.Invalid,
                ErrorMessage = "filter 值 {} 类型为 Object，期望类型为 Array",
                FailedKeyPath = ["$and"],
            },
            new ()
            {
                FilterJson = "{\"userName\":123}",
                StatusCode = FilterStatusCode.Invalid,
                ErrorMessage = "filter 值 123 类型为 Number，期望类型为 String",
                FailedKeyPath = ["userName", "$eq"],
            },
            new ()
            {
                FilterJson = "{\"$and\":[]}",
                StatusCode = FilterStatusCode.Invalid,
                ErrorMessage = "[] 数组元素个数不能为0",
                FailedKeyPath = ["$and"],
            }
        ];

        foreach (var expect in expects)
        {
            var e = Assert.ThrowsAsync<FilterException>(() =>
                _filterService.LazyMustMatchAsync(expect.FilterJson, Player1Getter, null));
            PrintFilterException(e);
            Assert.Multiple(() =>
            {
                Assert.That(e.StatusCode, Is.EqualTo(expect.StatusCode));
                Assert.That(e.Message, Does.StartWith(expect.ErrorMessage));
                Assert.That(e.FailedKeyPath.Traverse().ToList(), Is.EqualTo(expect.FailedKeyPath));
            });

            e = Assert.Throws<FilterException>(() =>
            {
                _filterService.MustMatch(expect.FilterJson, Player1);
            });

            Assert.Multiple(() =>
            {
                Assert.That(e.StatusCode, Is.EqualTo(expect.StatusCode));
                Assert.That(e.Message, Does.StartWith(expect.ErrorMessage));
                Assert.That(e.FailedKeyPath.Traverse().ToList(), Is.EqualTo(expect.FailedKeyPath));
            });
            
            e = await _filterService.LazyMatchAsync(expect.FilterJson, Player1Getter, null);
            Assert.Multiple(() =>
            {
                Assert.That(e.StatusCode, Is.EqualTo(expect.StatusCode));
                Assert.That(e.Message, Does.StartWith(expect.ErrorMessage));
                Assert.That(e.FailedKeyPath.Traverse().ToList(), Is.EqualTo(expect.FailedKeyPath));
            });

            e = _filterService.Match(expect.FilterJson, Player1);
            Assert.Multiple(() =>
            {
                Assert.That(e.StatusCode, Is.EqualTo(expect.StatusCode));
                Assert.That(e.Message, Does.StartWith(expect.ErrorMessage));
                Assert.That(e.FailedKeyPath.Traverse().ToList(), Is.EqualTo(expect.FailedKeyPath));
            });
        }
    }

    [Test]
    public void ShouldFailedWhenMatchTargetNotFound()
    {
        var filterJson = "{\"pid\": {\"$in\": [1000, 1001]}}";
        
        var playerGetter = new IFilter<Player>.MatchTargetGetter(a =>
        {
            if (a == null)
            {
                return Task.FromResult<(Player, bool)>((null!, false));
            }

            var player = Players.FirstOrDefault(p => p.Pid == (int)a["pid"]);
            if (player == null)
            {
                return Task.FromResult<(Player, bool)>((null!, false));
            }

            return Task.FromResult((player, true));
        });

        Dictionary<string, object> args = new()
        {
            { "pid", 10000 }
        };

        var e = Assert.ThrowsAsync<FilterException>(() =>
            _filterService.LazyMustMatchAsync(filterJson, playerGetter, args));
        Assert.Multiple(() =>
        {
            Assert.That(e.StatusCode, Is.EqualTo(FilterStatusCode.MatchError));
            Assert.That(e.Message, Does.StartWith("matchTarget of type System.Double get failed, args: {\"pid\":10000}"));
            Assert.That(e.FailedKeyPath.Traverse().ToList(), Is.EqualTo(new List<string> { "pid", "$in" }));
        });
    }

    [Test]
    public void ShouldCacheFilterDocumentAfterFirstParse()
    {
        var filterJson = "{\"$and\": [{\"pid\": {\"$in\": [1000, 1001]}}, {\"userName\": {\"$eq\": \"Scott\"}}]}";
        _filterService.MustMatch(filterJson, Player1);
        Console.WriteLine($"player {Player1Json} match filter {filterJson} successfully");

        // FilterDocument will be cached after first parse
        filterJson = "{\"$and\": [{\"pid\": {\"$in\": [1000, 1001]}}, {\"userName\": {\"$eq\": \"Scott\"}}]}";
        _filterService.MustMatch(filterJson, Player1);
        Console.WriteLine($"player {Player1Json} match filter {filterJson} successfully");
    }

    [Test]
    public void ShouldFilterOutSuccessfully()
    {
        var filterJson = "{\"pid\": {\"$in\": [1000, 1001]}}";
        var filteredPlayers = _filterService.FilterOut(filterJson, Players).ToList();
        Assert.That(filteredPlayers, Has.Count.EqualTo(1));
        Assert.That(filteredPlayers, Has.All.Matches<Player>(p => p.Pid == 1000));
    }

    [Test]
    public async Task ShouldLazyFilterOutSuccessfully()
    {
        var filterJson = "{\"pid\": {\"$in\": [1000, 1001]}}";
        var playerGetter = new IFilter<Player>.MatchTargetGetter(a =>
        {
            if (a == null)
            {
                return Task.FromResult<(Player, bool)>((null!, false));
            }

            var player = Players.FirstOrDefault(p => p.Pid == (int)a["pid"]);
            if (player == null)
            {
                return Task.FromResult<(Player, bool)>((null!, false));
            }

            return Task.FromResult((player, true));
        });

        var args = new List<Dictionary<string, object>>
        {
            new()
            {
                { "pid", 1000 }
            },
            new()
            {
                { "pid", 1001 }
            }
        };

        var filteredPlayers = (await _filterService.LazyFilterOutAsync(filterJson, playerGetter, args)).ToList();
        Assert.That(filteredPlayers, Has.Count.EqualTo(1));
        Assert.That(filteredPlayers, Has.All.Matches<Player>(p => p.Pid == 1000));
        
        args =
        [
            new Dictionary<string, object>
            {
                { "pid", 10000 }
            },

            new Dictionary<string, object>
            {
                { "pid", 10010 }
            }
        ];
        filteredPlayers = (await _filterService.LazyFilterOutAsync(filterJson, playerGetter, args)).ToList();
        Assert.That(filteredPlayers, Has.Count.EqualTo(0));
    }

    [Test]
    public async Task ShouldJsonPathFilterMatchSuccessfully()
    {
        string[] filterJsons = [
            "{\"$.User.Address.Country\": \"CN\"}",
            "{\"$.User.Uid\": 1000000}",
            "{\"$.User.IsMale\": true}",
            "{\"$and\": [{\"$.User.IsMale\": true}, {\"$.User.Address.Street\": null}]}",
        ];

        foreach (var filterJson in filterJsons)
        {
            await _jsonPathFilterService.LazyMustMatchAsync(filterJson, Player1JObjectGetter, null);
            Console.WriteLine($"player {Player1Json} lazy must match filter {filterJson} successfully");

            _jsonPathFilterService.MustMatch(filterJson, Player1JObject);
            Console.WriteLine($"player {Player1Json} must match filter {filterJson} successfully");

            var filterException = await _jsonPathFilterService.LazyMatchAsync(filterJson, Player1JObjectGetter, null);
            Assert.That(filterException.StatusCode, Is.EqualTo(FilterStatusCode.Ok));
            Console.WriteLine($"player {Player1Json} lazy match filter {filterJson} successfully");

            filterException = _jsonPathFilterService.Match(filterJson, Player1JObject);
            Assert.That(filterException.StatusCode, Is.EqualTo(FilterStatusCode.Ok));
            Console.WriteLine($"player {Player1Json} match filter {filterJson} successfully");
        }
    }

    [Test]
    public async Task ShouldJsonPathFilterMatchFailed()
    {
        ExceptionExpect[] expects =
        [
            new()
            {
                FilterJson = "{\"$.User.Uid\": 5000}",
                StatusCode = FilterStatusCode.NotMatched,
                ErrorMessage = "matchTarget 1000000 of type System.Double not match {$eq: 5000}",
                FailedKeyPath = ["$.User.Uid", "$eq"],
            },
            new()
            {
                FilterJson = "{\"$.User.IsMale\": false}",
                StatusCode = FilterStatusCode.NotMatched,
                ErrorMessage = "matchTarget True of type System.Boolean not match {$eq: False}",
                FailedKeyPath = ["$.User.IsMale","$eq"],
            },
            new()
            {
                FilterJson = "{\"$.User.Uid\": \"1000000\"}",
                StatusCode = FilterStatusCode.MatchError,
                ErrorMessage = "JSONPath $.User.Uid value 1000000 expected string, got Integer",
                FailedKeyPath = ["$.User.Uid"],
            },
            new()
            {
                FilterJson = "{\"$.*\":\"test\"}",
                StatusCode = FilterStatusCode.MatchError,
                ErrorMessage = "JSONPath $.* matched 6 paths, but expected 1",
                FailedKeyPath = ["$.*"],
            },
            new()
            {
                FilterJson = "{\"$.NotFound.Address.Country\": \"CN\"}",
                StatusCode = FilterStatusCode.MatchError,
                ErrorMessage = "found no JSONPath $.NotFound.Address.Country in matchTarget",
                FailedKeyPath = ["$.NotFound.Address.Country"],
            },
            new()
            {
                FilterJson = "{\"$and\": [{\"$.User.IsMale\": true}, {\"$.User.Address.Country\": null}]}",
                StatusCode = FilterStatusCode.NotMatched,
                ErrorMessage = "JSONPath $.User.Address.Country value CN expected null, got String",
                FailedKeyPath = ["$and","$.User.Address.Country"],
            }
        ];

        foreach (var expect in expects)
        {
            var e = Assert.ThrowsAsync<FilterException>(() =>
                _jsonPathFilterService.LazyMustMatchAsync(expect.FilterJson, Player1JObjectGetter, null));
            PrintFilterException(e);
            Assert.Multiple(() =>
            {
                Assert.That(e.StatusCode, Is.EqualTo(expect.StatusCode));
                Assert.That(e.Message, Does.StartWith(expect.ErrorMessage));
                Assert.That(e.FailedKeyPath.Traverse().ToList(), Is.EqualTo(expect.FailedKeyPath));
            });
            
            e = Assert.Throws<FilterException>(() =>
            {
                _jsonPathFilterService.MustMatch(expect.FilterJson, Player1JObject);
            });
            
            Assert.Multiple(() =>
            {
                Assert.That(e.StatusCode, Is.EqualTo(expect.StatusCode));
                Assert.That(e.Message, Does.StartWith(expect.ErrorMessage));
                Assert.That(e.FailedKeyPath.Traverse().ToList(), Is.EqualTo(expect.FailedKeyPath));
            });
            
            e = await _jsonPathFilterService.LazyMatchAsync(expect.FilterJson, Player1JObjectGetter, null);
            Assert.Multiple(() =>
            {
                Assert.That(e.StatusCode, Is.EqualTo(expect.StatusCode));
                Assert.That(e.Message, Does.StartWith(expect.ErrorMessage));
                Assert.That(e.FailedKeyPath.Traverse().ToList(), Is.EqualTo(expect.FailedKeyPath));
            });
            
            e = _jsonPathFilterService.Match(expect.FilterJson, Player1JObject);
            Assert.Multiple(() =>
            {
                Assert.That(e.StatusCode, Is.EqualTo(expect.StatusCode));
                Assert.That(e.Message, Does.StartWith(expect.ErrorMessage));
                Assert.That(e.FailedKeyPath.Traverse().ToList(), Is.EqualTo(expect.FailedKeyPath));
            });
        }
    }

    private static void PrintFilterException(FilterException e)
    {
        Console.WriteLine("*******************************************************************");
        Console.WriteLine(e.StatusCode);
        Console.WriteLine(e.Message);
        Console.WriteLine(JsonSerializer.Serialize(e.FailedKeyPath.Traverse().ToList()));
    }
}