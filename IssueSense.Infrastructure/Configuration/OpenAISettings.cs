namespace IssueSense.Infrastructure.Configuration;

public sealed class OpenAISettings
{
    public const string SectionName = "OpenAI";

    public bool Enabled { get; set; }
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "gpt-5.4-nano";
    public string Endpoint { get; set; } = "https://api.openai.com/v1/responses";
    public bool UseMockFallback { get; set; } = true;
}
