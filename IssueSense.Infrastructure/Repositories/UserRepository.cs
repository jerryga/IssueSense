using IssueSense.Application.Interfaces.Repositories;
using IssueSense.Domain.Entities;
using IssueSense.Infrastructure.Contexts;
using IssueSense.Infrastructure.Documents;
using MongoDB.Driver;

namespace IssueSense.Infrastructure.Repositories;

public sealed class UserRepository(MongoDbContext context) : IUserRepository
{
    public async Task<IReadOnlyCollection<UserAccount>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var documents = await context.Users.Find(FilterDefinition<UserDocument>.Empty).ToListAsync(cancellationToken);
        return documents.Select(MapUser).ToArray();
    }

    public async Task<UserAccount?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var document = await context.Users.Find(x => x.Id == id).FirstOrDefaultAsync(cancellationToken);
        return document is null ? null : MapUser(document);
    }

    public async Task<UserAccount?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default)
    {
        var document = await context.Users.Find(x => x.UserName == userName).FirstOrDefaultAsync(cancellationToken);
        return document is null ? null : MapUser(document);
    }

    public async Task InsertAsync(UserAccount user, CancellationToken cancellationToken = default)
    {
        var document = MapDocument(user);
        await context.Users.InsertOneAsync(document, cancellationToken: cancellationToken);
        user.Id = document.Id;
    }

    public Task UpdateAsync(UserAccount user, CancellationToken cancellationToken = default)
    {
        user.UserName = user.UserName.Trim().ToLowerInvariant();
        var document = MapDocument(user);
        return context.Users.ReplaceOneAsync(x => x.Id == user.Id, document, cancellationToken: cancellationToken);
    }

    public Task DeleteAsync(string id, CancellationToken cancellationToken = default) =>
        context.Users.DeleteOneAsync(x => x.Id == id, cancellationToken);

    public Task<bool> AnyAsync(CancellationToken cancellationToken = default) =>
        context.Users.Find(FilterDefinition<UserDocument>.Empty).AnyAsync(cancellationToken);

    public async Task CreateManyAsync(IEnumerable<UserAccount> users, CancellationToken cancellationToken = default)
    {
        var documents = users.Select(MapDocument).ToArray();
        await context.Users.InsertManyAsync(documents, cancellationToken: cancellationToken);
    }

    private static UserDocument MapDocument(UserAccount user) =>
        new()
        {
            Id = user.Id,
            UserName = user.UserName.Trim().ToLowerInvariant(),
            PasswordHash = user.PasswordHash,
            Role = user.Role,
            DisplayName = user.DisplayName
        };

    private static UserAccount MapUser(UserDocument document) =>
        new()
        {
            Id = document.Id,
            UserName = document.UserName,
            PasswordHash = document.PasswordHash,
            Role = document.Role,
            DisplayName = document.DisplayName
        };
}
