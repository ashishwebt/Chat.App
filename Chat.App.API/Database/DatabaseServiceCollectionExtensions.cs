using Chat.App.API.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Chat.App.API.Database;

public static class DatabaseServiceCollectionExtensions
{
    public static IServiceCollection AddDatabaseServices(this IServiceCollection services, DatabaseSettings databaseSettings)
    {
        var dbOptions = databaseSettings ?? new DatabaseSettings();

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(dbOptions.ConnectionString));

        services.AddScoped<IConversationRepository, ConversationRepository>();

        return services;
    }
}
