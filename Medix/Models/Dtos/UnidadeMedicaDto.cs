using Medix.Models;
using System.Text.Json.Serialization;

namespace Medix.Models.Dtos
{
    /// <summary>
    /// DTO de resposta para UnidadeMedica.
    /// Declara explicitamente apenas os campos que a API deve expor,
    /// sem herdar do modelo de domínio (princípio da responsabilidade única).
    /// Isso evita que propriedades internas como AdministradorUser
    /// vazem acidentalmente na serialização JSON.
    /// </summary>
    public class UnidadeMedicaDto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string CNPJ { get; set; } = string.Empty;
        public string? Endereco { get; set; }
        public string? Telefone { get; set; }
        public string EmailAdmin { get; set; } = string.Empty;
        public StatusUnidade Status { get; set; }
        public DateTime DataCadastro { get; set; }

        [JsonPropertyOrder(100)]
        public List<LinkDto> Links { get; set; } = new();
    }
}
