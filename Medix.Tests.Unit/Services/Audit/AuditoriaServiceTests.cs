using Medix.Data.Mongo;
using Medix.Models.Audit;
using Medix.Services.Audit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using MongoDB.Driver;

namespace Medix.Tests.Unit.Services.Audit;

public class AuditoriaServiceTests
{
    // --- helpers ---

    private static Mock<IMongoCollection<LogAuditoria>> CriarCollectionMock()
    {
        var mock = new Mock<IMongoCollection<LogAuditoria>>();
        return mock;
    }

    private static IAuditoriaService CriarService(
        Mock<IMongoCollection<LogAuditoria>>? collectionMock = null)
    {
        var col = collectionMock ?? CriarCollectionMock();

        var mongoContextMock = new Mock<MongoDbContext>(
            Options.Create(new MongoDbSettings
            {
                ConnectionString = "mongodb://localhost:27017",
                DatabaseName = "TestDb",
                LogsCollectionName = "LogsAuditoria"
            }));

        mongoContextMock.Setup(m => m.LogsAuditoria).Returns(col.Object);

        var logger = new Mock<ILogger<AuditoriaService>>().Object;
        return new AuditoriaService(mongoContextMock.Object, logger);
    }

    // ── RegistrarAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task RegistrarAsync_QuandoChamado_DeveChamarInsertOneAsync()
    {
        // Arrange
        var col = CriarCollectionMock();
        col.Setup(c => c.InsertOneAsync(
                It.IsAny<LogAuditoria>(),
                It.IsAny<InsertOneOptions?>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var service = CriarService(col);

        // Act
        await service.RegistrarAsync(
            entidade: "UnidadeMedica",
            entidadeId: 1,
            operacao: "CREATE",
            realizadoPor: "admin@medix.com",
            detalhe: new Dictionary<string, object?> { ["nome"] = "Hospital Teste" });

        // Assert
        col.Verify(c => c.InsertOneAsync(
            It.Is<LogAuditoria>(l =>
                l.Entidade == "UnidadeMedica" &&
                l.EntidadeId == 1 &&
                l.Operacao == "CREATE" &&
                l.RealizadoPor == "admin@medix.com"),
            It.IsAny<InsertOneOptions?>(),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RegistrarAsync_QuandoMongoFalha_NaoDeveLancarExcecao()
    {
        // Arrange — simula falha no MongoDB
        var col = CriarCollectionMock();
        col.Setup(c => c.InsertOneAsync(
                It.IsAny<LogAuditoria>(),
                It.IsAny<InsertOneOptions?>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new MongoException("conexão recusada"));

        var service = CriarService(col);

        // Act & Assert — auditoria não deve quebrar o fluxo principal
        var ex = await Record.ExceptionAsync(() => service.RegistrarAsync(
            "Paciente", 99, "DELETE", "user@test.com",
            new Dictionary<string, object?>()));

        Assert.Null(ex);
    }

    [Fact]
    public async Task RegistrarAsync_QuandoChamado_DevePreencherRealizadoEmComDataAtual()
    {
        // Arrange
        LogAuditoria? capturado = null;
        var col = CriarCollectionMock();
        col.Setup(c => c.InsertOneAsync(
                It.IsAny<LogAuditoria>(),
                It.IsAny<InsertOneOptions?>(),
                It.IsAny<CancellationToken>()))
            .Callback<LogAuditoria, InsertOneOptions?, CancellationToken>(
                (log, _, _) => capturado = log)
            .Returns(Task.CompletedTask);

        var service = CriarService(col);
        var antes = DateTime.UtcNow;

        // Act
        await service.RegistrarAsync("Colaborador", 5, "UPDATE", "admin@medix.com",
            new Dictionary<string, object?>());

        // Assert
        Assert.NotNull(capturado);
        Assert.True(capturado!.RealizadoEm >= antes);
        Assert.True(capturado.RealizadoEm <= DateTime.UtcNow);
    }

    // ── LogAuditoria (modelo) ─────────────────────────────────────────────

    [Fact]
    public void LogAuditoria_QuandoCriado_DeveInicializarDetalheVazio()
    {
        // Arrange & Act
        var log = new LogAuditoria();

        // Assert
        Assert.NotNull(log.Detalhe);
        Assert.Empty(log.Detalhe);
    }

    [Fact]
    public void LogAuditoria_QuandoCriado_RealizadoEmDeveSerUtc()
    {
        // Arrange & Act
        var antes = DateTime.UtcNow;
        var log = new LogAuditoria();

        // Assert
        Assert.Equal(DateTimeKind.Utc, log.RealizadoEm.Kind);
        Assert.True(log.RealizadoEm >= antes);
    }

    [Fact]
    public void LogAuditoria_QuandoDetalhesPreenchidos_DeveManterValores()
    {
        // Arrange & Act
        var log = new LogAuditoria
        {
            Entidade = "UnidadeMedica",
            EntidadeId = 42,
            Operacao = "CREATE",
            RealizadoPor = "admin@medix.com",
            Detalhe = new Dictionary<string, object?>
            {
                ["nome"] = "Hospital Central",
                ["status"] = "Ativa"
            }
        };

        // Assert
        Assert.Equal("UnidadeMedica", log.Entidade);
        Assert.Equal(42, log.EntidadeId);
        Assert.Equal("CREATE", log.Operacao);
        Assert.Equal("Hospital Central", log.Detalhe["nome"]);
        Assert.Equal("Ativa", log.Detalhe["status"]);
    }
}
