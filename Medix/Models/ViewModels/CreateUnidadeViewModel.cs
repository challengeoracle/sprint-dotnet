using Medix.Models;
using System.ComponentModel.DataAnnotations;

namespace Medix.ViewModels
{
    public class CreateUnidadeViewModel
    {
        [Required(ErrorMessage = "O nome da unidade é obrigatório.")]
        [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres.")]
        public string Nome { get; set; }

        [Required(ErrorMessage = "O CNPJ é obrigatório.")]
        [StringLength(18, MinimumLength = 18, ErrorMessage = "O CNPJ deve ter 14 dígitos (formato: XX.XXX.XXX/XXXX-XX).")]
        public string CNPJ { get; set; }

        public string? Endereco { get; set; }
        public string? Telefone { get; set; }

        [Required(ErrorMessage = "O e-mail de contato é obrigatório.")]
        [EmailAddress(ErrorMessage = "O formato do e-mail não é válido.")]
        [Display(Name = "E-mail de Contato (Admin)")]
        public string EmailAdmin { get; set; }

        [Required(ErrorMessage = "O status é obrigatório.")]
        public StatusUnidade Status { get; set; }

        // --- NOVOS CAMPOS PARA O LOGIN DA UNIDADE ---

        [Required(ErrorMessage = "O e-mail de acesso é obrigatório.")]
        [EmailAddress(ErrorMessage = "O formato do e-mail não é válido.")]
        [Display(Name = "E-mail de Acesso (para Login da Unidade)")]
        public string EmailAcesso { get; set; }

        [Required(ErrorMessage = "A senha de acesso é obrigatória.")]
        [StringLength(100, ErrorMessage = "A senha deve ter no mínimo 6 caracteres.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Senha de Acesso (para Login da Unidade)")]
        public string SenhaAcesso { get; set; }
    }
}