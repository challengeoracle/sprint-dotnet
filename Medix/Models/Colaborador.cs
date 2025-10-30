using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Medix.Models
{
    public enum TipoColaborador
    {
        [Display(Name = "Médico(a)")]
        Medico,
        [Display(Name = "Enfermeiro(a)")]
        Enfermeiro,
        [Display(Name = "Administrativo")]
        Administrativo
    }

    public class Colaborador
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Nome Completo")]
        public string NomeCompleto { get; set; }

        [StringLength(100)]
        [EmailAddress]
        [Display(Name = "E-mail")]
        public string? Email { get; set; }

        [Required]
        public TipoColaborador Cargo { get; set; }

        [StringLength(50)]
        public string? Especialidade { get; set; } // Ex: Cardiologia, Ortopedia

        [StringLength(20)]
        [Display(Name = "Registro")]
        public string? RegistroProfissional { get; set; } // Ex: CRM, COREN

        // --- CHAVE ESTRANGEIRA ---
        // Isso garante que o colaborador pertence a uma Unidade Médica
        [Required]
        public int UnidadeMedicaId { get; set; }

        [ForeignKey("UnidadeMedicaId")]
        public virtual UnidadeMedica UnidadeMedica { get; set; }
    }
}