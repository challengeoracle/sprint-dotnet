using Medix.Models;

namespace Medix.Services.Colaborador
{
    public interface IColaboradorService
    {
        Task<(IEnumerable<Models.Colaborador> Items, int Total)> BuscarAsync(
            int unidadeId, string? nome, TipoColaborador? cargo, int pagina, int tamanhoPagina);

        Task<Models.Colaborador?> BuscarPorIdAsync(int unidadeId, int id);

        Task<bool> UnidadeExisteAsync(int unidadeId);

        Task<Models.Colaborador> CriarAsync(
            int unidadeId, string nomeCompleto, string email,
            TipoColaborador cargo, string? especialidade, string? registroProfissional);

        Task<Models.Colaborador?> AtualizarAsync(
            int unidadeId, int id, string nomeCompleto, string email,
            TipoColaborador cargo, string? especialidade, string? registroProfissional);

        Task<bool> ExcluirAsync(int unidadeId, int id);
    }
}
