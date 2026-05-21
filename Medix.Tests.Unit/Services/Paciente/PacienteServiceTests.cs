using Medix.Models;
using Medix.Repositories;
using Medix.Services.Paciente;
using Moq;
using PacienteModel = Medix.Models.Paciente;

namespace Medix.Tests.Unit.Services.Paciente;

public class PacienteServiceTests
{
    private static (PacienteService service, Mock<IPacienteRepository> pacienteRepo, Mock<IUnidadeMedicaRepository> unidadeRepo)
        CriarService()
    {
        var pacienteRepo = new Mock<IPacienteRepository>();
        var unidadeRepo = new Mock<IUnidadeMedicaRepository>();
        var service = new PacienteService(pacienteRepo.Object, unidadeRepo.Object);
        return (service, pacienteRepo, unidadeRepo);
    }

    // ── UnidadeExisteAsync ────────────────────────────────────────────────

    [Fact]
    public async Task UnidadeExisteAsync_QuandoUnidadeExiste_DeveRetornarTrue()
    {
        // Arrange
        var (service, _, unidadeRepo) = CriarService();
        unidadeRepo.Setup(r => r.ExisteAsync(It.IsAny<System.Linq.Expressions.Expression<Func<UnidadeMedica, bool>>>()))
                   .ReturnsAsync(true);

        // Act
        var resultado = await service.UnidadeExisteAsync(1);

        // Assert
        Assert.True(resultado);
    }

    [Fact]
    public async Task UnidadeExisteAsync_QuandoUnidadeNaoExiste_DeveRetornarFalse()
    {
        // Arrange
        var (service, _, unidadeRepo) = CriarService();
        unidadeRepo.Setup(r => r.ExisteAsync(It.IsAny<System.Linq.Expressions.Expression<Func<UnidadeMedica, bool>>>()))
                   .ReturnsAsync(false);

        // Act
        var resultado = await service.UnidadeExisteAsync(999);

        // Assert
        Assert.False(resultado);
    }

    // ── BuscarPorIdAsync ──────────────────────────────────────────────────

    [Fact]
    public async Task BuscarPorIdAsync_QuandoPacienteExiste_DeveRetornarPaciente()
    {
        // Arrange
        var (service, pacienteRepo, _) = CriarService();
        var paciente = new PacienteModel { Id = 1, NomeCompleto = "Ana Lima", UnidadeMedicaId = 1 };
        pacienteRepo.Setup(r => r.ObterPorUnidadeEIdAsync(1, 1)).ReturnsAsync(paciente);

        // Act
        var resultado = await service.BuscarPorIdAsync(1, 1);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal("Ana Lima", resultado.NomeCompleto);
    }

    [Fact]
    public async Task BuscarPorIdAsync_QuandoPacienteNaoExiste_DeveRetornarNull()
    {
        // Arrange
        var (service, pacienteRepo, _) = CriarService();
        pacienteRepo.Setup(r => r.ObterPorUnidadeEIdAsync(1, 99)).ReturnsAsync((PacienteModel?)null);

        // Act
        var resultado = await service.BuscarPorIdAsync(1, 99);

        // Assert
        Assert.Null(resultado);
    }

    // ── CriarAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task CriarAsync_QuandoChamado_DeveAdicionarESalvar()
    {
        // Arrange
        var (service, pacienteRepo, _) = CriarService();
        pacienteRepo.Setup(r => r.AdicionarAsync(It.IsAny<PacienteModel>())).Returns(Task.CompletedTask);
        pacienteRepo.Setup(r => r.SalvarAsync()).Returns(Task.CompletedTask);

        // Act
        var resultado = await service.CriarAsync(
            1, "Carlos Silva", "123.456.789-00",
            new DateTime(1990, 5, 10), "carlos@email.com", null, null);

        // Assert
        Assert.Equal("Carlos Silva", resultado.NomeCompleto);
        Assert.Equal(1, resultado.UnidadeMedicaId);
        pacienteRepo.Verify(r => r.AdicionarAsync(It.IsAny<PacienteModel>()), Times.Once);
        pacienteRepo.Verify(r => r.SalvarAsync(), Times.Once);
    }

    // ── ExcluirAsync ──────────────────────────────────────────────────────

    [Fact]
    public async Task ExcluirAsync_QuandoPacienteNaoExiste_DeveRetornarFalse()
    {
        // Arrange
        var (service, pacienteRepo, _) = CriarService();
        pacienteRepo.Setup(r => r.ObterPorUnidadeEIdAsync(1, 99)).ReturnsAsync((PacienteModel?)null);

        // Act
        var resultado = await service.ExcluirAsync(1, 99);

        // Assert
        Assert.False(resultado);
        pacienteRepo.Verify(r => r.SalvarAsync(), Times.Never);
    }

    [Fact]
    public async Task ExcluirAsync_QuandoPacienteExiste_DeveRemoverESalvar()
    {
        // Arrange
        var (service, pacienteRepo, _) = CriarService();
        var paciente = new PacienteModel { Id = 1, UnidadeMedicaId = 1 };
        pacienteRepo.Setup(r => r.ObterPorUnidadeEIdAsync(1, 1)).ReturnsAsync(paciente);
        pacienteRepo.Setup(r => r.SalvarAsync()).Returns(Task.CompletedTask);

        // Act
        var resultado = await service.ExcluirAsync(1, 1);

        // Assert
        Assert.True(resultado);
        pacienteRepo.Verify(r => r.Remover(paciente), Times.Once);
        pacienteRepo.Verify(r => r.SalvarAsync(), Times.Once);
    }
}