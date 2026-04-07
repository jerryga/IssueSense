using IssueSense.Application.DTOs.Complaints;

namespace IssueSense.Application.Interfaces.Services;

public interface IAIAnalysisService
{
    Task<SentimentAnalysisResultDto> AnalyzeTextAsync(string complaintText, CancellationToken cancellationToken = default);
}
