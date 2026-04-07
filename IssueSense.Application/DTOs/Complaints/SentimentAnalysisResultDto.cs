using IssueSense.Domain.Enums;

namespace IssueSense.Application.DTOs.Complaints;

public sealed class SentimentAnalysisResultDto
{
    public SentimentType Sentiment { get; set; }
    public string Category { get; set; } = string.Empty;
    public UrgencyLevel Urgency { get; set; }
    public double Confidence { get; set; }
    public bool RequiresAction { get; set; }
    public IReadOnlyCollection<AIActionItemDto> SuggestedActions { get; set; } = Array.Empty<AIActionItemDto>();
}
