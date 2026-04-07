using IssueSense.Application.DTOs.Users;

namespace IssueSense.Web.Models.Users;

public sealed class UserManagementPageViewModel
{
    public IReadOnlyCollection<UserListItemDto> Users { get; set; } = Array.Empty<UserListItemDto>();
    public UserCreateViewModel NewUser { get; set; } = new();
}
