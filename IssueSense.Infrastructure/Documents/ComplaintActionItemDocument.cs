using MongoDB.Bson.Serialization.Attributes;

namespace IssueSense.Infrastructure.Documents;

public sealed class ComplaintActionItemDocument
{
    [BsonElement("owner")]
    public string Owner { get; set; } = string.Empty;

    [BsonElement("action")]
    public string Action { get; set; } = string.Empty;
}
