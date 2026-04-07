using IssueSense.Application.DTOs.Complaints;
using IssueSense.Domain.Entities;
using IssueSense.Domain.Enums;

namespace IssueSense.Application.Interfaces.Repositories;

public interface IComplaintRepository
{
    Task<IReadOnlyCollection<Complaint>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Complaint>> GetAllAsync(ComplaintQueryDto query, CancellationToken cancellationToken = default);
    Task<Complaint?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task InsertAsync(Complaint complaint, CancellationToken cancellationToken = default);
    Task UpdateAsync(Complaint complaint, CancellationToken cancellationToken = default);
    Task DeleteAsync(string id, CancellationToken cancellationToken = default);
    Task CreateAsync(Complaint complaint, CancellationToken cancellationToken = default);
    Task UpdateStatusAsync(string id, ComplaintStatus status, CancellationToken cancellationToken = default);
    Task AddCommentAsync(string id, ComplaintComment comment, CancellationToken cancellationToken = default);
}
