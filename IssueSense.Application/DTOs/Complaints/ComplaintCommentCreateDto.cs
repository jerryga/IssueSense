namespace IssueSense.Application.DTOs.Complaints;

public sealed class ComplaintCommentCreateDto
{
    public string ComplaintId { get; set; } = string.Empty;
    public string AuthorUserName { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
