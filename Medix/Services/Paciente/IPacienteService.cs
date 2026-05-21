using Medix.Models;

namespace Medix.Services.Paciente
{
    public interface IPacienteService
    {
        Task<(IEnumerable<Models.Paciente> Items, int Total)> BuscarAsync(
            int unidadeId, string? nome, int pagina, int tamanhoPagina);

        Task<Models.Paciente?> BuscarPorIdAsync(int unidadeId, int id);

        Task<bool> UnidadeExisteAsync(int unidadeId);

        Task<Models.Paciente> CriarAsync(
            int unidadeId, string nomeCompleto, string cpf,
            DateTime dataNascimento, string? email, string? telefone, string? endereco);

        Task<Models.Paciente?> AtualizarAsync(
            int unidadeId, int id, string nomeCompleto, string cpf,
            DateTime dataNascimento, string? email, string? telefone, string? endereco);

        Task<bool> ExcluirAsync(int unidadeId, int id);
    }
}
