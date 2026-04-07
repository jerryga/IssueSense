using IssueSense.Domain.Enums;

namespace IssueSense.Application.DTOs.Complaints;

public sealed class ComplaintQueryDto
{
    public string SearchTerm { get; set; } = string.Empty;
    public ComplaintStatus? Status { get; set; }
    public SentimentType? Sentiment { get; set; }
    public UrgencyLevel? Urgency { get; set; }
    public EscalationStatus? EscalationStatus { get; set; }
    public bool IncludeArchived { get; set; }
}
