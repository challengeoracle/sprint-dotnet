using Medix.Repositories;

namespace Medix.Services.Paciente
{
    public class PacienteService : IPacienteService
    {
        private readonly IPacienteRepository _pacienteRepo;
        private readonly IUnidadeMedicaRepository _unidadeRepo;

        public PacienteService(IPacienteRepository pacienteRepo, IUnidadeMedicaRepository unidadeRepo)
        {
            _pacienteRepo = pacienteRepo;
            _unidadeRepo = unidadeRepo;
        }

        public async Task<bool> UnidadeExisteAsync(int unidadeId) =>
            await _unidadeRepo.ExisteAsync(u => u.Id == unidadeId);

        public async Task<(IEnumerable<Models.Paciente> Items, int Total)> BuscarAsync(
            int unidadeId, string? nome, int pagina, int tamanhoPagina) =>
            await _pacienteRepo.BuscarPorUnidadeAsync(unidadeId, nome, pagina, tamanhoPagina);

        public async Task<Models.Paciente?> BuscarPorIdAsync(int unidadeId, int id) =>
            await _pacienteRepo.ObterPorUnidadeEIdAsync(unidadeId, id);

        public async Task<Models.Paciente> CriarAsync(
            int unidadeId, string nomeCompleto, string cpf,
            DateTime dataNascimento, string? email, string? telefone, string? endereco)
        {
            var paciente = new Models.Paciente
            {
                UnidadeMedicaId = unidadeId,
                NomeCompleto = nomeCompleto,
                CPF = cpf,
                DataNascimento = dataNascimento,
                Email = email,
                Telefone = telefone,
                Endereco = endereco
            };

            await _pacienteRepo.AdicionarAsync(paciente);
            await _pacienteRepo.SalvarAsync();
            return paciente;
        }

        public async Task<Models.Paciente?> AtualizarAsync(
            int unidadeId, int id, string nomeCompleto, string cpf,
            DateTime dataNascimento, string? email, string? telefone, string? endereco)
        {
            var paciente = await _pacienteRepo.ObterPorUnidadeEIdAsync(unidadeId, id);
            if (paciente == null) return null;

            paciente.NomeCompleto = nomeCompleto;
            paciente.CPF = cpf;
            paciente.DataNascimento = dataNascimento;
            paciente.Email = email;
            paciente.Telefone = telefone;
            paciente.Endereco = endereco;

            _pacienteRepo.Atualizar(paciente);
            await _pacienteRepo.SalvarAsync();
            return paciente;
        }

        public async Task<bool> ExcluirAsync(int unidadeId, int id)
        {
            var paciente = await _pacienteRepo.ObterPorUnidadeEIdAsync(unidadeId, id);
            if (paciente == null) return false;

            _pacienteRepo.Remover(paciente);
            await _pacienteRepo.SalvarAsync();
            return true;
        }
    }
}
