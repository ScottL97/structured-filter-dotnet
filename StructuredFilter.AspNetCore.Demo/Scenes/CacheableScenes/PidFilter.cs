using System.Threading.Tasks;
using StructuredFilter.AspNetCore.Demo.Models;
using StructuredFilter.Filters;
using StructuredFilter.Filters.Common;
using StructuredFilter.Filters.SceneFilters.Scenes;

namespace StructuredFilter.AspNetCore.Demo.Scenes.CacheableScenes;

[FilterLabel("玩家 ID")]
[FilterKey("pid")]
[Cacheable]
public class PidFilter(FilterFactory<Player> filterFactory)
    : LongSceneFilter<Player>(filterFactory, player => ValueTask.FromResult(player.Pid), new PlayerFilterCache());
