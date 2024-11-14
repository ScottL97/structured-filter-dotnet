using StructuredFilter;
using StructuredFilter.AspNetCore;
using StructuredFilter.AspNetCore.Demo.Models;
using StructuredFilter.AspNetCore.Demo.Scenes;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddFilterService<Player>([
    f => new PidFilter(f),
    f => new UserNameFilter(f),
    f => new PlayerGameVersionFilter(f)
]);

var app = builder.Build();

List<Player> players =
[
    new()
    {
        Level = 11,
        Name = "Dark Knight",
        Pid = 1000,
        User = new User
        {
            Name = "Scott",
            PhoneNumber = "12000000000",
            Uid = 1000000,
            Address = new Address { Country = "CN" },
            IsMale = true,
        },
        GameVersion = new Version("1.0.1")
    },
    new()
    {
        Level = 25,
        Name = "Joker",
        Pid = 1050,
        User = new User
        {
            Name = "Tom",
            PhoneNumber = "13000000000",
            Uid = 1006000,
            Address = new Address { Country = "CN" },
            IsMale = true,
        },
        GameVersion = new Version("1.0.2")
    },
    new()
    {
        Level = 20,
        Name = "Riddler",
        Pid = 2050,
        User = new User
        {
            Name = "Bob",
            PhoneNumber = "13000002000",
            Uid = 1008000,
            Address = new Address { Country = "CN" },
            IsMale = true,
        },
        GameVersion = new Version("1.0.2")
    }
];

const string rawFilter = "{\"pid\": {\"$range\": [1000, 1200]}}";

app.MapGet("/players", (FilterService<Player> filterService) => filterService.FilterOut(rawFilter, players));

app.Run();
