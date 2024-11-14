using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace StructuredFilter.AspNetCore;

public static class ServiceCollectionExtensions
{
    public static void AddFilterService<T>(this IServiceCollection services,
        IEnumerable<FilterService<T>.SceneFilterCreator> sceneFilterCreators)
    {
        services.AddSingleton<FilterService<T>>(s => new FilterService<T>().WithSceneFilters(sceneFilterCreators));
    }
}
