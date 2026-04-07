using System.ComponentModel.DataAnnotations;
using IssueSense.Domain.Enums;

namespace IssueSense.Web.Models.Complaints;

public sealed class ComplaintStatusUpdateViewModel
{
    [Required]
    [RegularExpression("^[a-fA-F0-9]{24}$", ErrorMessage = "Invalid complaint identifier.")]
    public string ComplaintId { get; set; } = string.Empty;

    [Required]
    public ComplaintStatus Status { get; set; }
}
