using Medix.Controllers;
using Medix.Models;
using Medix.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Moq;

namespace Medix.Tests.Unit.Controllers;

public class UnidadesMedicasApiControllerTests
{
    private static UnidadesMedicasApiController CriarController(
        IUnidadeService service,
        LinkGenerator? linkGenerator = null)
    {
        var mockLink = linkGenerator ?? new Mock<LinkGenerator>().Object;
        var controller = new UnidadesMedicasApiController(service, mockLink);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        return controller;
    }

    // ── GetUnidadeMedica (GetById) ────────────────────────────────────────

    [Fact]
    public async Task GetUnidadeMedica_QuandoIdInexistente_DeveRetornar404()
    {
        // Arrange
        var mockService = new Mock<IUnidadeService>();
        mockService
            .Setup(s => s.BuscarPorIdAsync(9999))
            .ReturnsAsync((UnidadeMedica?)null);

        var controller = CriarController(mockService.Object);

        // Act
        var result = await controller.GetUnidadeMedica(9999);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task GetUnidadeMedica_QuandoIdInexistente_DeveChamarServicoComIdCorreto()
    {
        // Arrange
        var mockService = new Mock<IUnidadeService>();
        mockService
            .Setup(s => s.BuscarPorIdAsync(It.IsAny<int>()))
            .ReturnsAsync((UnidadeMedica?)null);

        var controller = CriarController(mockService.Object);

        // Act
        await controller.GetUnidadeMedica(42);

        // Assert
        mockService.Verify(s => s.BuscarPorIdAsync(42), Times.Once);
    }

    [Fact]
    public async Task GetUnidadeMedica_QuandoIdExistente_DeveRetornar200()
    {
        // Arrange
        var unidade = new UnidadeMedica
        {
            Id = 1,
            Nome = "Hospital Mock",
            CNPJ = "00.000.000/0001-00",
            EmailAdmin = "mock@hospital.com",
            Status = StatusUnidade.Ativa,
            DataCadastro = new DateTime(2024, 1, 1)
        };

        var mockService = new Mock<IUnidadeService>();
        mockService
            .Setup(s => s.BuscarPorIdAsync(1))
            .ReturnsAsync(unidade);

        var mockLink = new Mock<LinkGenerator>();
        mockLink
            .Setup(l => l.GetUriByAddress(
                It.IsAny<HttpContext>(),
                It.IsAny<RouteValuesAddress>(),
                It.IsAny<RouteValueDictionary>(),
                It.IsAny<RouteValueDictionary?>(),
                It.IsAny<string?>(),
                It.IsAny<HostString?>(),
                It.IsAny<PathString?>(),
                It.IsAny<FragmentString>(),
                It.IsAny<LinkOptions?>()))
            .Returns("https://test.com/api/unidades/1");

        var controller = CriarController(mockService.Object, mockLink.Object);

        // Act
        var result = await controller.GetUnidadeMedica(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(200, okResult.StatusCode);
    }

    // ── GetUnidadesMedicas (listagem) ─────────────────────────────────────

    [Fact]
    public async Task GetUnidadesMedicas_QuandoChamado_DeveChamarBuscarAsyncUmaVez()
    {
        // Arrange
        var mockService = new Mock<IUnidadeService>();
        mockService
            .Setup(s => s.BuscarAsync(
                It.IsAny<string?>(),
                It.IsAny<StatusUnidade?>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<int>()))
            .ReturnsAsync(new UnidadePagedResult { Items = new(), TotalCount = 0 });

        var mockLink = new Mock<LinkGenerator>();
        mockLink
            .Setup(l => l.GetUriByAddress(
                It.IsAny<HttpContext>(),
                It.IsAny<RouteValuesAddress>(),
                It.IsAny<RouteValueDictionary>(),
                It.IsAny<RouteValueDictionary?>(),
                It.IsAny<string?>(),
                It.IsAny<HostString?>(),
                It.IsAny<PathString?>(),
                It.IsAny<FragmentString>(),
                It.IsAny<LinkOptions?>()))
            .Returns(string.Empty);

        var controller = CriarController(mockService.Object, mockLink.Object);

        // Act
        await controller.GetUnidadesMedicas();

        // Assert
        mockService.Verify(s => s.BuscarAsync(
            It.IsAny<string?>(),
            It.IsAny<StatusUnidade?>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<int>(),
            It.IsAny<int>()), Times.Once);
    }

    [Fact]
    public async Task GetUnidadesMedicas_QuandoListaVazia_DeveRetornar200ComZeroItens()
    {
        // Arrange
        var mockService = new Mock<IUnidadeService>();
        mockService
            .Setup(s => s.BuscarAsync(
                It.IsAny<string?>(),
                It.IsAny<StatusUnidade?>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<int>()))
            .ReturnsAsync(new UnidadePagedResult { Items = new(), TotalCount = 0 });

        var mockLink = new Mock<LinkGenerator>();
        mockLink
            .Setup(l => l.GetUriByAddress(
                It.IsAny<HttpContext>(),
                It.IsAny<RouteValuesAddress>(),
                It.IsAny<RouteValueDictionary>(),
                It.IsAny<RouteValueDictionary?>(),
                It.IsAny<string?>(),
                It.IsAny<HostString?>(),
                It.IsAny<PathString?>(),
                It.IsAny<FragmentString>(),
                It.IsAny<LinkOptions?>()))
            .Returns(string.Empty);

        var controller = CriarController(mockService.Object, mockLink.Object);

        // Act
        var result = await controller.GetUnidadesMedicas();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(200, okResult.StatusCode);
    }
}
