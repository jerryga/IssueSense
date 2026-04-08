using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace IssueSense.Infrastructure.Documents;

public sealed class UserDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("userName")]
    public string UserName { get; set; } = string.Empty;

    [BsonElement("passwordHash")]
    public string PasswordHash { get; set; } = string.Empty;

    [BsonElement("role")]
    public string Role { get; set; } = string.Empty;

    [BsonElement("displayName")]
    public string DisplayName { get; set; } = string.Empty;

    [BsonElement("isActive")]
    public bool IsActive { get; set; } = true;

    [BsonElement("createdAtUtc")]
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    [BsonElement("lastLoginAtUtc")]
    [BsonIgnoreIfNull]
    public DateTime? LastLoginAtUtc { get; set; }

    [BsonElement("failedLoginCount")]
    public int FailedLoginCount { get; set; }

    [BsonElement("lockoutEndUtc")]
    [BsonIgnoreIfNull]
    public DateTime? LockoutEndUtc { get; set; }
}
