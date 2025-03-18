using System.Text.Json;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using StructuredFilter.Filters;
using StructuredFilter.Filters.Common;
using StructuredFilter.Filters.Common.FilterTypes;
using StructuredFilter.Filters.SceneFilters;
using StructuredFilter.Test.Models;
using StructuredFilter.Test.Scenes;
using StructuredFilter.Test.Scenes.CacheableScenes;
using StructuredFilter.Utils;
using Exception = System.Exception;
using PidFilter = StructuredFilter.Test.Scenes.PidFilter;

namespace StructuredFilter.Test;

public class StructuredFilterTests
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
    private static readonly LazyObjectGetter<Player> Player1Getter = new (_ => Task.FromResult((Player1, true)), null);
    private static readonly JObject Player1JObject = Newtonsoft.Json.JsonConvert.DeserializeObject<JObject>(Player1Json)!;
    private static readonly LazyObjectGetter<JObject> Player1JObjectGetter = new (_ => Task.FromResult((Player1JObject, true)), null);

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
    private static readonly LazyObjectGetter<Player> Player2Getter = new (_ => Task.FromResult((Player2, true)), null);
    private static readonly JObject Player2JObject = Newtonsoft.Json.JsonConvert.DeserializeObject<JObject>(Player2Json)!;
    private static readonly LazyObjectGetter<JObject> Player2JObjectGetter = new (_ => Task.FromResult((Player2JObject, true)), null);

    private static readonly List<Player> Players = [Player1, Player2];
    private static readonly List<LazyObjectGetter<Player>> PlayersGetter = [Player1Getter, Player2Getter];

    private readonly FilterService<Player> _filterService = new FilterService<Player>().WithSceneFilters([
        f => new PidFilter(f),
        f => new UserNameFilter(f),
        f => new PlayerGameVersionFilter(f)
    ]);

    private readonly FilterService<Player> _cacheableFilterService = new FilterService<Player>().WithSceneFilters([
        f => new Scenes.CacheableScenes.PidFilter(f),
        f => new UserNameFilter(f),
        f => new PlayerGameVersionFilter(f)
    ]);

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

    private static readonly Regex WhitespaceRegex = new (@"\s");

    [Test]
    public void GetSceneFilterInfosTest()
    {
        var sceneFilterInfos = JsonSerializer.Serialize(_filterService.GetSceneFilterInfos(), new JsonSerializerOptions
        {
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = true
        });

        Console.WriteLine(sceneFilterInfos);
        Assert.That(WhitespaceRegex.Replace(sceneFilterInfos, ""), Is.EqualTo("{\"pid\":{\"label\":\"玩家ID\",\"logics\":[{\"label\":\"属于\",\"value\":\"$in\"},{\"label\":\"不等于\",\"value\":\"$ne\"},{\"label\":\"等于\",\"value\":\"$eq\"},{\"label\":\"大于\",\"value\":\"$gt\"},{\"label\":\"在此范围（包含两端值）\",\"value\":\"$range\"},{\"label\":\"小于等于\",\"value\":\"$le\"},{\"label\":\"大于等于\",\"value\":\"$ge\"},{\"label\":\"小于\",\"value\":\"$lt\"}],\"type\":\"NUMBER\"},\"userName\":{\"label\":\"用户名\",\"logics\":[{\"label\":\"等于\",\"value\":\"$eq\"},{\"label\":\"不等于\",\"value\":\"$ne\"},{\"label\":\"属于\",\"value\":\"$in\"},{\"label\":\"匹配正则表达式\",\"value\":\"$regex\"},{\"label\":\"在此范围（包含两端值）\",\"value\":\"$range\"}],\"type\":\"STRING\"},\"playerGameVersion\":{\"label\":\"玩家游戏版本\",\"logics\":[{\"label\":\"属于\",\"value\":\"$in\"},{\"label\":\"不等于\",\"value\":\"$ne\"},{\"label\":\"等于\",\"value\":\"$eq\"},{\"label\":\"大于\",\"value\":\"$gt\"},{\"label\":\"在此范围（包含两端值）\",\"value\":\"$range\"},{\"label\":\"小于等于\",\"value\":\"$le\"},{\"label\":\"大于等于\",\"value\":\"$ge\"},{\"label\":\"小于\",\"value\":\"$lt\"}],\"type\":\"VERSION\"}}"));
    }

    [Test]
    public async Task ShouldMatchSuccessfully()
    {
        string[] filterJsons = [
            "{\"pid\": {\"$in\": [1000, 1001]}}",
            "{\"$and\": [{\"pid\": {\"$in\": [1000, 1001]}}, {\"userName\": {\"$eq\": \"Scott\"}}]}",
            "{\"$and\": [{\"userName\": {\"$eq\": \"Scott\"}}, {\"playerGameVersion\": {\"$le\": \"1.1.0\"}}]}",
            "{\"$and\": [{\"playerGameVersion\": {\"$lt\": \"2.0.0\"}}, {\"playerGameVersion\": {\"$gt\": \"0.9.0\"}}]}",
            "{\"playerGameVersion\": {\"$range\": [\"1.0.0\", \"2.0.0\"]}}",
            "{\"$or\": [{\"pid\": {\"$lt\": 10000}}, {\"userName\": {\"$in\": [\"Alice\", \"Bob\"]}}]}",
            "{\"$or\": [{\"pid\": {\"$ne\": 1000}}, {\"pid\": {\"$range\": [0, 10000]}}]}",
            "{\"userName\": \"Scott\"}",
            "{\"userName\": {\"$regex\": \"^S\"}}"
        ];

        await AssertPlayer1MatchSuccessfully(filterJsons, _filterService);
    }

    private record ExceptionExpect
    {
        public string FilterJson { get; set; }
        public FilterStatusCode StatusCode { get; set; }
        public string ErrorMessage { get; set; }
        public string? ErrorMessage2 { get; set; } = null;
        public List<string> FailedKeyPath { get; set; }
        public List<string>? FailedKeyPath2 { get; set; } = null;
    }

    // TODO: 覆盖所有异常
    [Test]
    public async Task ShouldMatchFailed()
    {
        ExceptionExpect[] expects = [
            new ()
            {
                FilterJson = string.Empty,
                StatusCode = FilterStatusCode.Invalid,
                ErrorMessage = "Filter cannot be empty",
                FailedKeyPath = [],
            },
            new ()
            {
                FilterJson = "test",
                StatusCode = FilterStatusCode.Invalid,
                ErrorMessage = "Filter 不是有效的 JSON",
                FailedKeyPath = [],
            },
            new ()
            {
                FilterJson = "{",
                StatusCode = FilterStatusCode.Invalid,
                ErrorMessage = "Filter 不是有效的 JSON",
                FailedKeyPath = [],
            },
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
                ErrorMessage = "FilterFactory of type System.Version 子 filter wrong_key 不存在",
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
                FilterJson = "[]",
                StatusCode = FilterStatusCode.Invalid,
                ErrorMessage = "无效的 filter 根节点类型：Array",
                FailedKeyPath = ["<UNKNOWN>"],
            },
            new ()
            {
                FilterJson = "{}",
                StatusCode = FilterStatusCode.Invalid,
                ErrorMessage = "对象键值对数需要为 1，但 filter 根节点对象 {} 有 0 对键值对",
                FailedKeyPath = ["<UNKNOWN>"],
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
                ErrorMessage = "FilterFactory of type StructuredFilter.Test.Models.Player 子 filter $and 不存在",
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

        await AssertPlayer1ExpectExceptions(expects, _filterService);
    }

    [Test]
    public void ShouldFailedWhenMatchTargetNotFound()
    {
        var filterJson = "{\"pid\": {\"$in\": [1000, 1001]}}";

        Dictionary<string, object> args = new()
        {
            { "pid", 10000 }
        };

        var playerGetter = new LazyObjectGetter<Player>(a =>
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
        }, args);

        var e = Assert.ThrowsAsync<FilterException>(() =>
            _filterService.LazyMustMatchAsync(filterJson, playerGetter));
        Assert.Multiple(() =>
        {
            Assert.That(e.StatusCode, Is.EqualTo(FilterStatusCode.MatchError));
            Assert.That(e.Message, Does.StartWith("matchTarget of type System.Double get failed, args: {\"pid\":10000}"));
            Assert.That(e.FailedKeyPath.Traverse().ToList(), Is.EqualTo(new List<string> { "pid", "$in" }));
        });
    }

    [Test]
    public async Task ShouldCacheableFiltersCacheFilterResultAfterFirstParse()
    {
        var filterJson = "{\"pid\": {\"$in\": [1000, 1001]}}";
        await _cacheableFilterService.MustMatchAsync(filterJson, Player1);
        Console.WriteLine($"player {Player1Json} match filter {filterJson} successfully");

        // filter result will be cached after first parse
        filterJson = "{\"pid\": {\"$in\": [1000, 1001]}}";
        await _cacheableFilterService.MustMatchAsync(filterJson, Player1);
        Console.WriteLine($"player {Player1Json} match filter {filterJson} successfully");

        filterJson = "{\"pid\": {\"$in\": [1001, 1002]}}";
        var e = Assert.ThrowsAsync<FilterException>(async () =>
        {
            await _cacheableFilterService.MustMatchAsync(filterJson, Player1);
        });
        Assert.Multiple(() =>
        {
            Assert.That(e.StatusCode, Is.EqualTo(FilterStatusCode.NotMatched));
            Assert.That(e.Message, Does.StartWith("matchTarget 1000 of type System.Double not match {$in: [1001,1002]}"));
            Assert.That(e.FailedKeyPath.Traverse().ToList(), Is.EqualTo(new List<string> { "pid", "$in" }));
        });

        // filter result will be cached after first parse
        filterJson = "{\"pid\": {\"$in\": [1001, 1002]}}";
        e = Assert.ThrowsAsync<FilterException>(async () =>
        {
            await _cacheableFilterService.MustMatchAsync(filterJson, Player1);
        });
        Assert.Multiple(() =>
        {
            Assert.That(e.StatusCode, Is.EqualTo(FilterStatusCode.NotMatched));
            Assert.That(e.Message, Does.StartWith("matchTarget StructuredFilter.Test.Models.Player of type StructuredFilter.Test.Models.Player not match {pid: FilterKv { Key = $in, Value = [1001,1002] }} according to cache"));
            Assert.That(e.FailedKeyPath.Traverse().ToList(), Is.EqualTo(new List<string> { "pid" }));
        });
    }

    [Test]
    public async Task ShouldFilterOutSuccessfully()
    {
        var filterJson = "{\"pid\": {\"$in\": [1000, 1001]}}";
        var filteredPlayers = (await _filterService.FilterOutAsync(filterJson, Players)).ToList();
        Assert.That(filteredPlayers, Has.Count.EqualTo(1));
        Assert.That(filteredPlayers, Has.All.Matches<Player>(p => p.Pid == 1000));
    }

    [Test]
    public async Task ShouldLazyFilterOutSuccessfully()
    {
        var filterJson = "{\"pid\": {\"$in\": [1000, 1001]}}";
        LazyObjectGetter<Player, string, object>.ObjectGetter getter = a =>
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
        };

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

        var playerGetters = args.Select(a => new LazyObjectGetter<Player>(getter, a)).ToList();

        var filteredPlayers = (await _filterService.LazyFilterOutAsync(filterJson, playerGetters)).ToList();
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
        playerGetters = args.Select(a => new LazyObjectGetter<Player>(getter, a)).ToList();

        filteredPlayers = (await _filterService.LazyFilterOutAsync(filterJson, playerGetters)).ToList();
        Assert.That(filteredPlayers, Has.Count.EqualTo(0));
    }

    [Test]
    public async Task ShouldCacheableDynamicFiltersMatchSuccessfully()
    {
        var cache = new PlayerFilterCache();
        var testCacheableDynamicFilterService = await new FilterService<Player>(new FilterOption<Player>
        {
            DynamicFiltersGetter = () => Task.FromResult(new DynamicFilter<Player>[]
            {
                new ("rank", FilterBasicType.Number, true, "玩家等级", cache)
            }),
            DynamicNumberSceneFilterValueGetter = (player, filterKey) =>
            {
                if (filterKey == "rank")
                {
                    return Task.FromResult((double)10);
                }

                throw new Exception($"player dynamic key {filterKey} not found");
            }
        }).WithSceneFilters([
            f => new PidFilter(f),
            f => new UserNameFilter(f),
            f => new PlayerGameVersionFilter(f)
        ]).LoadDynamicSceneFilters();

        string[] filterJsons =
        [
            "{\"rank\": {\"$range\": [0, 50]}}",
            "{\"rank\": {\"$range\": [0, 50]}}"
        ];

        await AssertPlayer1MatchSuccessfully(filterJsons, testCacheableDynamicFilterService);

        Assert.That(cache.HitCount, Is.EqualTo(filterJsons.Length * 4 - 1));
    }

    private static async Task AssertPlayer1MatchSuccessfully(string[] filterJsons, FilterService<Player> filterService)
    {
        foreach (var filterJson in filterJsons)
        {
            await filterService.LazyMustMatchAsync(filterJson, Player1Getter);
            Console.WriteLine($"player {Player1Json} lazy must match filter {filterJson} successfully");

            await filterService.MustMatchAsync(filterJson, Player1);
            Console.WriteLine($"player {Player1Json} must match filter {filterJson} successfully");

            var filterException = await filterService.LazyMatchAsync(filterJson, Player1Getter);
            Assert.That(filterException.StatusCode, Is.EqualTo(FilterStatusCode.Ok));
            Console.WriteLine($"player {Player1Json} lazy match filter {filterJson} successfully");

            filterException = await filterService.MatchAsync(filterJson, Player1);
            Assert.That(filterException.StatusCode, Is.EqualTo(FilterStatusCode.Ok));
            Console.WriteLine($"player {Player1Json} match filter {filterJson} successfully");
        }
    }

    [Test]
    public async Task ShouldCacheableDynamicFiltersMatchFailed()
    {
        var cache = new PlayerFilterCache();
        var testCacheableDynamicFilterService = await new FilterService<Player>(new FilterOption<Player>
        {
            DynamicFiltersGetter = () => Task.FromResult(new DynamicFilter<Player>[]
            {
                new ("rank", FilterBasicType.Number, true, "玩家等级", cache)
            }),
            DynamicNumberSceneFilterValueGetter = (player, filterKey) =>
            {
                if (filterKey == "rank")
                {
                    return Task.FromResult((double)10);
                }

                throw new Exception($"player dynamic key {filterKey} not found");
            }
        }).WithSceneFilters([
            f => new PidFilter(f),
            f => new UserNameFilter(f),
            f => new PlayerGameVersionFilter(f)
        ]).LoadDynamicSceneFilters();

        ExceptionExpect[] expects =
        [
            new()
            {
                FilterJson = "{\"rank\": {\"$regex\": \"^A\"}}",
                StatusCode = FilterStatusCode.Invalid,
                ErrorMessage = "FilterFactory of type System.Double 子 filter $regex 不存在",
                FailedKeyPath = ["rank", "$regex"],
            },
            new()
            {
                FilterJson = "{\"rank\": {\"$regex\": \"^A\"}}",
                StatusCode = FilterStatusCode.Invalid,
                ErrorMessage = "FilterFactory of type System.Double 子 filter $regex 不存在",
                FailedKeyPath = ["rank", "$regex"],
            },
            new()
            {
                FilterJson = "{\"rank\": {\"$range\": [50, 100]}}",
                StatusCode = FilterStatusCode.NotMatched,
                ErrorMessage = "matchTarget 10 of type System.Double not match {$range: [50,100]}",
                ErrorMessage2 = "matchTarget StructuredFilter.Test.Models.Player of type StructuredFilter.Test.Models.Player not match {rank: FilterKv { Key = $range, Value = [50,100] }} according to cache",
                FailedKeyPath = ["rank", "$range"],
                FailedKeyPath2 = ["rank"],
            },
            new()
            {
                FilterJson = "{\"rank\": {\"$range\": [50, 100]}}",
                StatusCode = FilterStatusCode.NotMatched,
                ErrorMessage = "matchTarget StructuredFilter.Test.Models.Player of type StructuredFilter.Test.Models.Player not match {rank: FilterKv { Key = $range, Value = [50,100] }} according to cache",
                FailedKeyPath = ["rank", "$range"],
                FailedKeyPath2 = ["rank"],
            }
        ];

        await AssertPlayer1ExpectExceptions(expects, testCacheableDynamicFilterService);

        Assert.That(cache.HitCount, Is.EqualTo(expects.Count(e => e.StatusCode == FilterStatusCode.NotMatched) * 4 - 1));
    }

    private static async Task AssertPlayer1ExpectExceptions(ExceptionExpect[] expects,
        FilterService<Player> filterService)
    {
        foreach (var expect in expects)
        {
            var e = Assert.ThrowsAsync<FilterException>(() =>
                filterService.LazyMustMatchAsync(expect.FilterJson, Player1Getter));
            PrintFilterException(e);
            Assert.Multiple(() =>
            {
                Assert.That(e.StatusCode, Is.EqualTo(expect.StatusCode));
                Assert.That(e.Message,
                    expect.ErrorMessage2 != null
                        ? Does.StartWith(expect.ErrorMessage).Or.StartsWith(expect.ErrorMessage2)
                        : Does.StartWith(expect.ErrorMessage));
                Assert.That(e.FailedKeyPath.Traverse().ToList(),
                    expect.FailedKeyPath2 != null
                        ? Is.EqualTo(expect.FailedKeyPath).Or.EqualTo(expect.FailedKeyPath2)
                        : Is.EqualTo(expect.FailedKeyPath));
            });

            e = Assert.ThrowsAsync<FilterException>(async () =>
            {
                await filterService.MustMatchAsync(expect.FilterJson, Player1);
            });

            Assert.Multiple(() =>
            {
                Assert.That(e.StatusCode, Is.EqualTo(expect.StatusCode));
                Assert.That(e.Message,
                    expect.ErrorMessage2 != null
                        ? Does.StartWith(expect.ErrorMessage).Or.StartsWith(expect.ErrorMessage2)
                        : Does.StartWith(expect.ErrorMessage));
                Assert.That(e.FailedKeyPath.Traverse().ToList(),
                    expect.FailedKeyPath2 != null
                        ? Is.EqualTo(expect.FailedKeyPath).Or.EqualTo(expect.FailedKeyPath2)
                        : Is.EqualTo(expect.FailedKeyPath));
            });

            e = await filterService.LazyMatchAsync(expect.FilterJson, Player1Getter);
            Assert.Multiple(() =>
            {
                Assert.That(e.StatusCode, Is.EqualTo(expect.StatusCode));
                Assert.That(e.Message,
                    expect.ErrorMessage2 != null
                        ? Does.StartWith(expect.ErrorMessage).Or.StartsWith(expect.ErrorMessage2)
                        : Does.StartWith(expect.ErrorMessage));
                Assert.That(e.FailedKeyPath.Traverse().ToList(),
                    expect.FailedKeyPath2 != null
                        ? Is.EqualTo(expect.FailedKeyPath).Or.EqualTo(expect.FailedKeyPath2)
                        : Is.EqualTo(expect.FailedKeyPath));
            });

            e = await filterService.MatchAsync(expect.FilterJson, Player1);
            Assert.Multiple(() =>
            {
                Assert.That(e.StatusCode, Is.EqualTo(expect.StatusCode));
                Assert.That(e.Message,
                    expect.ErrorMessage2 != null
                        ? Does.StartWith(expect.ErrorMessage).Or.StartsWith(expect.ErrorMessage2)
                        : Does.StartWith(expect.ErrorMessage));
                Assert.That(e.FailedKeyPath.Traverse().ToList(),
                    expect.FailedKeyPath2 != null
                        ? Is.EqualTo(expect.FailedKeyPath).Or.EqualTo(expect.FailedKeyPath2)
                        : Is.EqualTo(expect.FailedKeyPath));
            });
        }
    }

    [Test]
    public async Task ShouldDynamicFiltersMatchSuccessfully()
    {
        var testDynamicFilterService = await new FilterService<Player>(new FilterOption<Player>
        {
            DynamicFiltersGetter = () => Task.FromResult(new DynamicFilter<Player>[]
            {
                new ("rank", FilterBasicType.Number, Label: "玩家等级")
            }),
            DynamicNumberSceneFilterValueGetter = (player, filterKey) =>
            {
                if (filterKey == "rank")
                {
                    return Task.FromResult((double)10);
                }

                throw new Exception($"player dynamic key {filterKey} not found");
            }
        }).WithSceneFilters([
            f => new PidFilter(f),
            f => new UserNameFilter(f),
            f => new PlayerGameVersionFilter(f)
        ]).LoadDynamicSceneFilters();

        var sceneFilterInfos = JsonSerializer.Serialize(testDynamicFilterService.GetSceneFilterInfos(), new JsonSerializerOptions
        {
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = true
        });

        Console.WriteLine(WhitespaceRegex.Replace(sceneFilterInfos, ""));
        Assert.That(WhitespaceRegex.Replace(sceneFilterInfos, ""), Is.EqualTo("{\"pid\":{\"label\":\"玩家ID\",\"logics\":[{\"label\":\"属于\",\"value\":\"$in\"},{\"label\":\"不等于\",\"value\":\"$ne\"},{\"label\":\"等于\",\"value\":\"$eq\"},{\"label\":\"大于\",\"value\":\"$gt\"},{\"label\":\"在此范围（包含两端值）\",\"value\":\"$range\"},{\"label\":\"小于等于\",\"value\":\"$le\"},{\"label\":\"大于等于\",\"value\":\"$ge\"},{\"label\":\"小于\",\"value\":\"$lt\"}],\"type\":\"NUMBER\"},\"userName\":{\"label\":\"用户名\",\"logics\":[{\"label\":\"等于\",\"value\":\"$eq\"},{\"label\":\"不等于\",\"value\":\"$ne\"},{\"label\":\"属于\",\"value\":\"$in\"},{\"label\":\"匹配正则表达式\",\"value\":\"$regex\"},{\"label\":\"在此范围（包含两端值）\",\"value\":\"$range\"}],\"type\":\"STRING\"},\"playerGameVersion\":{\"label\":\"玩家游戏版本\",\"logics\":[{\"label\":\"属于\",\"value\":\"$in\"},{\"label\":\"不等于\",\"value\":\"$ne\"},{\"label\":\"等于\",\"value\":\"$eq\"},{\"label\":\"大于\",\"value\":\"$gt\"},{\"label\":\"在此范围（包含两端值）\",\"value\":\"$range\"},{\"label\":\"小于等于\",\"value\":\"$le\"},{\"label\":\"大于等于\",\"value\":\"$ge\"},{\"label\":\"小于\",\"value\":\"$lt\"}],\"type\":\"VERSION\"},\"rank\":{\"label\":\"玩家等级\",\"logics\":[{\"label\":\"属于\",\"value\":\"$in\"},{\"label\":\"不等于\",\"value\":\"$ne\"},{\"label\":\"等于\",\"value\":\"$eq\"},{\"label\":\"大于\",\"value\":\"$gt\"},{\"label\":\"在此范围（包含两端值）\",\"value\":\"$range\"},{\"label\":\"小于等于\",\"value\":\"$le\"},{\"label\":\"大于等于\",\"value\":\"$ge\"},{\"label\":\"小于\",\"value\":\"$lt\"}],\"type\":\"NUMBER\"}}"));

        string[] filterJsons =
        [
            "{\"rank\": {\"$range\": [0, 50]}}",
            "{\"rank\": {\"$ne\": 100}}",
            "{\"$and\": [{\"pid\": {\"$in\": [1000, 1001]}}, {\"rank\": {\"$eq\": 10}}]}"
        ];

        await AssertPlayer1MatchSuccessfully(filterJsons, testDynamicFilterService);
    }

    [Test]
    public async Task ShouldDynamicFiltersMatchFailed()
    {
        var testDynamicFilterService = await new FilterService<Player>(new FilterOption<Player>
        {
            DynamicFiltersGetter = () => Task.FromResult(new DynamicFilter<Player>[]
            {
                new ("rank", FilterBasicType.Number, Label: "玩家等级")
            }),
            DynamicNumberSceneFilterValueGetter = (player, filterKey) =>
            {
                if (filterKey == "rank")
                {
                    return Task.FromResult((double)10);
                }

                throw new Exception($"player dynamic key {filterKey} not found");
            }
        }).WithSceneFilters([
            f => new PidFilter(f),
            f => new UserNameFilter(f),
            f => new PlayerGameVersionFilter(f)
        ]).LoadDynamicSceneFilters();

        ExceptionExpect[] expects =
        [
            new()
            {
                FilterJson = "{\"rank\": {\"$gt\": 20}}",
                StatusCode = FilterStatusCode.NotMatched,
                ErrorMessage = "matchTarget 10 of type System.Double not match {$gt: 20}",
                FailedKeyPath = ["rank", "$gt"],
            },
            new()
            {
                FilterJson = "{\"rank\": {\"$eq\": \"100\"}}",
                StatusCode = FilterStatusCode.Invalid,
                ErrorMessage = "filter 值 100 类型为 String，期望类型为 Number",
                FailedKeyPath = ["rank", "$eq"],
            },
            new()
            {
                FilterJson = "{\"rank\": {\"wrong_key\": 100}}",
                StatusCode = FilterStatusCode.Invalid,
                ErrorMessage = "FilterFactory of type System.Double 子 filter wrong_key 不存在",
                FailedKeyPath = ["rank", "wrong_key"],
            }
        ];
        
        await AssertPlayer1ExpectExceptions(expects, testDynamicFilterService);
    }

    [Test]
    public void TestFilterNormalizer()
    {
        Assert.That(FilterNormalizer.Normalize("{\"pid\": 5000}"),
            Is.EqualTo("{\"pid\":{\"$eq\":5000}}"));

        Assert.That(FilterNormalizer.Normalize("{\"$and\":[{\"pid\": 5000},{\"name\":\"Tom\"}]}"),
            Is.EqualTo("{\"$and\":[{\"pid\":{\"$eq\":5000}},{\"name\":{\"$eq\":\"Tom\"}}]}"));
    }

    [Test]
    public void TestFilterValidator()
    {
        var filterFactory = new FilterFactory<Player>();
        filterFactory = filterFactory.WithSceneFilter(new PidFilter(filterFactory));

        FilterValidator.MustValid("{\"pid\": 5000}", filterFactory);
        _filterService.MustValidFilter("{\"pid\": 5000}");
        FilterValidator.MustValid("{\"pid\": {\"$ne\": 2000}}", filterFactory);
        _filterService.MustValidFilter("{\"pid\": {\"$ne\": 2000}}");

        var e = Assert.Throws<FilterException>(() =>
            FilterValidator.MustValid("", filterFactory));
        Assert.Multiple(() =>
        {
            Assert.That(e.StatusCode, Is.EqualTo(FilterStatusCode.Invalid));
            Assert.That(e.Message, Does.StartWith("Filter cannot be empty"));
            Assert.That(e.FailedKeyPath.Traverse().ToList(), Has.Count.EqualTo(0));
        });

        e = Assert.Throws<FilterException>(() =>
            _filterService.MustValidFilter(""));
        Assert.Multiple(() =>
        {
            Assert.That(e.StatusCode, Is.EqualTo(FilterStatusCode.Invalid));
            Assert.That(e.Message, Does.StartWith("Filter cannot be empty"));
            Assert.That(e.FailedKeyPath.Traverse().ToList(), Has.Count.EqualTo(0));
        });

        e = Assert.Throws<FilterException>(() =>
            FilterValidator.MustValid("{\"pid\": \"5000\"}", filterFactory));
        Assert.Multiple(() =>
        {
            Assert.That(e.StatusCode, Is.EqualTo(FilterStatusCode.Invalid));
            Assert.That(e.Message, Does.StartWith("filter 值 5000 类型为 String，期望类型为 Number"));
            Assert.That(e.FailedKeyPath.Traverse().ToList(), Is.EqualTo(new List<string> { "pid", "$eq" }));
        });
        
        e = Assert.Throws<FilterException>(() =>
            _filterService.MustValidFilter("{\"pid\": \"5000\"}"));
        Assert.Multiple(() =>
        {
            Assert.That(e.StatusCode, Is.EqualTo(FilterStatusCode.Invalid));
            Assert.That(e.Message, Does.StartWith("filter 值 5000 类型为 String，期望类型为 Number"));
            Assert.That(e.FailedKeyPath.Traverse().ToList(), Is.EqualTo(new List<string> { "pid", "$eq" }));
        });

        e = Assert.Throws<FilterException>(() =>
            FilterValidator.MustValid("{\"pid\": \"5000\"", filterFactory));
        Assert.Multiple(() =>
        {
            Assert.That(e.StatusCode, Is.EqualTo(FilterStatusCode.Invalid));
            Assert.That(e.Message, Does.StartWith("Filter 不是有效的 JSON"));
            Assert.That(e.FailedKeyPath.Traverse().ToList(), Has.Count.EqualTo(0));
        });
        
        e = Assert.Throws<FilterException>(() =>
            _filterService.MustValidFilter("{\"pid\": \"5000\""));
        Assert.Multiple(() =>
        {
            Assert.That(e.StatusCode, Is.EqualTo(FilterStatusCode.Invalid));
            Assert.That(e.Message, Does.StartWith("Filter 不是有效的 JSON"));
            Assert.That(e.FailedKeyPath.Traverse().ToList(), Has.Count.EqualTo(0));
        });

        e = Assert.Throws<FilterException>(() =>
            FilterValidator.MustValid("{\"not_found\": 1}", filterFactory));
        Assert.Multiple(() =>
        {
            Assert.That(e.StatusCode, Is.EqualTo(FilterStatusCode.Invalid));
            Assert.That(e.Message, Does.StartWith("FilterFactory of type StructuredFilter.Test.Models.Player 子 filter not_found 不存在"));
            Assert.That(e.FailedKeyPath.Traverse().ToList(), Is.EqualTo(new List<string> { "not_found" }));
        });
        
        e = Assert.Throws<FilterException>(() =>
            _filterService.MustValidFilter("{\"not_found\": 1}"));
        Assert.Multiple(() =>
        {
            Assert.That(e.StatusCode, Is.EqualTo(FilterStatusCode.Invalid));
            Assert.That(e.Message, Does.StartWith("FilterFactory of type StructuredFilter.Test.Models.Player 子 filter not_found 不存在"));
            Assert.That(e.FailedKeyPath.Traverse().ToList(), Is.EqualTo(new List<string> { "not_found" }));
        });
    }

    private static void PrintFilterException(FilterException e)
    {
        Console.WriteLine("*******************************************************************");
        Console.WriteLine(e.StatusCode);
        Console.WriteLine(e.Message);
        Console.WriteLine(JsonSerializer.Serialize(e.FailedKeyPath.Traverse().ToList()));
    }
}
