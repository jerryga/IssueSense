namespace IssueSense.Application.DTOs.Complaints;

public sealed class ComplaintAssignmentUpdateDto
{
    public string ComplaintId { get; set; } = string.Empty;
    public string AssignedOwner { get; set; } = string.Empty;
}
