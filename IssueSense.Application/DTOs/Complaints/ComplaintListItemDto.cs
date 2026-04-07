using IssueSense.Domain.Enums;

namespace IssueSense.Application.DTOs.Complaints;

public sealed class ComplaintListItemDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public ComplaintStatus Status { get; set; }
    public SentimentType Sentiment { get; set; }
    public UrgencyLevel Urgency { get; set; }
    public double Confidence { get; set; }
    public EscalationStatus EscalationStatus { get; set; }
    public string AssignedOwner { get; set; } = string.Empty;
    public bool IsArchived { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
