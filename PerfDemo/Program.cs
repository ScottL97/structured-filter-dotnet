using System;
using SceneFilterModelsExample.Models;
using SceneFilterModelsExample.Scenes;
using StructuredFilter;

var filterService = new FilterService<Player>().WithSceneFilters([
    f => new PidFilter(f),
    f => new UserNameFilter(f),
    f => new PlayerGameVersionFilter(f)
]);

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

while (true)
{
    await filterService.MatchAsync("{\"$and\": [{\"userName\": {\"$eq\": \"Scott\"}}, {\"playerGameVersion\": {\"$le\": \"1.1.0\"}}]}", player);
}
