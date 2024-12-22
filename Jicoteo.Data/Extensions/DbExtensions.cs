using Microsoft.Extensions.DependencyInjection;

namespace LSCC.Data.Extensions;

public static class DbExtensions
{
    public static IServiceCollection InitializeDatabase(this IServiceCollection services)
    {
        DbInitializer.AddRepositories(services);
        DbInitializer.InitDb();
        return services;
    }
}