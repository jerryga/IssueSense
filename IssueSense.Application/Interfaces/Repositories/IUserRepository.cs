using IssueSense.Domain.Entities;

namespace IssueSense.Application.Interfaces.Repositories;

public interface IUserRepository
{
    Task<IReadOnlyCollection<UserAccount>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<UserAccount?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<UserAccount?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default);
    Task InsertAsync(UserAccount user, CancellationToken cancellationToken = default);
    Task UpdateAsync(UserAccount user, CancellationToken cancellationToken = default);
    Task DeleteAsync(string id, CancellationToken cancellationToken = default);
    Task<bool> AnyAsync(CancellationToken cancellationToken = default);
    Task CreateManyAsync(IEnumerable<UserAccount> users, CancellationToken cancellationToken = default);
}
