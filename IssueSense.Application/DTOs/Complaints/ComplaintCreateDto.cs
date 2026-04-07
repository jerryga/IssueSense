namespace IssueSense.Application.DTOs.Complaints;

public sealed class ComplaintCreateDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string CreatedByUserName { get; set; } = string.Empty;
}
