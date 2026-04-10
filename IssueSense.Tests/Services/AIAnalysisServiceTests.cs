using System.Net;
using System.Text;
using IssueSense.Domain.Enums;
using IssueSense.Infrastructure.Configuration;
using IssueSense.Infrastructure.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace IssueSense.Tests.Services;

public sealed class AIAnalysisServiceTests
{
    [Fact]
    public async Task AnalyzeTextAsync_UsesMockRules_WhenOpenAiDisabled()
    {
        var service = new AIAnalysisService(
            new HttpClient(new StubHandler(_ => throw new InvalidOperationException("Should not be called"))),
            new OpenAISettings { Enabled = false },
            NullLogger<AIAnalysisService>.Instance);

        var result = await service.AnalyzeTextAsync("Refund issue, urgent and very bad service.");

        Assert.Equal(SentimentType.Negative, result.Sentiment);
        Assert.Equal(UrgencyLevel.High, result.Urgency);
        Assert.True(result.RequiresAction);
        Assert.NotEmpty(result.SuggestedActions);
    }

    [Fact]
    public async Task AnalyzeTextAsync_FallsBackToMockRules_WhenOpenAiFailsAndFallbackEnabled()
    {
        var service = new AIAnalysisService(
            new HttpClient(new StubHandler(_ => throw new HttpRequestException("boom"))),
            new OpenAISettings
            {
                Enabled = true,
                ApiKey = "test-key",
                UseMockFallback = true
            },
            NullLogger<AIAnalysisService>.Instance);

        var result = await service.AnalyzeTextAsync("The app is broken and this is urgent.");

        Assert.Equal(SentimentType.Negative, result.Sentiment);
        Assert.Equal(UrgencyLevel.High, result.Urgency);
    }

    [Fact]
    public async Task AnalyzeTextAsync_MapsStructuredOpenAiResponse_WhenCallSucceeds()
    {
        const string responseJson = """
        {
          "output": [
            {
              "content": [
                {
                  "type": "output_text",
                  "text": "{\"sentiment\":\"negative\",\"category\":\"Billing\",\"urgency\":\"high\",\"confidence\":0.87,\"requiresAction\":true,\"suggestedActions\":[{\"owner\":\"@case_manager\",\"action\":\"Review the billing dispute.\"}]}"
                }
              ]
            }
          ]
        }
        """;

        var service = new AIAnalysisService(
            new HttpClient(new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
            })),
            new OpenAISettings
            {
                Enabled = true,
                ApiKey = "test-key",
                UseMockFallback = true
            },
            NullLogger<AIAnalysisService>.Instance);

        var result = await service.AnalyzeTextAsync("I was charged twice.");

        Assert.Equal(SentimentType.Negative, result.Sentiment);
        Assert.Equal("Billing", result.Category);
        Assert.Equal(UrgencyLevel.High, result.Urgency);
        Assert.Equal(0.87, result.Confidence);
        Assert.True(result.RequiresAction);
        Assert.Single(result.SuggestedActions);
        Assert.Equal("@case_manager", result.SuggestedActions.First().Owner);
    }

    private sealed class StubHandler(Func<HttpRequestMessage, HttpResponseMessage> handler) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(handler(request));
    }
}
