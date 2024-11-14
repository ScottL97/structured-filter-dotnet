using StructuredFilter.Filters;
using StructuredFilter.Filters.Common;
using StructuredFilter.Filters.SceneFilters.Scenes;
using StructuredFilter.Test.Models;

namespace StructuredFilter.Test.Scenes;

[FilterLabel("玩家游戏版本")]
[FilterKey("playerGameVersion")]
public class PlayerGameVersionFilter(FilterFactory<Player> filterFactory)
    : VersionSceneFilter<Player>(filterFactory, player => player.GameVersion);
