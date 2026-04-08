using Medix.Models;
using System.ComponentModel.DataAnnotations;

namespace Medix.Models.Dtos
{
    public class ColaboradorCreateDto
    {
        [Required]
        [StringLength(100)]
        public string NomeCompleto { get; set; } = string.Empty;

        [StringLength(100)]
        [EmailAddress]
        public string? Email { get; set; }

        [Required]
        public TipoColaborador Cargo { get; set; }

        [StringLength(50)]
        public string? Especialidade { get; set; }

        [StringLength(20)]
        public string? RegistroProfissional { get; set; }
    }
}
