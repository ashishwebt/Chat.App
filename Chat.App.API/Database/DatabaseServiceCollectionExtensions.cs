using Microsoft.EntityFrameworkCore;

namespace Chat.App.API.Database;

public static class DatabaseServiceCollectionExtensions
{
    public static IServiceCollection AddDatabaseServices(this IServiceCollection services, string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentNullException("Database connection string is required. Set 'ConnectionStrings:Default' or 'ConnectionString' in configuration.");
        }

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(connectionString));

        services.AddScoped<IConversationRepository, ConversationRepository>();

        return services;
    }
}
