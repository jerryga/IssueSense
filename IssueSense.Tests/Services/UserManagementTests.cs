using IssueSense.Application.DTOs.Users;
using IssueSense.Application.Interfaces.Repositories;
using IssueSense.Application.Services;
using IssueSense.Domain.Entities;
using Moq;

namespace IssueSense.Tests.Services;

public sealed class UserManagementTests
{
    [Fact]
    public async Task CreateAsync_CreatesUserWithHashedPassword()
    {
        var repositoryMock = new Mock<IUserRepository>();
        repositoryMock
            .Setup(x => x.GetByUserNameAsync("new.user", It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserAccount?)null);

        UserAccount? insertedUser = null;
        repositoryMock
            .Setup(x => x.InsertAsync(It.IsAny<UserAccount>(), It.IsAny<CancellationToken>()))
            .Callback<UserAccount, CancellationToken>((user, _) => insertedUser = user)
            .Returns(Task.CompletedTask);

        var service = new UserService(repositoryMock.Object);

        var result = await service.CreateAsync(new UserCreateDto
        {
            UserName = "New.User",
            DisplayName = "New User",
            Password = "StrongPass@123",
            Role = "analyst"
        });

        Assert.True(result.Success);
        Assert.Null(result.ErrorMessage);
        Assert.NotNull(insertedUser);
        Assert.Equal("new.user", insertedUser!.UserName);
        Assert.Equal("New User", insertedUser.DisplayName);
        Assert.Equal("analyst", insertedUser.Role);
        Assert.StartsWith("PBKDF2$", insertedUser.PasswordHash, StringComparison.Ordinal);
    }

    [Fact]
    public async Task CreateAsync_RejectsDuplicateUserName()
    {
        var repositoryMock = new Mock<IUserRepository>();
        repositoryMock
            .Setup(x => x.GetByUserNameAsync("admin", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserAccount
            {
                Id = "1",
                UserName = "admin",
                DisplayName = "Support Admin",
                Role = "support_admin",
                PasswordHash = "PBKDF2$100000$abc$def"
            });

        var service = new UserService(repositoryMock.Object);

        var result = await service.CreateAsync(new UserCreateDto
        {
            UserName = "admin",
            DisplayName = "Duplicate Admin",
            Password = "StrongPass@123",
            Role = "support_admin"
        });

        Assert.False(result.Success);
        Assert.Equal("That username is already in use.", result.ErrorMessage);
        repositoryMock.Verify(x => x.InsertAsync(It.IsAny<UserAccount>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
