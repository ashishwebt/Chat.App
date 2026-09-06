using System.Text.Json;
using Chat.App.API.AgentServices;
using Chat.App.API.Configuration;
using Chat.App.API.Database;
using Chat.App.API.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddSingleton(new JsonSerializerOptions(JsonSerializerDefaults.Web));

var databaseSettings = builder.Configuration.GetSection(DatabaseSettings.SectionName).Get<DatabaseSettings>()
    ?? new DatabaseSettings();

builder.Services.AddDatabaseServices(databaseSettings);

var agentSettings = builder.Configuration.GetSection(AgentSettings.SectionName).Get<AgentSettings>()
    ?? new AgentSettings();

if (string.IsNullOrWhiteSpace(agentSettings.ApiKey))
{
    throw new InvalidOperationException("Agent:ApiKey is required in appsettings.json.");
}

if (string.IsNullOrWhiteSpace(agentSettings.Model))
{
    throw new InvalidOperationException("Agent:Model is required in appsettings.json.");
}

if (string.IsNullOrWhiteSpace(agentSettings.Name))
{
    throw new InvalidOperationException("Agent:Name is required in appsettings.json.");
}

if (string.IsNullOrWhiteSpace(agentSettings.SystemPrompt))
{
    throw new InvalidOperationException("Agent:SystemPrompt is required in appsettings.json.");
}

builder.Services.AddAgentServices(agentSettings.ApiKey, agentSettings.Model, agentSettings.Name, agentSettings.SystemPrompt);

builder.Services.AddCors(options =>
{
    var corsSettings = builder.Configuration.GetSection(CorsSettings.SectionName).Get<CorsSettings>()
        ?? new CorsSettings();
    var origins = corsSettings.AllowedOrigins;

    options.AddPolicy("ChatAppCors", policy =>
    {
        if (origins.Contains("*"))
        {
            policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
        }
        else
        {
            policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod();
        }
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("ChatAppCors");

app.MapControllers();

app.MapGet("/", () => Results.Ok(new { status = "Chat.App API is running." }));

app.Run();

public partial class Program;