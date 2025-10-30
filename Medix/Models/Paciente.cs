using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Medix.Models
{
    public class Paciente
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string NomeCompleto { get; set; }

        [StringLength(14)] // Formato 000.000.000-00
        public string? CPF { get; set; }

        public DateTime? DataNascimento { get; set; }

        [StringLength(100)]
        [EmailAddress]
        public string? Email { get; set; }

        [StringLength(15)] // (XX) XXXXX-XXXX
        public string? Telefone { get; set; }

        public string? Endereco { get; set; }

        // --- CHAVE ESTRANGEIRA ---
        // Isso garante que o paciente pertence a uma Unidade Médica
        [Required]
        public int UnidadeMedicaId { get; set; }

        [ForeignKey("UnidadeMedicaId")]
        public virtual UnidadeMedica UnidadeMedica { get; set; }
    }
}