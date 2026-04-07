using Medix.Models;
using System.Linq.Expressions;

namespace Medix.Services
{
    public class UnidadePagedResult
    {
        public List<UnidadeMedica> Items { get; set; } = new();
        public int TotalCount { get; set; }
    }

    public interface IUnidadeService
    {
        /// <summary>
        /// Busca paginada com filtro, ordenação — usada pela API REST.
        /// </summary>
        Task<UnidadePagedResult> BuscarAsync(
            string? nome,
            StatusUnidade? status,
            string sortBy,
            string sortDirection,
            int page,
            int pageSize);

        /// <summary>
        /// Busca por id. Retorna null se não encontrado.
        /// </summary>
        Task<UnidadeMedica?> BuscarPorIdAsync(int id);
    }
}
