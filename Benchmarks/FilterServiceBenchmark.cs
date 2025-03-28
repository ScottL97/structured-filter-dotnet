﻿using BenchmarkDotNet.Attributes;
using SceneFilterModelsExample.Models;
using SceneFilterModelsExample.Scenes;
using StructuredFilter;

namespace Benchmarks;

[MemoryDiagnoser]
public class FilterServiceBenchmark
{
    private readonly FilterService<Player> _filterServiceWithCache = new FilterService<Player>().WithSceneFilters([
        f => new PidFilter(f),
        f => new UserNameFilter(f),
        f => new PlayerGameVersionFilter(f)
    ]);
    
    private readonly FilterService<Player> _filterServiceWithCacheAndFilterCache = new FilterService<Player>().WithSceneFilters([
        f => new SceneFilterModelsExample.Scenes.CacheableScenes.PidFilter(f),
        f => new UserNameFilter(f),
        f => new PlayerGameVersionFilter(f)
    ]);

    private readonly FilterService<Player> _filterServiceWithoutCache = new FilterService<Player>(option: new FilterOption<Player>
    {
        EnableFilterDocumentCache = false
    }).WithSceneFilters([
        f => new PidFilter(f),
        f => new UserNameFilter(f),
        f => new PlayerGameVersionFilter(f)
    ]);

    private readonly string[] _rawFilters = [
        "{\"pid\": {\"$in\": [1000, 1001]}}",
        "{\"$and\": [{\"pid\": {\"$in\": [1000, 1001]}}, {\"userName\": {\"$eq\": \"Scott\"}}]}",
        "{\"$and\": [{\"userName\": {\"$eq\": \"Scott\"}}, {\"playerGameVersion\": {\"$le\": \"1.1.0\"}}]}",
        "{\"$and\": [{\"playerGameVersion\": {\"$lt\": \"2.0.0\"}}, {\"playerGameVersion\": {\"$gt\": \"0.9.0\"}}]}",
        "{\"playerGameVersion\": {\"$range\": [\"1.0.0\", \"2.0.0\"]}}",
        "{\"$or\": [{\"pid\": {\"$lt\": 10000}}, {\"userName\": {\"$in\": [\"Alice\", \"Bob\"]}}]}",
        "{\"userName\": \"Scott\"}",
        "{\"userName\": {\"$regex\": \"^S\"}}",
        "{\"playerGameVersion\": {\"$eq\": 100}}",
        "{\"pid\": {\"$ne\": 1000}}"
    ];

    private readonly Player _player = new ()
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

    [Benchmark]
    public async Task FilterServiceWithCache()
    {
        foreach (var rawFilter in _rawFilters)
        {
            await _filterServiceWithCache.MatchAsync(rawFilter, _player);
        }
    }

    [Benchmark]
    public async Task FilterServiceWithCacheAndFilterCache()
    {
        foreach (var rawFilter in _rawFilters)
        {
            await _filterServiceWithCacheAndFilterCache.MatchAsync(rawFilter, _player);
        }
    }

    [Benchmark]
    public async Task FilterServiceWithoutCache()
    {
        foreach (var rawFilter in _rawFilters)
        {
            await _filterServiceWithoutCache.MatchAsync(rawFilter, _player);
        }
    }

    [Benchmark]
    public async Task FilterServiceOneMatchOkWithCache()
    {
        await _filterServiceWithCache.MatchAsync("{\"pid\": {\"$in\": [1000, 1001]}}", _player);
    }

    [Benchmark]
    public async Task FilterServiceOneMatchAndOkWithCache()
    {
        await _filterServiceWithCache.MatchAsync("{\"$and\": [{\"userName\": {\"$eq\": \"Scott\"}}, {\"playerGameVersion\": {\"$le\": \"1.1.0\"}}]}", _player);
    }

    [Benchmark]
    public async Task FilterServiceOneMatchFailedWithCache()
    {
        await _filterServiceWithCache.MatchAsync("{\"pid\": {\"$in\": [1001, 1002]}}", _player);
    }

    [Benchmark]
    public async Task FilterServiceOneMatchAndFailedWithCache()
    {
        await _filterServiceWithCache.MatchAsync("{\"$and\": [{\"userName\": {\"$eq\": \"Scott\"}}, {\"playerGameVersion\": {\"$ge\": \"1.1.0\"}}]}", _player);
    }
}
