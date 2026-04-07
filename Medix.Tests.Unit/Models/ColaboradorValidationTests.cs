using System.ComponentModel.DataAnnotations;
using Medix.Models;

namespace Medix.Tests.Unit.Models;

public class ColaboradorValidationTests
{
    private static List<ValidationResult> Validate(object model)
    {
        var results = new List<ValidationResult>();
        var ctx = new ValidationContext(model, null, null);
        Validator.TryValidateObject(model, ctx, results, true);
        return results;
    }

    [Fact]
    public void Colaborador_QuandoNomeCompletoVazio_DeveRetornarErroDeValidacao()
    {
        // Arrange
        var colaborador = new Colaborador
        {
            NomeCompleto = "",
            Cargo = TipoColaborador.Medico,
            UnidadeMedicaId = 1
        };

        // Act
        var erros = Validate(colaborador);

        // Assert
        Assert.Contains(erros, e => e.MemberNames.Contains("NomeCompleto"));
    }

    [Fact]
    public void Colaborador_QuandoEmailInvalido_DeveRetornarErroDeValidacao()
    {
        // Arrange
        var colaborador = new Colaborador
        {
            NomeCompleto = "Dr. Carlos",
            Cargo = TipoColaborador.Medico,
            Email = "nao-eh-email",
            UnidadeMedicaId = 1
        };

        // Act
        var erros = Validate(colaborador);

        // Assert
        Assert.Contains(erros, e => e.MemberNames.Contains("Email"));
    }

    [Fact]
    public void Colaborador_QuandoDadosValidos_NaoDeveRetornarErros()
    {
        // Arrange
        var colaborador = new Colaborador
        {
            NomeCompleto = "Dra. Ana Lima",
            Cargo = TipoColaborador.Enfermeiro,
            Email = "ana@hospital.com",
            UnidadeMedicaId = 1
        };

        // Act
        var erros = Validate(colaborador);

        // Assert
        Assert.Empty(erros);
    }
}
