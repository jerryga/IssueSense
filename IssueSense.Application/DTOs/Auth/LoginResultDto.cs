namespace IssueSense.Application.DTOs.Auth;

public sealed class LoginResultDto
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime? LockoutEndUtc { get; set; }
    public AuthUserDto? User { get; set; }
}
