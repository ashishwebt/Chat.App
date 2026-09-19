
using Chat.App.API.AgentServices.HistoryProvider;
using Chat.App.API.Services;
using Microsoft.Agents.AI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;

namespace Chat.App.API.AgentServices;

public static class AgentServiceCollectionExtensions
{
    public static IServiceCollection AddAgentServices(
        this IServiceCollection services,
        string apiKey,
        string model,
        string name,
        string systemPrompt)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new ArgumentException("API key is required.", nameof(apiKey));
        }

        if (string.IsNullOrWhiteSpace(model))
        {
            throw new ArgumentException("Model is required.", nameof(model));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Agent name is required.", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(systemPrompt))
        {
            throw new ArgumentException("System prompt is required.", nameof(systemPrompt));
        }
        services.AddPooledDbContextFactory<ChatHistoryDbContext>(options =>
        {
            options.UseSqlite("Data Source=chat-history.db");
        });

        services.AddSingleton<SqliteChatHistoryProvider>();
        services.AddSingleton<SqliteChatHistoryProvider>();
        services.AddSingleton<IAgentService>((provider) =>
        {
            using (var scope = provider.CreateScope())
            {
                var chatHistoryProvider = scope.ServiceProvider.GetRequiredService<SqliteChatHistoryProvider>();
                ChatClientAgentOptions options = new()
                {
                    ChatHistoryProvider = chatHistoryProvider,
                    Name = name,
                    ChatOptions = new()
                    {
                        Instructions = systemPrompt,
                    }
                };

                ChatClientAgent agent = new(
                    options: options,
                    chatClient: new Google.GenAI.Client(vertexAI: false, apiKey: apiKey).AsIChatClient(model)
                );
                return new AgentService(agent);
            }

        });
        return services;
    }
}
