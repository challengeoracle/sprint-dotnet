using System.ComponentModel.DataAnnotations;
using Medix.Models;

namespace Medix.Tests.Unit.Models;

public class PacienteValidationTests
{
    private static List<ValidationResult> Validate(object model)
    {
        var results = new List<ValidationResult>();
        var ctx = new ValidationContext(model, null, null);
        Validator.TryValidateObject(model, ctx, results, true);
        return results;
    }

    [Fact]
    public void Paciente_QuandoNomeCompletoVazio_DeveRetornarErroDeValidacao()
    {
        // Arrange
        var paciente = new Paciente
        {
            NomeCompleto = "",
            UnidadeMedicaId = 1
        };

        // Act
        var erros = Validate(paciente);

        // Assert
        Assert.Contains(erros, e => e.MemberNames.Contains("NomeCompleto"));
    }

    [Fact]
    public void Paciente_QuandoEmailInvalido_DeveRetornarErroDeValidacao()
    {
        // Arrange
        var paciente = new Paciente
        {
            NomeCompleto = "João Silva",
            Email = "email-invalido",
            UnidadeMedicaId = 1
        };

        // Act
        var erros = Validate(paciente);

        // Assert
        Assert.Contains(erros, e => e.MemberNames.Contains("Email"));
    }

    [Fact]
    public void Paciente_QuandoDadosValidos_NaoDeveRetornarErros()
    {
        // Arrange
        var paciente = new Paciente
        {
            NomeCompleto = "Maria Oliveira",
            Email = "maria@example.com",
            UnidadeMedicaId = 1
        };

        // Act
        var erros = Validate(paciente);

        // Assert
        Assert.Empty(erros);
    }

    [Fact]
    public void Paciente_QuandoNomeCompletoExcede100Caracteres_DeveRetornarErroDeValidacao()
    {
        // Arrange
        var paciente = new Paciente
        {
            NomeCompleto = new string('A', 101),
            UnidadeMedicaId = 1
        };

        // Act
        var erros = Validate(paciente);

        // Assert
        Assert.Contains(erros, e => e.MemberNames.Contains("NomeCompleto"));
    }
}
