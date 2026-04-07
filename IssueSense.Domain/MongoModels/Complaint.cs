using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace IssueSense.Domain.MongoModels;

public sealed class Complaint
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("user")]
    public ComplaintUserInfo User { get; set; } = new();

    [BsonElement("title")]
    public string Title { get; set; } = string.Empty;

    [BsonElement("description")]
    public string Description { get; set; } = string.Empty;

    [BsonElement("status")]
    [BsonRepresentation(BsonType.String)]
    public ComplaintStatusValue Status { get; set; } = ComplaintStatusValue.pending;

    [BsonElement("priority")]
    [BsonRepresentation(BsonType.String)]
    public ComplaintPriority Priority { get; set; } = ComplaintPriority.medium;

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("analysisStatus")]
    [BsonRepresentation(BsonType.String)]
    public AnalysisStatusValue AnalysisStatus { get; set; } = AnalysisStatusValue.pending;

    [BsonElement("analysis")]
    [BsonIgnoreIfNull]
    public ComplaintAnalysis? Analysis { get; set; }

    [BsonElement("comments")]
    public List<ComplaintComment> Comments { get; set; } = [];
}

public sealed class ComplaintUserInfo
{
    [BsonElement("user_id")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string UserId { get; set; } = string.Empty;

    [BsonElement("email")]
    public string Email { get; set; } = string.Empty;
}

public sealed class ComplaintAnalysis
{
    [BsonElement("sentiment")]
    public string Sentiment { get; set; } = string.Empty;

    [BsonElement("category")]
    public string Category { get; set; } = string.Empty;

    [BsonElement("urgency")]
    public string Urgency { get; set; } = string.Empty;

    [BsonElement("confidence")]
    [BsonRepresentation(BsonType.Double)]
    public double Confidence { get; set; }

    [BsonElement("analyzedAt")]
    public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;
}

public sealed class ComplaintComment
{
    [BsonElement("commentId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string CommentId { get; set; } = ObjectId.GenerateNewId().ToString();

    [BsonElement("userId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string UserId { get; set; } = string.Empty;

    [BsonElement("userRole")]
    [BsonRepresentation(BsonType.String)]
    public UserRole UserRole { get; set; }

    [BsonElement("comment")]
    public string CommentText { get; set; } = string.Empty;

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public enum ComplaintStatusValue
{
    [BsonRepresentation(BsonType.String)]
    pending,

    [BsonRepresentation(BsonType.String)]
    processing,

    [BsonRepresentation(BsonType.String)]
    resolved
}

public enum ComplaintPriority
{
    [BsonRepresentation(BsonType.String)]
    low,

    [BsonRepresentation(BsonType.String)]
    medium,

    [BsonRepresentation(BsonType.String)]
    high
}

public enum AnalysisStatusValue
{
    [BsonRepresentation(BsonType.String)]
    pending,

    [BsonRepresentation(BsonType.String)]
    completed,

    [BsonRepresentation(BsonType.String)]
    failed
}
