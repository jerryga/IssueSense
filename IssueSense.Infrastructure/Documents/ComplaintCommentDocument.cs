using MongoDB.Bson.Serialization.Attributes;

namespace IssueSense.Infrastructure.Documents;

public sealed class ComplaintCommentDocument
{
    [BsonElement("id")]
    public string Id { get; set; } = string.Empty;

    [BsonElement("authorUserName")]
    public string AuthorUserName { get; set; } = string.Empty;

    [BsonElement("message")]
    public string Message { get; set; } = string.Empty;

    [BsonElement("createdAtUtc")]
    public DateTime CreatedAtUtc { get; set; }
}
