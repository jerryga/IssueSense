using IssueSense.Application.DTOs.Complaints;
using IssueSense.Application.Interfaces.Repositories;
using IssueSense.Application.Interfaces.Services;
using IssueSense.Domain.Entities;
using IssueSense.Domain.Enums;

namespace IssueSense.Application.Services;

public sealed class ComplaintService(
    IComplaintRepository complaintRepository,
    IAIAnalysisService aiAnalysisService) : IComplaintService
{
    private static readonly string[] CustomerNames =
    [
        "Ava Thompson", "Noah Carter", "Olivia Brooks", "Liam Foster", "Emma Hayes",
        "Mason Reed", "Sophia Bennett", "Lucas Morris", "Isabella Cooper", "Ethan Kelly",
        "Mia Price", "James Richardson", "Charlotte Murphy", "Benjamin Ward", "Amelia Cox",
        "Henry Diaz", "Harper Howard", "Alexander James", "Evelyn Watson", "Michael Torres"
    ];

    private static readonly string[] ComplaintTitles =
    [
        "Refund delay on recent order",
        "Mobile app crashes during checkout",
        "Agent did not follow up on ticket",
        "Invoice contains incorrect charge",
        "Delivery package arrived late",
        "Unable to reset account password",
        "Subscription canceled unexpectedly",
        "Duplicate payment posted to card",
        "Portal shows error while uploading files",
        "Customer support response was too slow"
    ];

    private static readonly string[] ComplaintDescriptions =
    [
        "I requested a refund two weeks ago and still have not received it. This delay is frustrating and support has not provided a clear update.",
        "The app keeps crashing whenever I try to complete checkout on my phone. This is urgent because I need to place the order today.",
        "Your representative promised a callback but nobody followed up. The lack of response makes the whole service feel poor.",
        "My latest invoice includes a charge that I do not recognize. Please review this billing issue immediately and explain the extra amount.",
        "The package was supposed to arrive three days ago and it is still missing. This late delivery is causing problems for our team.",
        "I cannot log in because the password reset link throws an error page. The system looks broken and I need access as soon as possible.",
        "My paid subscription suddenly shows as canceled even though payment was successful. This issue is affecting our daily work.",
        "I was charged twice for the same service. I need a refund and confirmation that this billing problem is fixed.",
        "The upload page returns an error every time I submit documents. This technical issue blocks our onboarding process.",
        "Support eventually replied, but the response time was far too slow and the answers were not helpful."
    ];

    private static readonly string[] SupportComments =
    [
        "Initial triage completed and routed to the relevant queue.",
        "Customer contacted for clarification on the reported problem.",
        "Internal team investigating the issue and validating next steps.",
        "Provided interim update and requested supporting screenshots.",
        "Marked for priority review based on complaint urgency."
    ];

    public async Task CreateAsync(ComplaintCreateDto request, CancellationToken cancellationToken = default)
    {
        var analysis = await aiAnalysisService.AnalyzeTextAsync(request.Description, cancellationToken);

        var complaint = new Complaint
        {
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            CustomerName = request.CustomerName.Trim(),
            CustomerEmail = request.CustomerEmail.Trim(),
            CreatedByUserName = request.CreatedByUserName,
            Category = analysis.Category,
            Sentiment = analysis.Sentiment,
            Urgency = analysis.Urgency,
            Confidence = analysis.Confidence,
            RequiresAction = analysis.RequiresAction,
            SuggestedActions = analysis.SuggestedActions.Select(x => new ComplaintActionItem
            {
                Owner = x.Owner,
                Action = x.Action
            }).ToList(),
            AssignedOwner = analysis.SuggestedActions.FirstOrDefault()?.Owner ?? string.Empty,
            Status = ComplaintStatus.New,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        ApplyEscalationRules(complaint);
        await complaintRepository.CreateAsync(complaint, cancellationToken);
    }

    public async Task<IReadOnlyCollection<ComplaintListItemDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await GetAllAsync(new ComplaintQueryDto(), cancellationToken);
    }

    public async Task<IReadOnlyCollection<ComplaintListItemDto>> GetAllAsync(ComplaintQueryDto query, CancellationToken cancellationToken = default)
    {
        var complaints = await complaintRepository.GetAllAsync(query, cancellationToken);
        return complaints
            .Select(MapListItem)
            .ToArray();
    }

    public async Task<ComplaintDetailDto?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var complaint = await complaintRepository.GetByIdAsync(id, cancellationToken);
        if (complaint is null)
        {
            return null;
        }

        return new ComplaintDetailDto
        {
            Id = complaint.Id,
            Title = complaint.Title,
            Description = complaint.Description,
            CustomerName = complaint.CustomerName,
            CustomerEmail = complaint.CustomerEmail,
            CreatedByUserName = complaint.CreatedByUserName,
            Category = complaint.Category,
            Status = complaint.Status,
            Sentiment = complaint.Sentiment,
            Urgency = complaint.Urgency,
            Confidence = complaint.Confidence,
            RequiresAction = complaint.RequiresAction,
            SuggestedActions = complaint.SuggestedActions.Select(x => new AIActionItemDto
            {
                Owner = x.Owner,
                Action = x.Action
            }).ToArray(),
            AssignedOwner = complaint.AssignedOwner,
            EscalationStatus = complaint.EscalationStatus,
            EscalationReason = complaint.EscalationReason,
            IsArchived = complaint.IsArchived,
            ArchivedAtUtc = complaint.ArchivedAtUtc,
            ArchivedByUserName = complaint.ArchivedByUserName,
            CreatedAtUtc = complaint.CreatedAtUtc,
            UpdatedAtUtc = complaint.UpdatedAtUtc,
            Comments = complaint.Comments
                .OrderByDescending(x => x.CreatedAtUtc)
                .Select(x => new ComplaintCommentDto
                {
                    Id = x.Id,
                    AuthorUserName = x.AuthorUserName,
                    Message = x.Message,
                    CreatedAtUtc = x.CreatedAtUtc
                })
                .ToArray()
        };
    }

    public Task UpdateStatusAsync(ComplaintStatusUpdateDto request, CancellationToken cancellationToken = default) =>
        complaintRepository.UpdateStatusAsync(request.ComplaintId, request.Status, cancellationToken);

    public Task AddCommentAsync(ComplaintCommentCreateDto request, CancellationToken cancellationToken = default)
    {
        var comment = new ComplaintComment
        {
            AuthorUserName = request.AuthorUserName,
            Message = request.Message.Trim(),
            CreatedAtUtc = DateTime.UtcNow
        };

        return complaintRepository.AddCommentAsync(request.ComplaintId, comment, cancellationToken);
    }

    public async Task UpdateAssignmentAsync(ComplaintAssignmentUpdateDto request, CancellationToken cancellationToken = default)
    {
        var complaint = await complaintRepository.GetByIdAsync(request.ComplaintId, cancellationToken);
        if (complaint is null)
        {
            return;
        }

        complaint.AssignedOwner = request.AssignedOwner.Trim();
        await complaintRepository.UpdateAsync(complaint, cancellationToken);
    }

    public async Task ReanalyzeAsync(string complaintId, CancellationToken cancellationToken = default)
    {
        var complaint = await complaintRepository.GetByIdAsync(complaintId, cancellationToken);
        if (complaint is null)
        {
            return;
        }

        var analysis = await aiAnalysisService.AnalyzeTextAsync(complaint.Description, cancellationToken);
        complaint.Sentiment = analysis.Sentiment;
        complaint.Category = analysis.Category;
        complaint.Urgency = analysis.Urgency;
        complaint.Confidence = analysis.Confidence;
        complaint.RequiresAction = analysis.RequiresAction;
        complaint.SuggestedActions = analysis.SuggestedActions.Select(x => new ComplaintActionItem
        {
            Owner = x.Owner,
            Action = x.Action
        }).ToList();
        if (string.IsNullOrWhiteSpace(complaint.AssignedOwner) && complaint.SuggestedActions.Count > 0)
        {
            complaint.AssignedOwner = complaint.SuggestedActions[0].Owner;
        }

        ApplyEscalationRules(complaint);
        await complaintRepository.UpdateAsync(complaint, cancellationToken);
    }

    public async Task ArchiveAsync(string complaintId, string archivedByUserName, CancellationToken cancellationToken = default)
    {
        var complaint = await complaintRepository.GetByIdAsync(complaintId, cancellationToken);
        if (complaint is null || complaint.IsArchived)
        {
            return;
        }

        complaint.IsArchived = true;
        complaint.ArchivedAtUtc = DateTime.UtcNow;
        complaint.ArchivedByUserName = archivedByUserName.Trim();
        await complaintRepository.UpdateAsync(complaint, cancellationToken);
    }

    public async Task<DashboardSummaryDto> GetDashboardSummaryAsync(CancellationToken cancellationToken = default)
    {
        var complaints = await complaintRepository.GetAllAsync(cancellationToken);
        var orderedComplaints = complaints
            .Where(x => !x.IsArchived)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToArray();

        return new DashboardSummaryDto
        {
            TotalComplaints = orderedComplaints.Length,
            OpenComplaints = orderedComplaints.Count(x => x.Status is ComplaintStatus.New or ComplaintStatus.InProgress),
            ResolvedComplaints = orderedComplaints.Count(x => x.Status is ComplaintStatus.Resolved or ComplaintStatus.Closed),
            HighUrgencyComplaints = orderedComplaints.Count(x => x.Urgency == UrgencyLevel.High),
            NegativeSentimentComplaints = orderedComplaints.Count(x => x.Sentiment == SentimentType.Negative),
            EscalatedComplaints = orderedComplaints.Count(x => x.EscalationStatus == EscalationStatus.Escalated),
            SentimentBreakdown = BuildBreakdown(
                orderedComplaints.GroupBy(x => x.Sentiment.ToString()),
                orderedComplaints.Length),
            UrgencyBreakdown = BuildBreakdown(
                orderedComplaints.GroupBy(x => x.Urgency.ToString()),
                orderedComplaints.Length),
            StatusBreakdown = BuildBreakdown(
                orderedComplaints.GroupBy(x => x.Status.ToString()),
                orderedComplaints.Length),
            CategoryBreakdown = BuildBreakdown(
                orderedComplaints.GroupBy(x => x.Category),
                orderedComplaints.Length),
            RecentComplaints = orderedComplaints.Take(5).Select(MapListItem).ToArray()
        };
    }

    public async Task SeedSampleComplaintsAsync(int count = 100, CancellationToken cancellationToken = default)
    {
        var existingComplaints = await complaintRepository.GetAllAsync(cancellationToken);
        if (existingComplaints.Count >= count)
        {
            return;
        }

        var complaintsToCreate = count - existingComplaints.Count;

        for (var i = 0; i < complaintsToCreate; i++)
        {
            var templateIndex = i % ComplaintTitles.Length;
            var customerIndex = i % CustomerNames.Length;
            var createdAt = DateTime.UtcNow.AddDays(-(i % 45)).AddHours(-(i % 24)).AddMinutes(-(i * 7 % 60));
            var description = $"{ComplaintDescriptions[templateIndex]} Reference #{1000 + i}.";
            var analysis = await aiAnalysisService.AnalyzeTextAsync(description, cancellationToken);

            var status = (i % 4) switch
            {
                0 => ComplaintStatus.New,
                1 => ComplaintStatus.InProgress,
                2 => ComplaintStatus.Resolved,
                _ => ComplaintStatus.Closed
            };

            var complaint = new Complaint
            {
                Title = $"{ComplaintTitles[templateIndex]} #{i + 1}",
                Description = description,
                CustomerName = CustomerNames[customerIndex],
                CustomerEmail = BuildCustomerEmail(CustomerNames[customerIndex], i),
                CreatedByUserName = i % 2 == 0 ? "admin" : "analyst",
                Status = status,
                Category = analysis.Category,
                Sentiment = analysis.Sentiment,
                Urgency = analysis.Urgency,
                Confidence = analysis.Confidence,
                RequiresAction = analysis.RequiresAction,
                SuggestedActions = analysis.SuggestedActions.Select(x => new ComplaintActionItem
                {
                    Owner = x.Owner,
                    Action = x.Action
                }).ToList(),
                AssignedOwner = analysis.SuggestedActions.FirstOrDefault()?.Owner ?? string.Empty,
                CreatedAtUtc = createdAt,
                UpdatedAtUtc = createdAt.AddHours(i % 12),
                Comments = BuildComments(i, createdAt, status)
            };

            ApplyEscalationRules(complaint);
            await complaintRepository.InsertAsync(complaint, cancellationToken);
        }
    }

    private static ComplaintListItemDto MapListItem(Complaint complaint) =>
        new()
        {
            Id = complaint.Id,
            Title = complaint.Title,
            CustomerName = complaint.CustomerName,
            Category = complaint.Category,
            Status = complaint.Status,
            Sentiment = complaint.Sentiment,
            Urgency = complaint.Urgency,
            Confidence = complaint.Confidence,
            EscalationStatus = complaint.EscalationStatus,
            AssignedOwner = complaint.AssignedOwner,
            IsArchived = complaint.IsArchived,
            CreatedAtUtc = complaint.CreatedAtUtc
        };

    private static IReadOnlyCollection<AnalyticsBreakdownDto> BuildBreakdown(
        IEnumerable<IGrouping<string, Complaint>> groups,
        int totalCount)
    {
        return groups
            .OrderByDescending(x => x.Count())
            .Select(x => new AnalyticsBreakdownDto
            {
                Label = x.Key,
                Count = x.Count(),
                Percentage = totalCount == 0
                    ? 0
                    : Math.Round((double)x.Count() / totalCount * 100, 1, MidpointRounding.AwayFromZero)
            })
            .ToArray();
    }

    private static void ApplyEscalationRules(Complaint complaint)
    {
        var reasons = new List<string>();

        if (complaint.Urgency == UrgencyLevel.High)
        {
            reasons.Add("High urgency complaint");
        }

        if (complaint.Sentiment == SentimentType.Negative)
        {
            reasons.Add("Negative sentiment detected");
        }

        var complaintAge = DateTime.UtcNow - complaint.CreatedAtUtc;
        if (complaint.Status is ComplaintStatus.New or ComplaintStatus.InProgress && complaintAge.TotalDays >= 3)
        {
            reasons.Add("Open complaint older than 3 days");
        }

        complaint.EscalationStatus = reasons.Count > 0
            ? EscalationStatus.Escalated
            : EscalationStatus.Normal;

        complaint.EscalationReason = string.Join("; ", reasons);
    }

    private static string BuildCustomerEmail(string customerName, int index)
    {
        var normalized = customerName.ToLowerInvariant().Replace(" ", ".");
        return $"{normalized}{index + 1}@example.com";
    }

    private static List<ComplaintComment> BuildComments(int index, DateTime createdAt, ComplaintStatus status)
    {
        var comments = new List<ComplaintComment>();
        var firstComment = new ComplaintComment
        {
            AuthorUserName = "Support Admin",
            Message = SupportComments[index % SupportComments.Length],
            CreatedAtUtc = createdAt.AddHours(2)
        };

        comments.Add(firstComment);

        if (status is ComplaintStatus.Resolved or ComplaintStatus.Closed)
        {
            comments.Add(new ComplaintComment
            {
                AuthorUserName = "Support Admin",
                Message = "Issue resolved and final response shared with the customer.",
                CreatedAtUtc = createdAt.AddHours(8)
            });
        }
        else if (status == ComplaintStatus.InProgress)
        {
            comments.Add(new ComplaintComment
            {
                AuthorUserName = "Support Admin",
                Message = "Work is in progress and the case remains under active review.",
                CreatedAtUtc = createdAt.AddHours(5)
            });
        }

        return comments;
    }
}
