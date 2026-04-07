using System.ComponentModel.DataAnnotations;

namespace IssueSense.Web.Models.Complaints;

public sealed class ComplaintCommentCreateViewModel
{
    [Required]
    [RegularExpression("^[a-fA-F0-9]{24}$", ErrorMessage = "Invalid complaint identifier.")]
    public string ComplaintId { get; set; } = string.Empty;

    [Required]
    [StringLength(800, MinimumLength = 3)]
    [RegularExpression(@".*\S.*", ErrorMessage = "Comment cannot be empty or whitespace.")]
    public string Message { get; set; } = string.Empty;
}
