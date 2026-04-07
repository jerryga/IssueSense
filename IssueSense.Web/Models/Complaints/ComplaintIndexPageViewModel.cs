using IssueSense.Application.DTOs.Complaints;

namespace IssueSense.Web.Models.Complaints;

public sealed class ComplaintIndexPageViewModel
{
    public ComplaintQueryDto Query { get; set; } = new();
    public IReadOnlyCollection<ComplaintListItemDto> Complaints { get; set; } = Array.Empty<ComplaintListItemDto>();
}
