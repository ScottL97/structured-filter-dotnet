using System.Threading.Tasks;
using StructuredFilter.AspNetCore.Demo.Models;
using StructuredFilter.Filters;
using StructuredFilter.Filters.Common;
using StructuredFilter.Filters.SceneFilters.Scenes;

namespace StructuredFilter.AspNetCore.Demo.Scenes;

[FilterLabel("用户名")]
[FilterKey("userName")]
public class UserNameFilter(FilterFactory<Player> filterFactory)
    : StringSceneFilter<Player>(filterFactory, player => ValueTask.FromResult(player.User.Name));
