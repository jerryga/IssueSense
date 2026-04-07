using IssueSense.Domain.Enums;

namespace IssueSense.Application.DTOs.Complaints;

public sealed class ComplaintStatusUpdateDto
{
    public string ComplaintId { get; set; } = string.Empty;
    public ComplaintStatus Status { get; set; }
}
