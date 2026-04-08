using Medix.Models;
using System.ComponentModel.DataAnnotations;

namespace Medix.Models.Dtos
{
    public class UnidadeMedicaUpdateDto
    {
        [Required]
        public string Nome { get; set; } = string.Empty;

        [Required]
        public string CNPJ { get; set; } = string.Empty;

        public string? Endereco { get; set; }

        public string? Telefone { get; set; }

        [Required]
        [EmailAddress]
        public string EmailAdmin { get; set; } = string.Empty;

        [Required]
        public StatusUnidade Status { get; set; }
    }
}
