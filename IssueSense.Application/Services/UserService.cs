using IssueSense.Application.DTOs.Auth;
using IssueSense.Application.DTOs.Users;
using IssueSense.Application.Interfaces.Repositories;
using IssueSense.Application.Interfaces.Services;
using IssueSense.Application.Security;
using IssueSense.Domain.Common;
using IssueSense.Domain.Entities;

namespace IssueSense.Application.Services;

public sealed class UserService(IUserRepository userRepository) : IUserService
{
    public async Task<AuthUserDto?> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        var normalizedUserName = request.UserName.Trim().ToLowerInvariant();
        var user = await userRepository.GetByUserNameAsync(normalizedUserName, cancellationToken);
        if (user is null)
        {
            return null;
        }

        var verificationResult = PasswordSecurity.VerifyPassword(request.Password, user.PasswordHash);
        if (verificationResult == PasswordVerificationResult.Failed)
        {
            return null;
        }

        if (verificationResult == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = PasswordSecurity.HashPassword(request.Password);
            await userRepository.UpdateAsync(user, cancellationToken);
        }

        return new AuthUserDto
        {
            Id = user.Id,
            UserName = user.UserName,
            DisplayName = user.DisplayName,
            Role = user.Role
        };
    }

    public bool HasRole(string? userRole, params string[] allowedRoles)
    {
        if (string.IsNullOrWhiteSpace(userRole))
        {
            return false;
        }

        return allowedRoles.Any(role => string.Equals(userRole, role, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<IReadOnlyCollection<UserListItemDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var users = await userRepository.GetAllAsync(cancellationToken);
        return users
            .OrderBy(x => x.DisplayName)
            .Select(x => new UserListItemDto
            {
                Id = x.Id,
                UserName = x.UserName,
                DisplayName = x.DisplayName,
                Role = x.Role
            })
            .ToArray();
    }

    public async Task<(bool Success, string? ErrorMessage)> CreateAsync(UserCreateDto request, CancellationToken cancellationToken = default)
    {
        var normalizedUserName = request.UserName.Trim().ToLowerInvariant();
        if (await userRepository.GetByUserNameAsync(normalizedUserName, cancellationToken) is not null)
        {
            return (false, "That username is already in use.");
        }

        var user = new UserAccount
        {
            UserName = normalizedUserName,
            DisplayName = request.DisplayName.Trim(),
            PasswordHash = PasswordSecurity.HashPassword(request.Password),
            Role = request.Role.Trim()
        };

        await userRepository.InsertAsync(user, cancellationToken);
        return (true, null);
    }

    public async Task SeedDefaultUsersAsync(CancellationToken cancellationToken = default)
    {
        var users = new[]
        {
            new UserAccount
            {
                UserName = "admin",
                DisplayName = "Support Admin",
                PasswordHash = PasswordSecurity.HashPassword("Admin@123"),
                Role = RoleNames.SupportAdmin
            },
            new UserAccount
            {
                UserName = "analyst",
                DisplayName = "Complaint Analyst",
                PasswordHash = PasswordSecurity.HashPassword("Analyst@123"),
                Role = RoleNames.Analyst
            },
            new UserAccount
            {
                UserName = "triage",
                DisplayName = "Triage Officer",
                PasswordHash = PasswordSecurity.HashPassword("Triage@123"),
                Role = RoleNames.TriageOfficer
            },
            new UserAccount
            {
                UserName = "casemanager",
                DisplayName = "Case Manager",
                PasswordHash = PasswordSecurity.HashPassword("Case@123"),
                Role = RoleNames.CaseManager
            },
            new UserAccount
            {
                UserName = "aireviewer",
                DisplayName = "AI Reviewer",
                PasswordHash = PasswordSecurity.HashPassword("Review@123"),
                Role = RoleNames.AiReviewer
            }
        };

        var usersToCreate = new List<UserAccount>();

        foreach (var user in users)
        {
            var existingUser = await userRepository.GetByUserNameAsync(user.UserName, cancellationToken);
            if (existingUser is null)
            {
                usersToCreate.Add(user);
            }
        }

        if (usersToCreate.Count > 0)
        {
            await userRepository.CreateManyAsync(usersToCreate, cancellationToken);
        }
    }
}
