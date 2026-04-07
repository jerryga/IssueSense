using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace IssueSense.Domain.MongoModels;

public sealed class User
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("email")]
    public string Email { get; set; } = string.Empty;

    [BsonElement("passwordHash")]
    public string PasswordHash { get; set; } = string.Empty;

    [BsonElement("role")]
    [BsonRepresentation(BsonType.String)]
    public UserRole Role { get; set; }

    [BsonElement("isActive")]
    public bool IsActive { get; set; } = true;

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("lastLoginAt")]
    [BsonIgnoreIfNull]
    public DateTime? LastLoginAt { get; set; }
}

public enum UserRole
{
    [BsonRepresentation(BsonType.String)]
    support_admin,

    [BsonRepresentation(BsonType.String)]
    analyst
}
