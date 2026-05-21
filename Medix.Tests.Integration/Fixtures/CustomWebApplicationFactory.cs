using Medix.Data;
using Medix.Data.Mongo;
using Medix.Models;
using Medix.Models.Audit;
using Medix.Services.Audit;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Moq;

namespace Medix.Tests.Integration.Fixtures;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // --- EF Core: substituir Oracle por InMemory ---
            var dbDescriptors = services
                .Where(d =>
                    d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>) ||
                    d.ServiceType == typeof(ApplicationDbContext))
                .ToList();
            foreach (var d in dbDescriptors)
                services.Remove(d);

            var internalConfigs = services
                .Where(d => d.ServiceType.FullName?.Contains("DbContextOptionsConfiguration") == true)
                .ToList();
            foreach (var d in internalConfigs)
                services.Remove(d);

            var inMemorySp = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            services.AddDbContext<ApplicationDbContext>(options =>
                options
                    .UseInMemoryDatabase("MedixTestDb")
                    .UseInternalServiceProvider(inMemorySp));

            // --- MongoDB: substituir MongoDbContext e IAuditoriaService por mocks ---
            var mongoDescriptors = services
                .Where(d =>
                    d.ServiceType == typeof(MongoDbContext) ||
                    d.ServiceType == typeof(IOptions<MongoDbSettings>) ||
                    d.ServiceType == typeof(IAuditoriaService))
                .ToList();
            foreach (var d in mongoDescriptors)
                services.Remove(d);

            // Mock do MongoDbContext — necessário para o MongoDbHealthCheck resolver a dependência
            // A cadeia Database.Client.ListDatabaseNamesAsync precisa estar totalmente configurada
            // para que o health check não lance NullReferenceException em testes.
            var cursorMock = new Mock<IAsyncCursor<string>>();
            cursorMock.Setup(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
                      .ReturnsAsync(false);
            cursorMock.Setup(c => c.Current).Returns(Enumerable.Empty<string>());

            var mongoClientMock = new Mock<IMongoClient>();
            mongoClientMock
                .Setup(c => c.ListDatabaseNamesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(cursorMock.Object);

            var mongoDatabaseMock = new Mock<IMongoDatabase>();
            mongoDatabaseMock.Setup(d => d.Client).Returns(mongoClientMock.Object);

            var mongoCollectionMock = new Mock<IMongoCollection<LogAuditoria>>();
            mongoCollectionMock.Setup(col => col.Database).Returns(mongoDatabaseMock.Object);

            var mongoContextMock = new Mock<MongoDbContext>(
                Options.Create(new MongoDbSettings
                {
                    ConnectionString = "mongodb://localhost:27017",
                    DatabaseName = "TestDb",
                    LogsCollectionName = "LogsAuditoria"
                }));
            mongoContextMock.Setup(m => m.LogsAuditoria).Returns(mongoCollectionMock.Object);
            services.AddSingleton(_ => mongoContextMock.Object);

            // Mock do IAuditoriaService — não testa Mongo nos testes de integração da API
            var auditoriaServiceMock = new Mock<IAuditoriaService>();
            auditoriaServiceMock
                .Setup(s => s.RegistrarAsync(
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<Dictionary<string, object?>>()))
                .Returns(Task.CompletedTask);

            auditoriaServiceMock
                .Setup(s => s.ObterRecentesAsync(It.IsAny<int>()))
                .ReturnsAsync(new List<LogAuditoria>());

            auditoriaServiceMock
                .Setup(s => s.ObterPorEntidadeAsync(It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(new List<LogAuditoria>());

            services.AddScoped(_ => auditoriaServiceMock.Object);

            // --- Autenticação fake ---
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "TestScheme";
                options.DefaultChallengeScheme = "TestScheme";
                options.DefaultScheme = "TestScheme";
            })
            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("TestScheme", _ => { });

            // --- Seed de dados de teste ---
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Database.EnsureCreated();
            SeedTestData(db);
        });

        builder.UseEnvironment("Test");
    }

    private static void SeedTestData(ApplicationDbContext db)
    {
        if (db.UnidadesMedicas.Any()) return;

        db.UnidadesMedicas.AddRange(
            new UnidadeMedica
            {
                Nome = "Hospital Teste Alpha",
                CNPJ = "11.111.111/0001-11",
                EmailAdmin = "alpha@test.com",
                Status = StatusUnidade.Ativa,
                DataCadastro = new DateTime(2024, 1, 1)
            },
            new UnidadeMedica
            {
                Nome = "Clínica Beta",
                CNPJ = "22.222.222/0001-22",
                EmailAdmin = "beta@test.com",
                Status = StatusUnidade.Inativa,
                DataCadastro = new DateTime(2024, 2, 1)
            },
            new UnidadeMedica
            {
                Nome = "UPA Gama",
                CNPJ = "33.333.333/0001-33",
                EmailAdmin = "gama@test.com",
                Status = StatusUnidade.Ativa,
                DataCadastro = new DateTime(2024, 3, 1)
            }
        );
        db.SaveChanges();
    }
}