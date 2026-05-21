using Medix.Data;
using Medix.Models;
using Medix.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Medix.Tests.Unit.Repositories;

/// <summary>
/// Testes unitários dos repositórios usando EF Core InMemory.
/// Padrão AAA (Arrange / Act / Assert) em todos os casos.
/// </summary>
public class RepositoryTests : IDisposable
{
    // ── Infraestrutura compartilhada ─────────────────────────────────────

    private readonly ApplicationDbContext _context;
    private readonly UnidadeMedicaRepository _unidadeRepo;
    private readonly PacienteRepository _pacienteRepo;
    private readonly ColaboradorRepository _colaboradorRepo;

    public RepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()) // banco isolado por teste
            .Options;

        _context = new ApplicationDbContext(options);
        _unidadeRepo = new UnidadeMedicaRepository(_context);
        _pacienteRepo = new PacienteRepository(_context);
        _colaboradorRepo = new ColaboradorRepository(_context);
    }

    public void Dispose() => _context.Dispose();

    // ── Helpers ──────────────────────────────────────────────────────────

    private async Task<UnidadeMedica> CriarUnidadeAsync(string nome = "Hospital Teste")
    {
        var u = new UnidadeMedica
        {
            Nome = nome,
            CNPJ = $"{Random.Shared.Next(10000000, 99999999):D8}/0001-00",
            EmailAdmin = "admin@test.com",
            Status = StatusUnidade.Ativa,
            DataCadastro = DateTime.UtcNow
        };
        await _unidadeRepo.AdicionarAsync(u);
        await _unidadeRepo.SalvarAsync();
        return u;
    }

    private async Task<Paciente> CriarPacienteAsync(int unidadeId, string nome = "João Silva")
    {
        var p = new Paciente
        {
            NomeCompleto = nome,
            CPF = $"{Random.Shared.Next(100_000_000, 999_999_999):D9}-00",
            DataNascimento = new DateTime(1990, 1, 1),
            UnidadeMedicaId = unidadeId
        };
        await _pacienteRepo.AdicionarAsync(p);
        await _pacienteRepo.SalvarAsync();
        return p;
    }

    private async Task<Colaborador> CriarColaboradorAsync(int unidadeId, string nome = "Dra. Maria")
    {
        var c = new Colaborador
        {
            NomeCompleto = nome,
            Email = "maria@test.com",
            Cargo = TipoColaborador.Medico,
            UnidadeMedicaId = unidadeId
        };
        await _colaboradorRepo.AdicionarAsync(c);
        await _colaboradorRepo.SalvarAsync();
        return c;
    }

    // ══════════════════════════════════════════════════════════════════
    // UnidadeMedicaRepository
    // ══════════════════════════════════════════════════════════════════

    [Fact]
    public async Task UnidadeRepo_AdicionarEObterPorId_DeveRetornarEntidade()
    {
        // Arrange
        var unidade = await CriarUnidadeAsync("Hospital Central");

        // Act
        var resultado = await _unidadeRepo.ObterPorIdAsync(unidade.Id);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal("Hospital Central", resultado!.Nome);
    }

    [Fact]
    public async Task UnidadeRepo_ObterPorIdInexistente_DeveRetornarNull()
    {
        // Arrange — banco vazio

        // Act
        var resultado = await _unidadeRepo.ObterPorIdAsync(9999);

        // Assert
        Assert.Null(resultado);
    }

    [Fact]
    public async Task UnidadeRepo_Buscar_DeveRetornarPaginado()
    {
        // Arrange
        await CriarUnidadeAsync("Alpha");
        await CriarUnidadeAsync("Beta");
        await CriarUnidadeAsync("Gamma");

        // Act
        var (items, total) = await _unidadeRepo.BuscarAsync(
            nome: null, status: null,
            sortBy: "Nome", sortDirection: "ASC",
            pagina: 1, tamanhoPagina: 2);

        // Assert
        Assert.Equal(3, total);
        Assert.Equal(2, items.Count());
    }

    [Fact]
    public async Task UnidadeRepo_Buscar_ComFiltroNome_DeveRetornarApenasCombinantes()
    {
        // Arrange
        await CriarUnidadeAsync("Hospital A");
        await CriarUnidadeAsync("Clínica B");
        await CriarUnidadeAsync("Hospital C");

        // Act
        var (items, total) = await _unidadeRepo.BuscarAsync(
            nome: "Hospital", status: null,
            sortBy: "Nome", sortDirection: "ASC",
            pagina: 1, tamanhoPagina: 10);

        // Assert
        Assert.Equal(2, total);
        Assert.All(items, u => Assert.Contains("Hospital", u.Nome));
    }

    [Fact]
    public async Task UnidadeRepo_Buscar_ComFiltroStatus_DeveRetornarApenasAtivas()
    {
        // Arrange
        await CriarUnidadeAsync("Ativa");
        var inativa = await CriarUnidadeAsync("Inativa");
        inativa.Status = StatusUnidade.Inativa;
        _unidadeRepo.Atualizar(inativa);
        await _unidadeRepo.SalvarAsync();

        // Act
        var (items, total) = await _unidadeRepo.BuscarAsync(
            nome: null, status: StatusUnidade.Ativa,
            sortBy: "Nome", sortDirection: "ASC",
            pagina: 1, tamanhoPagina: 10);

        // Assert
        Assert.Equal(1, total);
        Assert.Equal(StatusUnidade.Ativa, items.First().Status);
    }

    [Fact]
    public async Task UnidadeRepo_Atualizar_DevePersistitrMudancas()
    {
        // Arrange
        var unidade = await CriarUnidadeAsync("Nome Original");

        // Act
        unidade.Nome = "Nome Atualizado";
        _unidadeRepo.Atualizar(unidade);
        await _unidadeRepo.SalvarAsync();

        var resultado = await _unidadeRepo.ObterPorIdAsync(unidade.Id);

        // Assert
        Assert.Equal("Nome Atualizado", resultado!.Nome);
    }

    [Fact]
    public async Task UnidadeRepo_Remover_DeveExcluirEntidade()
    {
        // Arrange
        var unidade = await CriarUnidadeAsync();

        // Act
        _unidadeRepo.Remover(unidade);
        await _unidadeRepo.SalvarAsync();

        var resultado = await _unidadeRepo.ObterPorIdAsync(unidade.Id);

        // Assert
        Assert.Null(resultado);
    }

    [Fact]
    public async Task UnidadeRepo_ExisteAsync_DeveRetornarTrueQuandoExiste()
    {
        // Arrange
        var unidade = await CriarUnidadeAsync();

        // Act
        var existe = await _unidadeRepo.ExisteAsync(u => u.Id == unidade.Id);

        // Assert
        Assert.True(existe);
    }

    [Fact]
    public async Task UnidadeRepo_ExisteAsync_DeveRetornarFalseQuandoNaoExiste()
    {
        // Arrange — banco vazio

        // Act
        var existe = await _unidadeRepo.ExisteAsync(u => u.Id == 9999);

        // Assert
        Assert.False(existe);
    }

    // ══════════════════════════════════════════════════════════════════
    // PacienteRepository
    // ══════════════════════════════════════════════════════════════════

    [Fact]
    public async Task PacienteRepo_AdicionarEObterPorId_DeveRetornarEntidade()
    {
        // Arrange
        var unidade = await CriarUnidadeAsync();
        var paciente = await CriarPacienteAsync(unidade.Id, "Ana Paula");

        // Act
        var resultado = await _pacienteRepo.ObterPorIdAsync(paciente.Id);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal("Ana Paula", resultado!.NomeCompleto);
    }

    [Fact]
    public async Task PacienteRepo_BuscarPorUnidade_DeveRetornarApenasDaUnidade()
    {
        // Arrange
        var unidade1 = await CriarUnidadeAsync("U1");
        var unidade2 = await CriarUnidadeAsync("U2");

        await CriarPacienteAsync(unidade1.Id, "Paciente A");
        await CriarPacienteAsync(unidade1.Id, "Paciente B");
        await CriarPacienteAsync(unidade2.Id, "Paciente C");

        // Act
        var (items, total) = await _pacienteRepo.BuscarPorUnidadeAsync(
            unidade1.Id, nome: null, pagina: 1, tamanhoPagina: 10);

        // Assert
        Assert.Equal(2, total);
        Assert.All(items, p => Assert.Equal(unidade1.Id, p.UnidadeMedicaId));
    }

    [Fact]
    public async Task PacienteRepo_BuscarPorUnidade_ComFiltroNome_DeveRetornarCombinantes()
    {
        // Arrange
        var unidade = await CriarUnidadeAsync();
        await CriarPacienteAsync(unidade.Id, "Carlos Lima");
        await CriarPacienteAsync(unidade.Id, "Carla Santos");
        await CriarPacienteAsync(unidade.Id, "Pedro Souza");

        // Act
        var (items, total) = await _pacienteRepo.BuscarPorUnidadeAsync(
            unidade.Id, nome: "Car", pagina: 1, tamanhoPagina: 10);

        // Assert
        Assert.Equal(2, total);
        Assert.All(items, p => Assert.Contains("Car", p.NomeCompleto));
    }

    [Fact]
    public async Task PacienteRepo_ObterPorUnidadeEId_DeveRetornarNull_QuandoUnidadeErrada()
    {
        // Arrange
        var unidade1 = await CriarUnidadeAsync("U1");
        var unidade2 = await CriarUnidadeAsync("U2");
        var paciente = await CriarPacienteAsync(unidade1.Id);

        // Act
        var resultado = await _pacienteRepo.ObterPorUnidadeEIdAsync(unidade2.Id, paciente.Id);

        // Assert
        Assert.Null(resultado);
    }

    // ══════════════════════════════════════════════════════════════════
    // ColaboradorRepository
    // ══════════════════════════════════════════════════════════════════

    [Fact]
    public async Task ColaboradorRepo_AdicionarEObterPorId_DeveRetornarEntidade()
    {
        // Arrange
        var unidade = await CriarUnidadeAsync();
        var colaborador = await CriarColaboradorAsync(unidade.Id, "Dr. Roberto");

        // Act
        var resultado = await _colaboradorRepo.ObterPorIdAsync(colaborador.Id);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal("Dr. Roberto", resultado!.NomeCompleto);
    }

    [Fact]
    public async Task ColaboradorRepo_BuscarPorUnidade_ComFiltroCargo_DeveRetornarApenasMedicos()
    {
        // Arrange
        var unidade = await CriarUnidadeAsync();

        await _colaboradorRepo.AdicionarAsync(new Colaborador
        {
            NomeCompleto = "Dr. Medico",
            Email = "medico@test.com",
            Cargo = TipoColaborador.Medico,
            UnidadeMedicaId = unidade.Id
        });
        await _colaboradorRepo.AdicionarAsync(new Colaborador
        {
            NomeCompleto = "Enf. Enfermeiro",
            Email = "enf@test.com",
            Cargo = TipoColaborador.Enfermeiro,
            UnidadeMedicaId = unidade.Id
        });
        await _colaboradorRepo.SalvarAsync();

        // Act
        var (items, total) = await _colaboradorRepo.BuscarPorUnidadeAsync(
            unidade.Id, nome: null, cargo: TipoColaborador.Medico,
            pagina: 1, tamanhoPagina: 10);

        // Assert
        Assert.Equal(1, total);
        Assert.All(items, c => Assert.Equal(TipoColaborador.Medico, c.Cargo));
    }

    [Fact]
    public async Task ColaboradorRepo_BuscarPorUnidade_DeveRetornarApenasDaUnidade()
    {
        // Arrange
        var unidade1 = await CriarUnidadeAsync("U1");
        var unidade2 = await CriarUnidadeAsync("U2");

        await CriarColaboradorAsync(unidade1.Id, "Colaborador A");
        await CriarColaboradorAsync(unidade2.Id, "Colaborador B");

        // Act
        var (items, total) = await _colaboradorRepo.BuscarPorUnidadeAsync(
            unidade1.Id, nome: null, cargo: null, pagina: 1, tamanhoPagina: 10);

        // Assert
        Assert.Equal(1, total);
        Assert.Equal(unidade1.Id, items.First().UnidadeMedicaId);
    }

    [Fact]
    public async Task ColaboradorRepo_ObterPorUnidadeEId_DeveRetornarNull_QuandoUnidadeErrada()
    {
        // Arrange
        var unidade1 = await CriarUnidadeAsync("U1");
        var unidade2 = await CriarUnidadeAsync("U2");
        var colaborador = await CriarColaboradorAsync(unidade1.Id);

        // Act
        var resultado = await _colaboradorRepo.ObterPorUnidadeEIdAsync(unidade2.Id, colaborador.Id);

        // Assert
        Assert.Null(resultado);
    }

    // ══════════════════════════════════════════════════════════════════
    // IRepository<T> genérico — via UnidadeMedicaRepository
    // ══════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Repository_ObterTodosAsync_DeveRetornarTodasEntidades()
    {
        // Arrange
        await CriarUnidadeAsync("A");
        await CriarUnidadeAsync("B");

        // Act
        var todas = await _unidadeRepo.ObterTodosAsync();

        // Assert
        Assert.Equal(2, todas.Count());
    }

    [Fact]
    public async Task Repository_BuscarAsync_ComFiltroExpression_DeveFiltrar()
    {
        // Arrange
        await CriarUnidadeAsync("Hospital A");
        await CriarUnidadeAsync("Clínica B");

        // Act
        var (items, total) = await _unidadeRepo.BuscarAsync(
            filtro: u => u.Nome.Contains("Clínica"),
            ordenacao: q => q.OrderBy(u => u.Nome),
            pagina: 1,
            tamanhoPagina: 10);

        // Assert
        Assert.Equal(1, total);
        Assert.Contains("Clínica", items.First().Nome);
    }
}
