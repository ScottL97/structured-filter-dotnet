using StructuredFilter.Filters;
using StructuredFilter.Filters.Common;
using StructuredFilter.Filters.SceneFilters.Scenes;
using StructuredFilter.Test.Models;

namespace StructuredFilter.Test.Scenes;

[FilterLabel("玩家 ID")]
[FilterKey("pid")]
public class PidFilter(FilterFactory<Player> filterFactory)
    : NumberSceneFilter<Player>(filterFactory, player => player.Pid);
