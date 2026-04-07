namespace IssueSense.Application.DTOs.Complaints;

public sealed class ComplaintCommentDto
{
    public string Id { get; set; } = string.Empty;
    public string AuthorUserName { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
}
