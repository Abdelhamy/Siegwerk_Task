using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Pricing.Infrastructure.Persistence;
public static class DbSeeder
{
    public static async Task Seed(this IServiceProvider scopedProvider, string rawEnvName)
    {
        var dbContext = scopedProvider.GetRequiredService<PricingDbContext>();
        Console.WriteLine($"Database migration started for environment: {rawEnvName}");

        await dbContext.Database.MigrateAsync();

        Console.WriteLine($"Database migration completed for environment: {rawEnvName}");
    }

}
