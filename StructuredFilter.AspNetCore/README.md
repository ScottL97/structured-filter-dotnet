# StructuredFilter.AspNetCore

ASP.NET Core version of [StructuredFilter](https://www.nuget.org/packages/StructuredFilter).

## How to use

* Integration:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddFilterService<Player>([
    f => new PidFilter(f),
    f => new UserNameFilter(f),
    f => new PlayerGameVersionFilter(f)
]);

var app = builder.Build();

app.Run();
```

* Match:

```csharp
app.MapGet("/players",
    (FilterService<Player> filterService) => filterService.FilterOut(rawFilter, players));
```
