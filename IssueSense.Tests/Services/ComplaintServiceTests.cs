using IssueSense.Application.DTOs.Complaints;
using IssueSense.Application.Interfaces.Repositories;
using IssueSense.Application.Interfaces.Services;
using IssueSense.Application.Services;
using IssueSense.Domain.Entities;
using IssueSense.Domain.Enums;
using Moq;

namespace IssueSense.Tests.Services;

public sealed class ComplaintServiceTests
{
    [Fact]
    public async Task CreateAsync_StoresAnalyzedComplaintWithSuggestedAssignment()
    {
        var repositoryMock = new Mock<IComplaintRepository>();
        Complaint? savedComplaint = null;
        repositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Complaint>(), It.IsAny<CancellationToken>()))
            .Callback<Complaint, CancellationToken>((complaint, _) => savedComplaint = complaint)
            .Returns(Task.CompletedTask);

        var analysisServiceMock = new Mock<IAIAnalysisService>();
        analysisServiceMock
            .Setup(x => x.AnalyzeTextAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SentimentAnalysisResultDto
            {
                Sentiment = SentimentType.Negative,
                Category = "Billing",
                Urgency = UrgencyLevel.High,
                Confidence = 0.96,
                RequiresAction = true,
                SuggestedActions =
                [
                    new AIActionItemDto
                    {
                        Owner = "@case_manager",
                        Action = "Review the double charge and contact billing."
                    }
                ]
            });

        var service = new ComplaintService(repositoryMock.Object, analysisServiceMock.Object);

        await service.CreateAsync(new ComplaintCreateDto
        {
            Title = "  Duplicate payment posted  ",
            Description = "  I was charged twice and need this fixed today.  ",
            CustomerName = "  Test User  ",
            CustomerEmail = "  test@example.com  ",
            CreatedByUserName = "admin"
        });

        repositoryMock.Verify(x => x.CreateAsync(It.IsAny<Complaint>(), It.IsAny<CancellationToken>()), Times.Once);
        Assert.NotNull(savedComplaint);
        Assert.Equal("Duplicate payment posted", savedComplaint!.Title);
        Assert.Equal("I was charged twice and need this fixed today.", savedComplaint.Description);
        Assert.Equal("Test User", savedComplaint.CustomerName);
        Assert.Equal("test@example.com", savedComplaint.CustomerEmail);
        Assert.Equal(SentimentType.Negative, savedComplaint.Sentiment);
        Assert.Equal("Billing", savedComplaint.Category);
        Assert.Equal(UrgencyLevel.High, savedComplaint.Urgency);
        Assert.True(savedComplaint.RequiresAction);
        Assert.Equal("@case_manager", savedComplaint.AssignedOwner);
        Assert.Single(savedComplaint.SuggestedActions);
        Assert.Equal(EscalationStatus.Escalated, savedComplaint.EscalationStatus);
        Assert.Contains("High urgency complaint", savedComplaint.EscalationReason);
    }

    [Fact]
    public async Task ReanalyzeAsync_PreservesExistingAssignmentAndRefreshesAnalysis()
    {
        var existingComplaint = new Complaint
        {
            Id = "complaint-1",
            Title = "App outage",
            Description = "The portal is failing and customers are blocked.",
            CustomerName = "Alex",
            CustomerEmail = "alex@example.com",
            AssignedOwner = "@triage_officer",
            CreatedAtUtc = DateTime.UtcNow.AddDays(-5),
            Status = ComplaintStatus.InProgress
        };

        var repositoryMock = new Mock<IComplaintRepository>();
        repositoryMock
            .Setup(x => x.GetByIdAsync(existingComplaint.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingComplaint);

        Complaint? updatedComplaint = null;
        repositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Complaint>(), It.IsAny<CancellationToken>()))
            .Callback<Complaint, CancellationToken>((complaint, _) => updatedComplaint = complaint)
            .Returns(Task.CompletedTask);

        var analysisServiceMock = new Mock<IAIAnalysisService>();
        analysisServiceMock
            .Setup(x => x.AnalyzeTextAsync(existingComplaint.Description, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SentimentAnalysisResultDto
            {
                Sentiment = SentimentType.Negative,
                Category = "Technical",
                Urgency = UrgencyLevel.High,
                Confidence = 0.91,
                RequiresAction = true,
                SuggestedActions =
                [
                    new AIActionItemDto
                    {
                        Owner = "@case_manager",
                        Action = "Coordinate with engineering and send a customer update."
                    }
                ]
            });

        var service = new ComplaintService(repositoryMock.Object, analysisServiceMock.Object);

        await service.ReanalyzeAsync(existingComplaint.Id);

        repositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Complaint>(), It.IsAny<CancellationToken>()), Times.Once);
        Assert.NotNull(updatedComplaint);
        Assert.Equal("@triage_officer", updatedComplaint!.AssignedOwner);
        Assert.Equal("Technical", updatedComplaint.Category);
        Assert.Equal(SentimentType.Negative, updatedComplaint.Sentiment);
        Assert.Equal(UrgencyLevel.High, updatedComplaint.Urgency);
        Assert.True(updatedComplaint.RequiresAction);
        Assert.Single(updatedComplaint.SuggestedActions);
        Assert.Equal(EscalationStatus.Escalated, updatedComplaint.EscalationStatus);
        Assert.Contains("Open complaint older than 3 days", updatedComplaint.EscalationReason);
    }

    [Fact]
    public async Task ArchiveAsync_MarksComplaintAsArchived()
    {
        var existingComplaint = new Complaint
        {
            Id = "complaint-1",
            Title = "Archive me"
        };

        var repositoryMock = new Mock<IComplaintRepository>();
        repositoryMock
            .Setup(x => x.GetByIdAsync(existingComplaint.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingComplaint);

        Complaint? updatedComplaint = null;
        repositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Complaint>(), It.IsAny<CancellationToken>()))
            .Callback<Complaint, CancellationToken>((complaint, _) => updatedComplaint = complaint)
            .Returns(Task.CompletedTask);

        var analysisServiceMock = new Mock<IAIAnalysisService>();
        var service = new ComplaintService(repositoryMock.Object, analysisServiceMock.Object);

        await service.ArchiveAsync(existingComplaint.Id, "Support Admin");

        Assert.NotNull(updatedComplaint);
        Assert.True(updatedComplaint!.IsArchived);
        Assert.Equal("Support Admin", updatedComplaint.ArchivedByUserName);
        Assert.NotNull(updatedComplaint.ArchivedAtUtc);
        repositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Complaint>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
