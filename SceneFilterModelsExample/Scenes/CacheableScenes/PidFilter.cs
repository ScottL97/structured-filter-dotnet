using System.Threading.Tasks;
using SceneFilterModelsExample.Models;
using StructuredFilter.Filters;
using StructuredFilter.Filters.Common;
using StructuredFilter.Filters.SceneFilters.Scenes;

namespace SceneFilterModelsExample.Scenes.CacheableScenes;

[FilterLabel("玩家 ID")]
[FilterKey("pid")]
[Cacheable]
public class PidFilter(FilterFactory<Player> filterFactory)
    : NumberSceneFilter<Player>(filterFactory, player => Task.FromResult((double)player.Pid), new PlayerFilterCache());
