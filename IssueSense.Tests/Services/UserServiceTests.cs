using IssueSense.Application.DTOs.Auth;
using IssueSense.Application.Interfaces.Repositories;
using IssueSense.Application.Services;
using IssueSense.Domain.Entities;
using Moq;

namespace IssueSense.Tests.Services;

public sealed class UserServiceTests
{
    [Fact]
    public async Task LoginAsync_UpgradesLegacySha256HashToPbkdf2()
    {
        var user = new UserAccount
        {
            Id = "user-1",
            UserName = "admin",
            DisplayName = "Support Admin",
            Role = "support_admin",
            PasswordHash = "E86F78A8A3CAF0B60D8E74E5942AA6D86DC150CD3C03338AEF25B7D2D7E3ACC7"
        };

        var repositoryMock = new Mock<IUserRepository>();
        repositoryMock
            .Setup(x => x.GetByUserNameAsync("admin", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        repositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<UserAccount>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var service = new UserService(repositoryMock.Object);

        var result = await service.LoginAsync(new LoginRequestDto
        {
            UserName = " Admin ",
            Password = "Admin@123"
        });

        Assert.True(result.Success);
        Assert.NotNull(result.User);
        Assert.StartsWith("PBKDF2$", user.PasswordHash, StringComparison.Ordinal);
        repositoryMock.Verify(x => x.UpdateAsync(It.Is<UserAccount>(u => u.PasswordHash.StartsWith("PBKDF2$", StringComparison.Ordinal)), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_ReturnsNull_WhenPasswordIsInvalid()
    {
        var repositoryMock = new Mock<IUserRepository>();
        repositoryMock
            .Setup(x => x.GetByUserNameAsync("admin", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserAccount
            {
                Id = "user-1",
                UserName = "admin",
                DisplayName = "Support Admin",
                Role = "support_admin",
                PasswordHash = "PBKDF2$100000$3P5LoK/oe0GSqzLwYfVQjQ==$TPrxvF+0TL5Jg8Jd9fpwN02rFjW4MHaUHx0r7wQVvFU="
            });

        var service = new UserService(repositoryMock.Object);

        var result = await service.LoginAsync(new LoginRequestDto
        {
            UserName = "admin",
            Password = "wrong-password"
        });

        Assert.False(result.Success);
        Assert.Null(result.User);
        Assert.Equal("Invalid username or password.", result.ErrorMessage);
        repositoryMock.Verify(x => x.UpdateAsync(It.Is<UserAccount>(u => u.FailedLoginCount == 1), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_LocksUserAfterMaxFailedAttempts()
    {
        var repositoryMock = new Mock<IUserRepository>();
        repositoryMock
            .Setup(x => x.GetByUserNameAsync("admin", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserAccount
            {
                Id = "user-1",
                UserName = "admin",
                DisplayName = "Support Admin",
                Role = "support_admin",
                FailedLoginCount = 4,
                PasswordHash = "PBKDF2$100000$3P5LoK/oe0GSqzLwYfVQjQ==$TPrxvF+0TL5Jg8Jd9fpwN02rFjW4MHaUHx0r7wQVvFU="
            });

        repositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<UserAccount>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var service = new UserService(repositoryMock.Object);

        var result = await service.LoginAsync(new LoginRequestDto
        {
            UserName = "admin",
            Password = "wrong-password"
        });

        Assert.False(result.Success);
        Assert.NotNull(result.LockoutEndUtc);
        Assert.Contains("Too many failed login attempts", result.ErrorMessage);
        repositoryMock.Verify(
            x => x.UpdateAsync(
                It.Is<UserAccount>(u => u.FailedLoginCount == 5 && u.LockoutEndUtc.HasValue),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task LoginAsync_ReturnsLockoutMessage_WhenUserIsStillLocked()
    {
        var repositoryMock = new Mock<IUserRepository>();
        repositoryMock
            .Setup(x => x.GetByUserNameAsync("admin", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserAccount
            {
                Id = "user-1",
                UserName = "admin",
                DisplayName = "Support Admin",
                Role = "support_admin",
                LockoutEndUtc = DateTime.UtcNow.AddMinutes(10),
                PasswordHash = "PBKDF2$100000$3P5LoK/oe0GSqzLwYfVQjQ==$TPrxvF+0TL5Jg8Jd9fpwN02rFjW4MHaUHx0r7wQVvFU="
            });

        var service = new UserService(repositoryMock.Object);

        var result = await service.LoginAsync(new LoginRequestDto
        {
            UserName = "admin",
            Password = "Admin@123"
        });

        Assert.False(result.Success);
        Assert.Contains("Too many failed login attempts", result.ErrorMessage);
        repositoryMock.Verify(x => x.UpdateAsync(It.IsAny<UserAccount>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
