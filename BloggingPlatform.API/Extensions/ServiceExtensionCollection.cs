using BloggingPlatform.Data.Repositories.Interface;
using BloggingPlatform.Data.Repositories.Providers;

namespace BloggingPlatform.API.Extensions;

public static class ServiceExtensionCollection
{
    public static IServiceCollection RegisterServices(this IServiceCollection services, IConfiguration config = null!)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

        return services;
    }
}