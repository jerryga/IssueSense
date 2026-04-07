using System.Text.RegularExpressions;
using IssueSense.Application.DTOs.Complaints;
using IssueSense.Application.Interfaces.Repositories;
using IssueSense.Domain.Entities;
using IssueSense.Domain.Enums;
using IssueSense.Infrastructure.Contexts;
using IssueSense.Infrastructure.Documents;
using MongoDB.Driver;

namespace IssueSense.Infrastructure.Repositories;

public sealed class ComplaintRepository(MongoDbContext context) : IComplaintRepository
{
    public async Task InsertAsync(Complaint complaint, CancellationToken cancellationToken = default)
    {
        var document = MapComplaint(complaint);
        await context.Complaints.InsertOneAsync(document, cancellationToken: cancellationToken);
        complaint.Id = document.Id;
    }

    public Task CreateAsync(Complaint complaint, CancellationToken cancellationToken = default) =>
        InsertAsync(complaint, cancellationToken);

    public async Task<IReadOnlyCollection<Complaint>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var documents = await context.Complaints.Find(FilterDefinition<ComplaintDocument>.Empty)
            .SortByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        return documents.Select(MapComplaint).ToArray();
    }

    public async Task<IReadOnlyCollection<Complaint>> GetAllAsync(ComplaintQueryDto query, CancellationToken cancellationToken = default)
    {
        var filter = BuildFilter(query);
        var sort = BuildSort(query);

        var documents = await context.Complaints
            .Find(filter)
            .Sort(sort)
            .ToListAsync(cancellationToken);

        return documents.Select(MapComplaint).ToArray();
    }

    public async Task<Complaint?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var document = await context.Complaints.Find(x => x.Id == id).FirstOrDefaultAsync(cancellationToken);
        return document is null ? null : MapComplaint(document);
    }

    public Task UpdateAsync(Complaint complaint, CancellationToken cancellationToken = default)
    {
        complaint.UpdatedAtUtc = DateTime.UtcNow;
        var document = MapComplaint(complaint);
        return context.Complaints.ReplaceOneAsync(x => x.Id == complaint.Id, document, cancellationToken: cancellationToken);
    }

    public Task DeleteAsync(string id, CancellationToken cancellationToken = default) =>
        context.Complaints.DeleteOneAsync(x => x.Id == id, cancellationToken);

    public Task UpdateStatusAsync(string id, ComplaintStatus status, CancellationToken cancellationToken = default)
    {
        var update = Builders<ComplaintDocument>.Update
            .Set(x => x.Status, status)
            .Set(x => x.UpdatedAtUtc, DateTime.UtcNow);

        return context.Complaints.UpdateOneAsync(x => x.Id == id, update, cancellationToken: cancellationToken);
    }

    public Task AddCommentAsync(string id, ComplaintComment comment, CancellationToken cancellationToken = default)
    {
        var update = Builders<ComplaintDocument>.Update
            .Push(x => x.Comments, new ComplaintCommentDocument
            {
                Id = comment.Id,
                AuthorUserName = comment.AuthorUserName,
                Message = comment.Message,
                CreatedAtUtc = comment.CreatedAtUtc
            })
            .Set(x => x.UpdatedAtUtc, DateTime.UtcNow);

        return context.Complaints.UpdateOneAsync(x => x.Id == id, update, cancellationToken: cancellationToken);
    }

    private static FilterDefinition<ComplaintDocument> BuildFilter(ComplaintQueryDto query)
    {
        var filters = new List<FilterDefinition<ComplaintDocument>>();
        var builder = Builders<ComplaintDocument>.Filter;

        if (!query.IncludeArchived)
        {
            filters.Add(builder.Or(
                builder.Eq(x => x.IsArchived, false),
                builder.Exists(nameof(ComplaintDocument.IsArchived), false)));
        }

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var escaped = Regex.Escape(query.SearchTerm.Trim());
            var searchFilter = builder.Or(
                builder.Regex(x => x.Title, new MongoDB.Bson.BsonRegularExpression(escaped, "i")),
                builder.Regex(x => x.CustomerName, new MongoDB.Bson.BsonRegularExpression(escaped, "i")),
                builder.Regex(x => x.CustomerEmail, new MongoDB.Bson.BsonRegularExpression(escaped, "i")),
                builder.Regex(x => x.Category, new MongoDB.Bson.BsonRegularExpression(escaped, "i")));
            filters.Add(searchFilter);
        }

        if (query.Status.HasValue)
        {
            filters.Add(builder.Eq(x => x.Status, query.Status.Value));
        }

        if (query.Sentiment.HasValue)
        {
            filters.Add(builder.Eq(x => x.Sentiment, query.Sentiment.Value));
        }

        if (query.Urgency.HasValue)
        {
            filters.Add(builder.Eq(x => x.Urgency, query.Urgency.Value));
        }

        if (query.EscalationStatus.HasValue)
        {
            filters.Add(builder.Eq(x => x.EscalationStatus, query.EscalationStatus.Value));
        }

        return filters.Count == 0
            ? builder.Empty
            : builder.And(filters);
    }

    private static SortDefinition<ComplaintDocument> BuildSort(ComplaintQueryDto query)
    {
        var builder = Builders<ComplaintDocument>.Sort;

        return query.IncludeArchived
            ? builder.Descending(x => x.CreatedAtUtc)
            : builder.Ascending(x => x.IsArchived).Descending(x => x.CreatedAtUtc);
    }

    private static ComplaintDocument MapComplaint(Complaint complaint) =>
        new()
        {
            Id = complaint.Id,
            Title = complaint.Title,
            Description = complaint.Description,
            CustomerName = complaint.CustomerName,
            CustomerEmail = complaint.CustomerEmail,
            CreatedByUserName = complaint.CreatedByUserName,
            Status = complaint.Status,
            Sentiment = complaint.Sentiment,
            Category = complaint.Category,
            Urgency = complaint.Urgency,
            Confidence = complaint.Confidence,
            RequiresAction = complaint.RequiresAction,
            SuggestedActions = complaint.SuggestedActions.Select(x => new ComplaintActionItemDocument
            {
                Owner = x.Owner,
                Action = x.Action
            }).ToList(),
            AssignedOwner = complaint.AssignedOwner,
            EscalationStatus = complaint.EscalationStatus,
            EscalationReason = complaint.EscalationReason,
            IsArchived = complaint.IsArchived,
            ArchivedAtUtc = complaint.ArchivedAtUtc,
            ArchivedByUserName = complaint.ArchivedByUserName,
            CreatedAtUtc = complaint.CreatedAtUtc,
            UpdatedAtUtc = complaint.UpdatedAtUtc,
            Comments = complaint.Comments.Select(x => new ComplaintCommentDocument
            {
                Id = x.Id,
                AuthorUserName = x.AuthorUserName,
                Message = x.Message,
                CreatedAtUtc = x.CreatedAtUtc
            }).ToList()
        };

    private static Complaint MapComplaint(ComplaintDocument document) =>
        new()
        {
            Id = document.Id,
            Title = document.Title,
            Description = document.Description,
            CustomerName = document.CustomerName,
            CustomerEmail = document.CustomerEmail,
            CreatedByUserName = document.CreatedByUserName,
            Status = document.Status,
            Sentiment = document.Sentiment,
            Category = document.Category,
            Urgency = document.Urgency,
            Confidence = document.Confidence,
            RequiresAction = document.RequiresAction,
            SuggestedActions = document.SuggestedActions.Select(x => new ComplaintActionItem
            {
                Owner = x.Owner,
                Action = x.Action
            }).ToList(),
            AssignedOwner = document.AssignedOwner,
            EscalationStatus = document.EscalationStatus,
            EscalationReason = document.EscalationReason,
            IsArchived = document.IsArchived,
            ArchivedAtUtc = document.ArchivedAtUtc,
            ArchivedByUserName = document.ArchivedByUserName,
            CreatedAtUtc = document.CreatedAtUtc,
            UpdatedAtUtc = document.UpdatedAtUtc,
            Comments = document.Comments.Select(x => new ComplaintComment
            {
                Id = x.Id,
                AuthorUserName = x.AuthorUserName,
                Message = x.Message,
                CreatedAtUtc = x.CreatedAtUtc
            }).ToList()
        };
}
