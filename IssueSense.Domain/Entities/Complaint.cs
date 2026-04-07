using IssueSense.Domain.Enums;

namespace IssueSense.Domain.Entities;

public sealed class Complaint
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string CreatedByUserName { get; set; } = string.Empty;
    public ComplaintStatus Status { get; set; } = ComplaintStatus.New;
    public SentimentType Sentiment { get; set; } = SentimentType.Neutral;
    public string Category { get; set; } = "General";
    public UrgencyLevel Urgency { get; set; } = UrgencyLevel.Medium;
    public double Confidence { get; set; }
    public bool RequiresAction { get; set; }
    public List<ComplaintActionItem> SuggestedActions { get; set; } = [];
    public string AssignedOwner { get; set; } = string.Empty;
    public EscalationStatus EscalationStatus { get; set; } = EscalationStatus.Normal;
    public string EscalationReason { get; set; } = string.Empty;
    public bool IsArchived { get; set; }
    public DateTime? ArchivedAtUtc { get; set; }
    public string ArchivedByUserName { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    public List<ComplaintComment> Comments { get; set; } = [];
}
