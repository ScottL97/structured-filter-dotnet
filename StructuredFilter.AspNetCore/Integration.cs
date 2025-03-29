using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using StructuredFilter.Filters;
using StructuredFilter.Filters.Common;

namespace StructuredFilter.AspNetCore;

public static class ServiceCollectionExtensions
{
    public static void AddFilterService<T>(this IServiceCollection services, FilterOption<T>? option = null)
    {
        var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
        var filterTypes = assembly
            .GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && t.GetCustomAttribute<FilterKey>() != null &&
                        typeof(ISceneFilter<T>).IsAssignableFrom(t));
        foreach (var filterType in filterTypes)
        {
            services.AddSingleton(typeof(ISceneFilter<T>), filterType);
        }

        services.AddSingleton<FilterFactory<T>>();
        option ??= new FilterOption<T>();
        services.AddSingleton<FilterService<T>>(s =>
        {
            var filterFactory = s.GetRequiredService<FilterFactory<T>>();
            var sceneFilters = s.GetServices<ISceneFilter<T>>();
            filterFactory.WithSceneFilters(sceneFilters, option.IsSceneFilterOverrideAllowed);
            return new FilterService<T>(filterFactory, option);
        });
    }
}
