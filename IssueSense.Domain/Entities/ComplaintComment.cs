namespace IssueSense.Domain.Entities;

public sealed class ComplaintComment
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string AuthorUserName { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
