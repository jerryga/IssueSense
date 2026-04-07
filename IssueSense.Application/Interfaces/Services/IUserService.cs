using IssueSense.Application.DTOs.Auth;
using IssueSense.Application.DTOs.Users;

namespace IssueSense.Application.Interfaces.Services;

public interface IUserService
{
    Task<AuthUserDto?> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<UserListItemDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<(bool Success, string? ErrorMessage)> CreateAsync(UserCreateDto request, CancellationToken cancellationToken = default);
    bool HasRole(string? userRole, params string[] allowedRoles);
    Task SeedDefaultUsersAsync(CancellationToken cancellationToken = default);
}
