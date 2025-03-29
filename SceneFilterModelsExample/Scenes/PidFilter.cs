using System.Threading.Tasks;
using SceneFilterModelsExample.Models;
using StructuredFilter.Filters;
using StructuredFilter.Filters.Common;
using StructuredFilter.Filters.SceneFilters.Scenes;

namespace SceneFilterModelsExample.Scenes;

[FilterLabel("玩家 ID")]
[FilterKey("pid")]
public class PidFilter(FilterFactory<Player> filterFactory)
    : NumberSceneFilter<Player>(filterFactory, player => Task.FromResult((double)player.Pid));
