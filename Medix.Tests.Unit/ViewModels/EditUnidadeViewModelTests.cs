using System.ComponentModel.DataAnnotations;
using Medix.Models;
using Medix.ViewModels;

namespace Medix.Tests.Unit.ViewModels;

public class EditUnidadeViewModelTests
{
    private static List<ValidationResult> Validate(object model)
    {
        var results = new List<ValidationResult>();
        var ctx = new ValidationContext(model, null, null);
        Validator.TryValidateObject(model, ctx, results, true);
        return results;
    }

    private static EditUnidadeViewModel ViewModelValido() => new EditUnidadeViewModel
    {
        Id = 1,
        Nome = "UPA Leste",
        CNPJ = "98.765.432/0001-10",
        EmailAdmin = "upa@hospital.com",
        Status = StatusUnidade.Ativa,
        DataCadastro = DateTime.Now
    };

    [Fact]
    public void EditUnidadeViewModel_QuandoDadosValidos_NaoDeveRetornarErros()
    {
        // Arrange
        var vm = ViewModelValido();

        // Act
        var erros = Validate(vm);

        // Assert
        Assert.Empty(erros);
    }

    [Fact]
    public void EditUnidadeViewModel_QuandoNomeVazio_DeveRetornarErroDeValidacao()
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
    public void EditUnidadeViewModel_QuandoEmailInvalido_DeveRetornarErroDeValidacao()
    {
        // Arrange
        var vm = ViewModelValido();
        vm.EmailAdmin = "invalido";

        // Act
        var erros = Validate(vm);

        // Assert
        Assert.Contains(erros, e => e.MemberNames.Contains("EmailAdmin"));
    }
}
