using IssueSense.Application.DTOs.Complaints;
using IssueSense.Domain.Enums;

namespace IssueSense.Web.Models.Complaints;

public sealed class ComplaintDetailsPageViewModel
{
    public ComplaintDetailDto Complaint { get; set; } = new();
    public ComplaintCommentCreateViewModel NewComment { get; set; } = new();
    public ComplaintStatusUpdateViewModel StatusUpdate { get; set; } = new();
    public ComplaintAssignmentUpdateViewModel AssignmentUpdate { get; set; } = new();
    public IReadOnlyCollection<ComplaintStatus> AvailableStatuses { get; set; } = Enum.GetValues<ComplaintStatus>();
}
