using Medix.Models;

namespace Medix.Repositories
{
    /// <summary>
    /// Contrato específico para o repositório de Colaborador.
    /// </summary>
    public interface IColaboradorRepository : IRepository<Colaborador>
    {
        Task<(IEnumerable<Colaborador> Items, int Total)> BuscarPorUnidadeAsync(
            int unidadeId,
            string? nome,
            TipoColaborador? cargo,
            int pagina,
            int tamanhoPagina);

        Task<Colaborador?> ObterPorUnidadeEIdAsync(int unidadeId, int id);
    }
}
