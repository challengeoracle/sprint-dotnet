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

        /// <summary>
        /// Cria uma nova unidade médica.
        /// </summary>
        Task<UnidadeMedica> CriarAsync(string nome, string cnpj, string? endereco, string? telefone, string emailAdmin, StatusUnidade status);

        /// <summary>
        /// Atualiza os dados de uma unidade. Retorna null se não encontrada.
        /// </summary>
        Task<UnidadeMedica?> AtualizarAsync(int id, string nome, string cnpj, string? endereco, string? telefone, string emailAdmin, StatusUnidade status);

        /// <summary>
        /// Remove uma unidade. Retorna false se não encontrada.
        /// </summary>
        Task<bool> ExcluirAsync(int id);
    }
}
