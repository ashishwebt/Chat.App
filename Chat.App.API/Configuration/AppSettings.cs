namespace Chat.App.API.Configuration;



public class OllamaSettings
{
    public const string SectionName = "Ollama";

    public string BaseUrl { get; set; } = "http://localhost:11434";

    public string Model { get; set; } = "qwen3.5:0.8b";
}

public class AgentSettings
{
    public const string SectionName = "Agent";

    public string ApiKey { get; set; } = string.Empty;

    public string Model { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string SystemPrompt { get; set; } = string.Empty;
}

public class SkillsSettings
{
    public const string SectionName = "Skills";

    public string Directory { get; set; } = "Skills";
}

public class CorsSettings
{
    public const string SectionName = "Cors";

    public string[] AllowedOrigins { get; set; } = { "*" };
}