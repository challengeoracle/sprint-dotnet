using Medix.Models;
using System.Text.Json.Serialization; // Para ignorar propriedades

namespace Medix.Models.Dtos
{
    public class UnidadeMedicaDto : UnidadeMedica // Herda da unidade para ter os dados
    {
        [JsonPropertyOrder(100)] // Coloca os links no final do JSON
        public List<LinkDto> Links { get; set; } = new List<LinkDto>();
    }
}