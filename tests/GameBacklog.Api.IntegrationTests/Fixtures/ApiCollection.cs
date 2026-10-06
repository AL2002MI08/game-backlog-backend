namespace GameBacklog.Api.IntegrationTests.Fixtures {
    [CollectionDefinition(Name)]
    public class ApiCollection : ICollectionFixture<GameBacklogApplicationFactory>
    {
        public const string Name = "Api";
    }
}
