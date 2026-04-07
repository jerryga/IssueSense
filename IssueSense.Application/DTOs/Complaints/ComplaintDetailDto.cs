using IssueSense.Domain.Enums;

namespace IssueSense.Application.DTOs.Complaints;

public sealed class ComplaintDetailDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string CreatedByUserName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public ComplaintStatus Status { get; set; }
    public SentimentType Sentiment { get; set; }
    public UrgencyLevel Urgency { get; set; }
    public double Confidence { get; set; }
    public bool RequiresAction { get; set; }
    public IReadOnlyCollection<AIActionItemDto> SuggestedActions { get; set; } = Array.Empty<AIActionItemDto>();
    public string AssignedOwner { get; set; } = string.Empty;
    public EscalationStatus EscalationStatus { get; set; }
    public string EscalationReason { get; set; } = string.Empty;
    public bool IsArchived { get; set; }
    public DateTime? ArchivedAtUtc { get; set; }
    public string ArchivedByUserName { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public IReadOnlyCollection<ComplaintCommentDto> Comments { get; set; } = Array.Empty<ComplaintCommentDto>();
}
