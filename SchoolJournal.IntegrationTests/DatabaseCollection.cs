using Xunit;

namespace SchoolJournal.IntegrationTests;

[CollectionDefinition("Database test group")]
public class DatabaseTestGroup : ICollectionFixture<MsSqlFixture>
{

}