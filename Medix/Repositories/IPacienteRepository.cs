using Medix.Models;

namespace Medix.Repositories
{
    /// <summary>
    /// Contrato específico para o repositório de Paciente.
    /// </summary>
    public interface IPacienteRepository : IRepository<Paciente>
    {
        Task<(IEnumerable<Paciente> Items, int Total)> BuscarPorUnidadeAsync(
            int unidadeId,
            string? nome,
            int pagina,
            int tamanhoPagina);

        Task<Paciente?> ObterPorUnidadeEIdAsync(int unidadeId, int id);
    }
}
