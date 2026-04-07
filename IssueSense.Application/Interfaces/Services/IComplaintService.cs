using IssueSense.Application.DTOs.Complaints;

namespace IssueSense.Application.Interfaces.Services;

public interface IComplaintService
{
    Task CreateAsync(ComplaintCreateDto request, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<ComplaintListItemDto>> GetAllAsync(ComplaintQueryDto query, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<ComplaintListItemDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ComplaintDetailDto?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task UpdateStatusAsync(ComplaintStatusUpdateDto request, CancellationToken cancellationToken = default);
    Task AddCommentAsync(ComplaintCommentCreateDto request, CancellationToken cancellationToken = default);
    Task UpdateAssignmentAsync(ComplaintAssignmentUpdateDto request, CancellationToken cancellationToken = default);
    Task ReanalyzeAsync(string complaintId, CancellationToken cancellationToken = default);
    Task ArchiveAsync(string complaintId, string archivedByUserName, CancellationToken cancellationToken = default);
    Task<DashboardSummaryDto> GetDashboardSummaryAsync(CancellationToken cancellationToken = default);
    Task SeedSampleComplaintsAsync(int count = 100, CancellationToken cancellationToken = default);
}
