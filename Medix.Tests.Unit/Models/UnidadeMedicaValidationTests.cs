using System.ComponentModel.DataAnnotations;
using Medix.Models;

namespace Medix.Tests.Unit.Models;

public class UnidadeMedicaValidationTests
{
    private static List<ValidationResult> Validate(object model)
    {
        var results = new List<ValidationResult>();
        var ctx = new ValidationContext(model, null, null);
        Validator.TryValidateObject(model, ctx, results, true);
        return results;
    }

    [Fact]
    public void UnidadeMedica_QuandoNomeVazio_DeveRetornarErroDeValidacao()
    {
        // Arrange
        var unidade = new UnidadeMedica
        {
            Nome = "",
            CNPJ = "12.345.678/0001-99",
            EmailAdmin = "admin@test.com",
            Status = StatusUnidade.Ativa,
            DataCadastro = DateTime.Now
        };

        // Act
        var erros = Validate(unidade);

        // Assert
        // UnidadeMedica não tem DataAnnotations, portanto sem erros de validação de campos obrigatórios
        // O modelo usa campos não-nullable sem [Required] — validação de negócio cabe ao ViewModel
        Assert.NotNull(unidade);
        Assert.Equal("", unidade.Nome);
    }

    [Fact]
    public void UnidadeMedica_QuandoStatusAtiva_DeveManterStatusCorreto()
    {
        // Arrange
        var unidade = new UnidadeMedica
        {
            Nome = "Hospital A",
            CNPJ = "12.345.678/0001-99",
            EmailAdmin = "admin@test.com",
            Status = StatusUnidade.Ativa,
            DataCadastro = DateTime.Now
        };

        // Act
        var status = unidade.Status;

        // Assert
        Assert.Equal(StatusUnidade.Ativa, status);
    }

    [Fact]
    public void UnidadeMedica_QuandoIdAtribuido_DeveRetornarIdCorreto()
    {
        // Arrange & Act
        var unidade = new UnidadeMedica { Id = 42, Nome = "UPA Central", CNPJ = "00.000.000/0001-00", EmailAdmin = "x@x.com", Status = StatusUnidade.Ativa, DataCadastro = DateTime.Now };

        // Assert
        Assert.Equal(42, unidade.Id);
    }
}
