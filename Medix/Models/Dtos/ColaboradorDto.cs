using Medix.Models;
using System.Text.Json.Serialization;

namespace Medix.Models.Dtos
{
    public class ColaboradorDto
    {
        public int Id { get; set; }
        public string NomeCompleto { get; set; } = string.Empty;
        public string? Email { get; set; }
        public TipoColaborador Cargo { get; set; }
        public string? Especialidade { get; set; }
        public string? RegistroProfissional { get; set; }
        public int UnidadeMedicaId { get; set; }

        [JsonPropertyOrder(100)]
        public List<LinkDto> Links { get; set; } = new();
    }
}
