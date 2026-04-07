namespace Medix.Tests.Integration.Fixtures;

[CollectionDefinition("Integration")]
public class IntegrationTestCollection : ICollectionFixture<CustomWebApplicationFactory>
{
    // Classe marcadora — xUnit usa isso para injetar a factory compartilhada entre classes de teste
}
