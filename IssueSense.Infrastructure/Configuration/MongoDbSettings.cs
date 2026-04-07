namespace IssueSense.Infrastructure.Configuration;

public sealed class MongoDbSettings
{
    public const string SectionName = "MongoDb";

    public string ConnectionString { get; set; } = "mongodb://localhost:27017";
    public string DatabaseName { get; set; } = "IssueSenseDb";
    public string ComplaintsCollectionName { get; set; } = "complaints";
    public string UsersCollectionName { get; set; } = "users";
}
