using Medix.Data;
using Medix.Models;
using Medix.Services;
using Microsoft.EntityFrameworkCore;

namespace Medix.Tests.Unit.Services;

public class UnidadeServiceTests
{
    private static ApplicationDbContext CriarContextoInMemory(string dbName)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new ApplicationDbContext(options);
    }

    private static void SeedUnidades(ApplicationDbContext ctx)
    {
        ctx.UnidadesMedicas.AddRange(
            new UnidadeMedica { Nome = "Hospital Alpha", CNPJ = "11.111.111/0001-11", EmailAdmin = "a@a.com", Status = StatusUnidade.Ativa, DataCadastro = new DateTime(2024, 1, 1) },
            new UnidadeMedica { Nome = "Clínica Beta", CNPJ = "22.222.222/0001-22", EmailAdmin = "b@b.com", Status = StatusUnidade.Inativa, DataCadastro = new DateTime(2024, 2, 1) },
            new UnidadeMedica { Nome = "UPA Gama", CNPJ = "33.333.333/0001-33", EmailAdmin = "g@g.com", Status = StatusUnidade.Ativa, DataCadastro = new DateTime(2024, 3, 1) }
        );
        ctx.SaveChanges();
    }

    [Fact]
    public async Task UnidadeService_BuscarPorFiltro_QuandoFiltroCorresponde_DeveRetornarApenasUnidadesFiltradas()
    {
        // Arrange
        using var ctx = CriarContextoInMemory(nameof(UnidadeService_BuscarPorFiltro_QuandoFiltroCorresponde_DeveRetornarApenasUnidadesFiltradas));
        SeedUnidades(ctx);
        var service = new UnidadeService(ctx);

        // Act
        var resultado = await service.BuscarAsync(nome: "Hospital", status: null, sortBy: "Nome", sortDirection: "ASC", page: 1, pageSize: 10);

        // Assert
        Assert.Single(resultado.Items);
        Assert.Equal("Hospital Alpha", resultado.Items[0].Nome);
    }

    [Fact]
    public async Task UnidadeService_BuscarPorFiltro_QuandoFiltroNaoCorresponde_DeveRetornarListaVazia()
    {
        // Arrange
        using var ctx = CriarContextoInMemory(nameof(UnidadeService_BuscarPorFiltro_QuandoFiltroNaoCorresponde_DeveRetornarListaVazia));
        SeedUnidades(ctx);
        var service = new UnidadeService(ctx);

        // Act
        var resultado = await service.BuscarAsync(nome: "Inexistente", status: null, sortBy: "Nome", sortDirection: "ASC", page: 1, pageSize: 10);

        // Assert
        Assert.Empty(resultado.Items);
        Assert.Equal(0, resultado.TotalCount);
    }

    [Fact]
    public async Task UnidadeService_Buscar_QuandoOrdenacaoDescendente_DeveRetornarOrdemCorreta()
    {
        // Arrange
        using var ctx = CriarContextoInMemory(nameof(UnidadeService_Buscar_QuandoOrdenacaoDescendente_DeveRetornarOrdemCorreta));
        SeedUnidades(ctx);
        var service = new UnidadeService(ctx);

        // Act
        var resultado = await service.BuscarAsync(nome: null, status: null, sortBy: "Nome", sortDirection: "DESC", page: 1, pageSize: 10);

        // Assert
        Assert.Equal(3, resultado.Items.Count);
        Assert.Equal("UPA Gama", resultado.Items[0].Nome);
        Assert.Equal("Clínica Beta", resultado.Items[2].Nome);
    }

    [Fact]
    public async Task UnidadeService_Buscar_QuandoPaginacao_DeveRetornarApenasItemsDaPagina()
    {
        // Arrange
        using var ctx = CriarContextoInMemory(nameof(UnidadeService_Buscar_QuandoPaginacao_DeveRetornarApenasItemsDaPagina));
        SeedUnidades(ctx);
        var service = new UnidadeService(ctx);

        // Act
        var resultado = await service.BuscarAsync(nome: null, status: null, sortBy: "Nome", sortDirection: "ASC", page: 1, pageSize: 2);

        // Assert
        Assert.Equal(2, resultado.Items.Count);
        Assert.Equal(3, resultado.TotalCount);  // total ainda é 3
    }

    [Fact]
    public async Task UnidadeService_Buscar_QuandoPaginaSegunda_DeveRetornarItemRestante()
    {
        // Arrange
        using var ctx = CriarContextoInMemory(nameof(UnidadeService_Buscar_QuandoPaginaSegunda_DeveRetornarItemRestante));
        SeedUnidades(ctx);
        var service = new UnidadeService(ctx);

        // Act
        var resultado = await service.BuscarAsync(nome: null, status: null, sortBy: "Nome", sortDirection: "ASC", page: 2, pageSize: 2);

        // Assert
        Assert.Single(resultado.Items);
    }

    [Fact]
    public async Task UnidadeService_BuscarPorId_QuandoIdExistente_DeveRetornarUnidade()
    {
        // Arrange
        using var ctx = CriarContextoInMemory(nameof(UnidadeService_BuscarPorId_QuandoIdExistente_DeveRetornarUnidade));
        SeedUnidades(ctx);
        var id = ctx.UnidadesMedicas.First().Id;
        var service = new UnidadeService(ctx);

        // Act
        var resultado = await service.BuscarPorIdAsync(id);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(id, resultado!.Id);
    }

    [Fact]
    public async Task UnidadeService_BuscarPorId_QuandoIdInexistente_DeveRetornarNull()
    {
        // Arrange
        using var ctx = CriarContextoInMemory(nameof(UnidadeService_BuscarPorId_QuandoIdInexistente_DeveRetornarNull));
        SeedUnidades(ctx);
        var service = new UnidadeService(ctx);

        // Act
        var resultado = await service.BuscarPorIdAsync(9999);

        // Assert
        Assert.Null(resultado);
    }

    [Fact]
    public async Task UnidadeService_BuscarPorStatus_QuandoStatusInativa_DeveRetornarApenasInativas()
    {
        // Arrange
        using var ctx = CriarContextoInMemory(nameof(UnidadeService_BuscarPorStatus_QuandoStatusInativa_DeveRetornarApenasInativas));
        SeedUnidades(ctx);
        var service = new UnidadeService(ctx);

        // Act
        var resultado = await service.BuscarAsync(nome: null, status: StatusUnidade.Inativa, sortBy: "Nome", sortDirection: "ASC", page: 1, pageSize: 10);

        // Assert
        Assert.Single(resultado.Items);
        Assert.All(resultado.Items, u => Assert.Equal(StatusUnidade.Inativa, u.Status));
    }
}
