using IssueSense.Domain.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace IssueSense.Infrastructure.Documents;

public sealed class ComplaintDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("title")]
    public string Title { get; set; } = string.Empty;

    [BsonElement("description")]
    public string Description { get; set; } = string.Empty;

    [BsonElement("customerName")]
    public string CustomerName { get; set; } = string.Empty;

    [BsonElement("customerEmail")]
    public string CustomerEmail { get; set; } = string.Empty;

    [BsonElement("createdByUserName")]
    public string CreatedByUserName { get; set; } = string.Empty;

    [BsonElement("status")]
    public ComplaintStatus Status { get; set; }

    [BsonElement("sentiment")]
    public SentimentType Sentiment { get; set; }

    [BsonElement("category")]
    public string Category { get; set; } = string.Empty;

    [BsonElement("urgency")]
    public UrgencyLevel Urgency { get; set; }

    [BsonElement("confidence")]
    public double Confidence { get; set; }

    [BsonElement("requiresAction")]
    public bool RequiresAction { get; set; }

    [BsonElement("suggestedActions")]
    public List<ComplaintActionItemDocument> SuggestedActions { get; set; } = [];

    [BsonElement("assignedOwner")]
    public string AssignedOwner { get; set; } = string.Empty;

    [BsonElement("escalationStatus")]
    public EscalationStatus EscalationStatus { get; set; }

    [BsonElement("escalationReason")]
    public string EscalationReason { get; set; } = string.Empty;

    [BsonElement("isArchived")]
    public bool IsArchived { get; set; }

    [BsonElement("archivedAtUtc")]
    [BsonIgnoreIfNull]
    public DateTime? ArchivedAtUtc { get; set; }

    [BsonElement("archivedByUserName")]
    public string ArchivedByUserName { get; set; } = string.Empty;

    [BsonElement("createdAtUtc")]
    public DateTime CreatedAtUtc { get; set; }

    [BsonElement("updatedAtUtc")]
    public DateTime UpdatedAtUtc { get; set; }

    [BsonElement("comments")]
    public List<ComplaintCommentDocument> Comments { get; set; } = [];
}
