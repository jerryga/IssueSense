using IssueSense.Infrastructure.Configuration;
using IssueSense.Infrastructure.Documents;
using MongoDB.Driver;

namespace IssueSense.Infrastructure.Contexts;

public sealed class MongoDbContext
{
    private readonly IMongoDatabase _database;
    private readonly MongoDbSettings _settings;

    public MongoDbContext(MongoDbSettings settings)
    {
        _settings = settings;
        var client = new MongoClient(settings.ConnectionString);
        _database = client.GetDatabase(settings.DatabaseName);
    }

    public IMongoCollection<ComplaintDocument> Complaints =>
        _database.GetCollection<ComplaintDocument>(_settings.ComplaintsCollectionName);

    public IMongoCollection<UserDocument> Users =>
        _database.GetCollection<UserDocument>(_settings.UsersCollectionName);

    public IMongoCollection<TDocument> GetCollection<TDocument>(string collectionName) =>
        _database.GetCollection<TDocument>(collectionName);

    public async Task EnsureIndexesAsync(CancellationToken cancellationToken = default)
    {
        var complaintIndexModels = new[]
        {
            new CreateIndexModel<ComplaintDocument>(
                Builders<ComplaintDocument>.IndexKeys.Descending(x => x.CreatedAtUtc)),
            new CreateIndexModel<ComplaintDocument>(
                Builders<ComplaintDocument>.IndexKeys
                    .Ascending(x => x.IsArchived)
                    .Descending(x => x.CreatedAtUtc)),
            new CreateIndexModel<ComplaintDocument>(
                Builders<ComplaintDocument>.IndexKeys
                    .Ascending(x => x.Status)
                    .Ascending(x => x.IsArchived)),
            new CreateIndexModel<ComplaintDocument>(
                Builders<ComplaintDocument>.IndexKeys
                    .Ascending(x => x.EscalationStatus)
                    .Ascending(x => x.IsArchived)),
            new CreateIndexModel<ComplaintDocument>(
                Builders<ComplaintDocument>.IndexKeys.Ascending(x => x.AssignedOwner))
        };

        var userIndexModels = new[]
        {
            new CreateIndexModel<UserDocument>(
                Builders<UserDocument>.IndexKeys.Ascending(x => x.UserName),
                new CreateIndexOptions { Unique = true })
        };

        await Complaints.Indexes.CreateManyAsync(complaintIndexModels, cancellationToken);
        await Users.Indexes.CreateManyAsync(userIndexModels, cancellationToken);
    }
}
