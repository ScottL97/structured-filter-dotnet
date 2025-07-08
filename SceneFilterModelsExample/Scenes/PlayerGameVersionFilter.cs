using System.Threading.Tasks;
using SceneFilterModelsExample.Models;
using StructuredFilter.Filters;
using StructuredFilter.Filters.Common;
using StructuredFilter.Filters.SceneFilters.Scenes;

namespace SceneFilterModelsExample.Scenes;

[FilterLabel("玩家游戏版本")]
[FilterKey("playerGameVersion")]
public class PlayerGameVersionFilter(FilterFactory<Player> filterFactory)
    : VersionSceneFilter<Player>(filterFactory, player => ValueTask.FromResult(player.GameVersion));
