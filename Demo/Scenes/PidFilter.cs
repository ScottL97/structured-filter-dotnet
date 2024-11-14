﻿using StructuredFilter.Filters;
using StructuredFilter.Filters.Common;
using StructuredFilter.Filters.SceneFilters.Scenes;

namespace Demo.Scenes;

[FilterLabel("玩家 ID")]
[FilterKey("pid")]
public class PidFilter(FilterFactory<Player> filterFactory)
    : NumberSceneFilter<Player>(filterFactory, player => player.Pid);
