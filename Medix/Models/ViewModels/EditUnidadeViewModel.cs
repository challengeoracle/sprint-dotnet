using Medix.Models;
using System.ComponentModel.DataAnnotations;

namespace Medix.ViewModels
{
    public class EditUnidadeViewModel
    {
        // Necessário para identificar qual unidade editar
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome da unidade é obrigatório.")]
        [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres.")]
        public string Nome { get; set; }

        [Required(ErrorMessage = "O CNPJ é obrigatório.")]
        [StringLength(18, MinimumLength = 18, ErrorMessage = "O CNPJ deve ter 14 dígitos (formato: XX.XXX.XXX/XXXX-XX).")]
        public string CNPJ { get; set; }

        public string? Endereco { get; set; }

        public string? Telefone { get; set; }

        [Required(ErrorMessage = "O e-mail do administrador é obrigatório.")]
        [EmailAddress(ErrorMessage = "O formato do e-mail não é válido.")]
        public string EmailAdmin { get; set; }

        [Required(ErrorMessage = "O status é obrigatório.")]
        public StatusUnidade Status { get; set; }

        // Apenas para exibição, não será editável
        [Display(Name = "Data de Cadastro")]
        [DataType(DataType.DateTime)]
        public DateTime DataCadastro { get; set; }
    }
}