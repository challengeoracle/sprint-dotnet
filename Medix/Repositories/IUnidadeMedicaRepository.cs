using Medix.Models;

namespace Medix.Repositories
{
    /// <summary>
    /// Contrato específico para o repositório de UnidadeMedica.
    /// Estende IRepository com operações próprias da entidade.
    /// </summary>
    public interface IUnidadeMedicaRepository : IRepository<UnidadeMedica>
    {
        /// <summary>
        /// Busca paginada com filtros opcionais de nome, status e ordenação.
        /// </summary>
        Task<(IEnumerable<UnidadeMedica> Items, int Total)> BuscarAsync(
            string? nome,
            StatusUnidade? status,
            string sortBy,
            string sortDirection,
            int pagina,
            int tamanhoPagina);
    }
}
