using System.ComponentModel.DataAnnotations;

namespace IssueSense.Web.Models.Auth;

public sealed class LoginViewModel
{
    [Required]
    [StringLength(64, MinimumLength = 3)]
    [RegularExpression(@".*\S.*", ErrorMessage = "Username cannot be empty or whitespace.")]
    [Display(Name = "Username")]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [StringLength(128, MinimumLength = 8)]
    [RegularExpression(@".*\S.*", ErrorMessage = "Password cannot be empty or whitespace.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}
