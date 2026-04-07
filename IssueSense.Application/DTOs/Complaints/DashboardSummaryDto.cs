namespace IssueSense.Application.DTOs.Complaints;

public sealed class DashboardSummaryDto
{
    public int TotalComplaints { get; set; }
    public int OpenComplaints { get; set; }
    public int ResolvedComplaints { get; set; }
    public int HighUrgencyComplaints { get; set; }
    public int NegativeSentimentComplaints { get; set; }
    public int EscalatedComplaints { get; set; }
    public IReadOnlyCollection<AnalyticsBreakdownDto> SentimentBreakdown { get; set; } = Array.Empty<AnalyticsBreakdownDto>();
    public IReadOnlyCollection<AnalyticsBreakdownDto> UrgencyBreakdown { get; set; } = Array.Empty<AnalyticsBreakdownDto>();
    public IReadOnlyCollection<AnalyticsBreakdownDto> StatusBreakdown { get; set; } = Array.Empty<AnalyticsBreakdownDto>();
    public IReadOnlyCollection<AnalyticsBreakdownDto> CategoryBreakdown { get; set; } = Array.Empty<AnalyticsBreakdownDto>();
    public IReadOnlyCollection<ComplaintListItemDto> RecentComplaints { get; set; } = Array.Empty<ComplaintListItemDto>();
}
