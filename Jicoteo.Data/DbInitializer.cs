using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LCSC.Data;

public static class DbInitializer
{
    public static void AddRepositories(IServiceCollection collection)
    {
    }

    public static void InitDb()
    {
        var context = new JicoteoDbContext();
        context.Database.Migrate();
    }
}