using System.ComponentModel.DataAnnotations;
using Medix.Models;
using Medix.ViewModels;

namespace Medix.Tests.Unit.ViewModels;

public class CreateUnidadeViewModelTests
{
    private static List<ValidationResult> Validate(object model)
    {
        var results = new List<ValidationResult>();
        var ctx = new ValidationContext(model, null, null);
        Validator.TryValidateObject(model, ctx, results, true);
        return results;
    }

    private static CreateUnidadeViewModel ViewModelValido() => new CreateUnidadeViewModel
    {
        Nome = "Hospital Central",
        CNPJ = "12.345.678/0001-99",
        EmailAdmin = "admin@hospital.com",
        Status = StatusUnidade.Ativa,
        EmailAcesso = "login@hospital.com",
        SenhaAcesso = "Senha@123"
    };

    [Fact]
    public void CreateUnidadeViewModel_QuandoDadosValidos_NaoDeveRetornarErros()
    {
        // Arrange
        var vm = ViewModelValido();

        // Act
        var erros = Validate(vm);

        // Assert
        Assert.Empty(erros);
    }

    [Fact]
    public void CreateUnidadeViewModel_QuandoNomeVazio_DeveRetornarErroDeValidacao()
    {
        // Arrange
        var vm = ViewModelValido();
        vm.Nome = "";

        // Act
        var erros = Validate(vm);

        // Assert
        Assert.Contains(erros, e => e.MemberNames.Contains("Nome"));
    }

    [Fact]
    public void CreateUnidadeViewModel_QuandoEmailInvalido_DeveRetornarErroDeValidacao()
    {
        // Arrange
        var vm = ViewModelValido();
        vm.EmailAdmin = "nao-eh-email";

        // Act
        var erros = Validate(vm);

        // Assert
        Assert.Contains(erros, e => e.MemberNames.Contains("EmailAdmin"));
    }

    [Fact]
    public void CreateUnidadeViewModel_QuandoCNPJForaDoTamanho_DeveRetornarErroDeValidacao()
    {
        // Arrange
        var vm = ViewModelValido();
        vm.CNPJ = "12345";  // menos que 18 chars

        // Act
        var erros = Validate(vm);

        // Assert
        Assert.Contains(erros, e => e.MemberNames.Contains("CNPJ"));
    }

    [Fact]
    public void CreateUnidadeViewModel_QuandoSenhaMenorQue6Chars_DeveRetornarErroDeValidacao()
    {
        // Arrange
        var vm = ViewModelValido();
        vm.SenhaAcesso = "123";

        // Act
        var erros = Validate(vm);

        // Assert
        Assert.Contains(erros, e => e.MemberNames.Contains("SenhaAcesso"));
    }

    [Fact]
    public void CreateUnidadeViewModel_QuandoEmailAcessoVazio_DeveRetornarErroDeValidacao()
    {
        // Arrange
        var vm = ViewModelValido();
        vm.EmailAcesso = "";

        // Act
        var erros = Validate(vm);

        // Assert
        Assert.Contains(erros, e => e.MemberNames.Contains("EmailAcesso"));
    }
}
