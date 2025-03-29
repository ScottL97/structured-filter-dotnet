# StructuredFilter.AspNetCore

ASP.NET Core version of [StructuredFilter](https://www.nuget.org/packages/StructuredFilter).

## How to use

* Integration:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddFilterService<Player>();

var app = builder.Build();

app.Run();
```

* Match:

```csharp
app.MapGet("/players",
    (FilterService<Player> filterService) => filterService.FilterOutAsync(rawFilter, players));
```

## FilterOption

### IsSceneFilterOverrideAllowed

* AddFilterService searches and dependency-injects all classes with FilterKey Attribute as singletons and adds them to FilterFactory. By default, AddFilterService adding a SceneFilter with an existing key will throw a FilterException:

```csharp
builder.Services.AddSingleton<PidFilter>();
builder.Services.AddFilterService(); // throws a FilterException
```

* To allow overwriting of SceneFilter with the same key, you can configure it in FilterOption:

```csharp
builder.Services.AddSingleton<PidFilter>();
builder.Services.AddFilterService(option: new FilterOption<Player>
{
    IsSceneFilterOverrideAllowed = true
});
```
