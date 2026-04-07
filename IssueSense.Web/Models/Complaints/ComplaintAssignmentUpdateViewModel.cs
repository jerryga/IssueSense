using System.ComponentModel.DataAnnotations;

namespace IssueSense.Web.Models.Complaints;

public sealed class ComplaintAssignmentUpdateViewModel
{
    [Required]
    [RegularExpression("^[a-fA-F0-9]{24}$", ErrorMessage = "Invalid complaint identifier.")]
    public string ComplaintId { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Assign To")]
    [RegularExpression(@".*\S.*", ErrorMessage = "Assigned owner is required.")]
    public string AssignedOwner { get; set; } = string.Empty;
}
