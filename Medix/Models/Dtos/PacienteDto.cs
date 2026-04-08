using System.Text.Json.Serialization;

namespace Medix.Models.Dtos
{
    public class PacienteDto
    {
        public int Id { get; set; }
        public string NomeCompleto { get; set; } = string.Empty;
        public string? CPF { get; set; }
        public DateTime? DataNascimento { get; set; }
        public string? Email { get; set; }
        public string? Telefone { get; set; }
        public string? Endereco { get; set; }
        public int UnidadeMedicaId { get; set; }

        [JsonPropertyOrder(100)]
        public List<LinkDto> Links { get; set; } = new();
    }
}
