using Chat.App.API.AgentServices;
using Chat.App.API.Database;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException(
        "Database connection string is required.");

builder.Services.AddDatabaseServices(connectionString);

var agentApiKey = builder.Configuration["Agent:ApiKey"] ?? string.Empty;
var agentModel = builder.Configuration["Agent:Model"] ?? string.Empty;
var agentName = builder.Configuration["Agent:Name"] ?? string.Empty;
var agentSystemPrompt = builder.Configuration["Agent:SystemPrompt"] ?? string.Empty;

builder.Services.AddAgentServices(
    agentApiKey,
    agentModel,
    agentName,
    agentSystemPrompt);

builder.Services.AddCors(options =>
{
    var origins =
        builder.Configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>()
        ?? ["*"];

    options.AddPolicy("ChatAppCors", policy =>
    {
        if (origins.Contains("*"))
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        }
        else
        {
            policy.WithOrigins(origins)
                  .AllowAnyHeader()
                  .AllowAnyMethod();
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

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "v1");
    });
}

app.UseHttpsRedirection();
app.UseCors("ChatAppCors");

app.MapControllers();

app.MapGet("/", () => Results.Ok(new
{
    status = "Chat.App API is running."
}));

app.Run();

public partial class Program;