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

        var authUser = await service.LoginAsync(new LoginRequestDto
        {
            UserName = " Admin ",
            Password = "Admin@123"
        });

        Assert.NotNull(authUser);
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

        var authUser = await service.LoginAsync(new LoginRequestDto
        {
            UserName = "admin",
            Password = "wrong-password"
        });

        Assert.Null(authUser);
        repositoryMock.Verify(x => x.UpdateAsync(It.IsAny<UserAccount>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
