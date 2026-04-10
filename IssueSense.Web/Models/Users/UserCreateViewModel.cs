using System.ComponentModel.DataAnnotations;
using IssueSense.Domain.Common;

namespace IssueSense.Web.Models.Users;

public sealed class UserCreateViewModel
{
    [Required]
    [StringLength(64, MinimumLength = 3)]
    [RegularExpression("^[a-zA-Z0-9._-]+$", ErrorMessage = "Username can only contain letters, numbers, periods, underscores, and hyphens.")]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Display Name")]
    [StringLength(120, MinimumLength = 3)]
    [RegularExpression(@".*\S.*", ErrorMessage = "Display name cannot be empty or whitespace.")]
    public string DisplayName { get; set; } = string.Empty;

    [Required]
    [StringLength(128, MinimumLength = 8)]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Role")]
    [RegularExpression($"^({RoleNames.UserAdmin}|{RoleNames.SupportAdmin}|{RoleNames.Analyst}|{RoleNames.TriageOfficer}|{RoleNames.CaseManager}|{RoleNames.AiReviewer})$", ErrorMessage = "Invalid role.")]
    public string Role { get; set; } = string.Empty;
}
