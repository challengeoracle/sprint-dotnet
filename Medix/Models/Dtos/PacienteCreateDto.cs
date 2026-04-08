using System.ComponentModel.DataAnnotations;

namespace Medix.Models.Dtos
{
    public class PacienteCreateDto
    {
        [Required]
        [StringLength(100)]
        public string NomeCompleto { get; set; } = string.Empty;

        [StringLength(14)]
        public string? CPF { get; set; }

        public DateTime? DataNascimento { get; set; }

        [StringLength(100)]
        [EmailAddress]
        public string? Email { get; set; }

        [StringLength(15)]
        public string? Telefone { get; set; }

        public string? Endereco { get; set; }
    }
}
