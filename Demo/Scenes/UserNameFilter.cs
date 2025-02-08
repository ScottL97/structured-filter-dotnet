using StructuredFilter.Filters;
using StructuredFilter.Filters.Common;
using StructuredFilter.Filters.SceneFilters.Scenes;

namespace Demo.Scenes;

[FilterLabel("用户名")]
[FilterKey("userName")]
public class UserNameFilter(FilterFactory<Player> filterFactory)
    : StringSceneFilter<Player>(filterFactory, player => Task.FromResult(player.User.Name));
