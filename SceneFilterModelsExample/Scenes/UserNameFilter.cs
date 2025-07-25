﻿using System.Threading.Tasks;
using SceneFilterModelsExample.Models;
using StructuredFilter.Filters;
using StructuredFilter.Filters.Common;
using StructuredFilter.Filters.SceneFilters.Scenes;

namespace SceneFilterModelsExample.Scenes;

[FilterLabel("用户名")]
[FilterKey("userName")]
public class UserNameFilter(FilterFactory<Player> filterFactory)
    : StringSceneFilter<Player>(filterFactory, player => ValueTask.FromResult(player.User.Name));
