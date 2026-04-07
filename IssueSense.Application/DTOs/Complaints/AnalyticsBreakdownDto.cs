namespace IssueSense.Application.DTOs.Complaints;

public sealed class AnalyticsBreakdownDto
{
    public string Label { get; set; } = string.Empty;
    public int Count { get; set; }
    public double Percentage { get; set; }
}
