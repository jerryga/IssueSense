using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using IssueSense.Application.DTOs.Complaints;
using IssueSense.Application.Interfaces.Services;
using IssueSense.Domain.Enums;
using IssueSense.Infrastructure.Configuration;

namespace IssueSense.Infrastructure.Services;

public sealed class AIAnalysisService(HttpClient httpClient, OpenAISettings settings) : IAIAnalysisService
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private static readonly string[] NegativeKeywords = ["angry", "bad", "broken", "delay", "issue", "late", "poor", "refund", "terrible", "worst"];
    private static readonly string[] PositiveKeywords = ["appreciate", "excellent", "good", "great", "happy", "love", "quick", "satisfied", "thanks"];
    private static readonly string[] HighUrgencyKeywords = ["asap", "critical", "immediately", "urgent", "outage", "fraud"];
    private static readonly Dictionary<string, string[]> CategoryKeywords = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Billing"] = ["billing", "charge", "invoice", "payment", "refund"],
        ["Technical"] = ["app", "bug", "error", "system", "login", "crash"],
        ["Delivery"] = ["delivery", "shipment", "package", "courier", "late"],
        ["Customer Service"] = ["agent", "service", "representative", "support", "response"]
    };

    public async Task<SentimentAnalysisResultDto> AnalyzeTextAsync(string complaintText, CancellationToken cancellationToken = default)
    {
        if (!settings.Enabled || string.IsNullOrWhiteSpace(settings.ApiKey))
        {
            return AnalyzeWithMockRules(complaintText);
        }

        try
        {
            var aiResult = await AnalyzeWithOpenAiAsync(complaintText, cancellationToken);
            return aiResult ?? AnalyzeWithMockRules(complaintText);
        }
        catch
        {
            if (!settings.UseMockFallback)
            {
                throw;
            }

            return AnalyzeWithMockRules(complaintText);
        }
    }

    private async Task<SentimentAnalysisResultDto?> AnalyzeWithOpenAiAsync(string complaintText, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, settings.Endpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.ApiKey);

        var payload = new
        {
            model = settings.Model,
            instructions = "You are an AI complaint classifier. Classify complaint text and return only structured JSON matching the schema.",
            input = $"Analyze this complaint description: {complaintText}",
            text = new
            {
                format = new
                {
                    type = "json_schema",
                    name = "complaint_analysis",
                    strict = true,
                    schema = new
                    {
                        type = "object",
                        additionalProperties = false,
                        properties = new
                        {
                            sentiment = new
                            {
                                type = "string",
                                enum_ = new[] { "positive", "neutral", "negative" }
                            },
                            category = new
                            {
                                type = "string"
                            },
                            urgency = new
                            {
                                type = "string",
                                enum_ = new[] { "low", "medium", "high" }
                            },
                            confidence = new
                            {
                                type = "number"
                            },
                            requiresAction = new
                            {
                                type = "boolean"
                            },
                            suggestedActions = new
                            {
                                type = "array",
                                items = new
                                {
                                    type = "object",
                                    additionalProperties = false,
                                    properties = new
                                    {
                                        owner = new { type = "string" },
                                        action = new { type = "string" }
                                    },
                                    required = new[] { "owner", "action" }
                                }
                            }
                        },
                        required = new[] { "sentiment", "category", "urgency", "confidence", "requiresAction", "suggestedActions" }
                    }
                }
            }
        };

        var normalizedPayload = NormalizeSchemaPropertyNames(payload);
        request.Content = new StringContent(JsonSerializer.Serialize(normalizedPayload, SerializerOptions), Encoding.UTF8, "application/json");

        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var apiResponse = await JsonSerializer.DeserializeAsync<OpenAIResponse>(responseStream, SerializerOptions, cancellationToken);

        var rawJson = apiResponse?.Output?
            .SelectMany(x => x.Content ?? [])
            .FirstOrDefault(x => x.Type == "output_text")
            ?.Text;

        if (string.IsNullOrWhiteSpace(rawJson))
        {
            return null;
        }

        var result = JsonSerializer.Deserialize<OpenAIComplaintAnalysisResult>(rawJson, SerializerOptions);
        if (result is null)
        {
            return null;
        }

        return new SentimentAnalysisResultDto
        {
            Sentiment = MapSentiment(result.Sentiment),
            Category = string.IsNullOrWhiteSpace(result.Category) ? "General" : result.Category.Trim(),
            Urgency = MapUrgency(result.Urgency),
            Confidence = Math.Clamp(result.Confidence, 0, 1),
            RequiresAction = result.RequiresAction,
            SuggestedActions = result.SuggestedActions
                .Where(x => !string.IsNullOrWhiteSpace(x.Owner) && !string.IsNullOrWhiteSpace(x.Action))
                .Select(x => new AIActionItemDto
                {
                    Owner = x.Owner.Trim(),
                    Action = x.Action.Trim()
                })
                .ToArray()
        };
    }

    private static object NormalizeSchemaPropertyNames(object payload)
    {
        using var document = JsonDocument.Parse(JsonSerializer.Serialize(payload, SerializerOptions));
        using var buffer = new MemoryStream();
        using var writer = new Utf8JsonWriter(buffer);
        RewriteElement(document.RootElement, writer);
        writer.Flush();
        return JsonSerializer.Deserialize<object>(buffer.ToArray(), SerializerOptions)!;
    }

    private static void RewriteElement(JsonElement element, Utf8JsonWriter writer)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                writer.WriteStartObject();
                foreach (var property in element.EnumerateObject())
                {
                    var propertyName = property.NameEquals("enum_") ? "enum" : property.Name;
                    writer.WritePropertyName(propertyName);
                    RewriteElement(property.Value, writer);
                }
                writer.WriteEndObject();
                break;
            case JsonValueKind.Array:
                writer.WriteStartArray();
                foreach (var item in element.EnumerateArray())
                {
                    RewriteElement(item, writer);
                }
                writer.WriteEndArray();
                break;
            default:
                element.WriteTo(writer);
                break;
        }
    }

    private static SentimentAnalysisResultDto AnalyzeWithMockRules(string complaintText)
    {
        var text = complaintText.Trim().ToLowerInvariant();
        var negativeMatches = NegativeKeywords.Count(text.Contains);
        var positiveMatches = PositiveKeywords.Count(text.Contains);
        var highUrgencyMatches = HighUrgencyKeywords.Count(text.Contains);
        var categoryMatch = CategoryKeywords.FirstOrDefault(x => x.Value.Any(text.Contains));

        var sentiment = negativeMatches > positiveMatches
            ? SentimentType.Negative
            : positiveMatches > negativeMatches
                ? SentimentType.Positive
                : SentimentType.Neutral;

        var urgency = highUrgencyMatches > 0 || negativeMatches >= 3
            ? UrgencyLevel.High
            : highUrgencyMatches == 0 && negativeMatches == 0
                ? UrgencyLevel.Low
                : UrgencyLevel.Medium;

        var confidence = CalculateConfidence(negativeMatches, positiveMatches, highUrgencyMatches, categoryMatch.Key is not null);

        var requiresAction = urgency != UrgencyLevel.Low || sentiment == SentimentType.Negative;
        var suggestedActions = BuildMockActionItems(sentiment, urgency, categoryMatch.Key ?? "General");

        return new SentimentAnalysisResultDto
        {
            Sentiment = sentiment,
            Urgency = urgency,
            Category = categoryMatch.Key ?? "General",
            Confidence = confidence,
            RequiresAction = requiresAction,
            SuggestedActions = suggestedActions
        };
    }

    private static IReadOnlyCollection<AIActionItemDto> BuildMockActionItems(
        SentimentType sentiment,
        UrgencyLevel urgency,
        string category)
    {
        var actions = new List<AIActionItemDto>();

        if (urgency == UrgencyLevel.High)
        {
            actions.Add(new AIActionItemDto
            {
                Owner = "@triage_officer",
                Action = "Review this complaint immediately and confirm escalation priority."
            });
        }

        if (category.Equals("Billing", StringComparison.OrdinalIgnoreCase))
        {
            actions.Add(new AIActionItemDto
            {
                Owner = "@case_manager",
                Action = "Validate billing records and prepare a customer update or refund decision."
            });
        }
        else if (category.Equals("Technical", StringComparison.OrdinalIgnoreCase))
        {
            actions.Add(new AIActionItemDto
            {
                Owner = "@case_manager",
                Action = "Reproduce the technical issue and coordinate with engineering support."
            });
        }
        else if (category.Equals("Delivery", StringComparison.OrdinalIgnoreCase))
        {
            actions.Add(new AIActionItemDto
            {
                Owner = "@triage_officer",
                Action = "Check shipment status and contact logistics for a delivery update."
            });
        }

        if (sentiment == SentimentType.Negative)
        {
            actions.Add(new AIActionItemDto
            {
                Owner = "@support_admin",
                Action = "Review customer communication quality and approve the next response."
            });
        }

        return actions.DistinctBy(x => $"{x.Owner}:{x.Action}").ToArray();
    }

    private static SentimentType MapSentiment(string value) =>
        value.Trim().ToLowerInvariant() switch
        {
            "positive" => SentimentType.Positive,
            "negative" => SentimentType.Negative,
            _ => SentimentType.Neutral
        };

    private static UrgencyLevel MapUrgency(string value) =>
        value.Trim().ToLowerInvariant() switch
        {
            "high" => UrgencyLevel.High,
            "low" => UrgencyLevel.Low,
            _ => UrgencyLevel.Medium
        };

    private static double CalculateConfidence(int negativeMatches, int positiveMatches, int urgencyMatches, bool categoryMatched)
    {
        var signalScore = negativeMatches + positiveMatches + urgencyMatches + (categoryMatched ? 1 : 0);
        var boundedScore = Math.Min(signalScore, 5);
        return Math.Round(0.55 + (boundedScore * 0.08), 2, MidpointRounding.AwayFromZero);
    }

    private sealed class OpenAIResponse
    {
        [JsonPropertyName("output")]
        public List<OpenAIOutputItem>? Output { get; set; }
    }

    private sealed class OpenAIOutputItem
    {
        [JsonPropertyName("content")]
        public List<OpenAIContentItem>? Content { get; set; }
    }

    private sealed class OpenAIContentItem
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("text")]
        public string? Text { get; set; }
    }

    private sealed class OpenAIComplaintAnalysisResult
    {
        [JsonPropertyName("sentiment")]
        public string Sentiment { get; set; } = "neutral";

        [JsonPropertyName("category")]
        public string Category { get; set; } = "General";

        [JsonPropertyName("urgency")]
        public string Urgency { get; set; } = "medium";

        [JsonPropertyName("confidence")]
        public double Confidence { get; set; }

        [JsonPropertyName("requiresAction")]
        public bool RequiresAction { get; set; }

        [JsonPropertyName("suggestedActions")]
        public List<OpenAIActionItem> SuggestedActions { get; set; } = [];
    }

    private sealed class OpenAIActionItem
    {
        [JsonPropertyName("owner")]
        public string Owner { get; set; } = string.Empty;

        [JsonPropertyName("action")]
        public string Action { get; set; } = string.Empty;
    }
}
