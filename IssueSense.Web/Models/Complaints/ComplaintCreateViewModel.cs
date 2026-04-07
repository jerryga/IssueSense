using System.ComponentModel.DataAnnotations;

namespace IssueSense.Web.Models.Complaints;

public sealed class ComplaintCreateViewModel
{
    [Required]
    [StringLength(120)]
    [RegularExpression(@".*\S.*", ErrorMessage = "Title cannot be empty or whitespace.")]
    public string Title { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Customer Name")]
    [StringLength(120)]
    [RegularExpression(@".*\S.*", ErrorMessage = "Customer name cannot be empty or whitespace.")]
    public string CustomerName { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Customer Email")]
    [EmailAddress]
    public string CustomerEmail { get; set; } = string.Empty;

    [Required]
    [StringLength(2000, MinimumLength = 20)]
    [RegularExpression(@".*\S.*", ErrorMessage = "Description cannot be empty or whitespace.")]
    public string Description { get; set; } = string.Empty;
}
