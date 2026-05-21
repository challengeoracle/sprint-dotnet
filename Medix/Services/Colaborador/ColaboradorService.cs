using Medix.Models;
using Medix.Repositories;

namespace Medix.Services.Colaborador
{
    public class ColaboradorService : IColaboradorService
    {
        private readonly IColaboradorRepository _colaboradorRepo;
        private readonly IUnidadeMedicaRepository _unidadeRepo;

        public ColaboradorService(IColaboradorRepository colaboradorRepo, IUnidadeMedicaRepository unidadeRepo)
        {
            _colaboradorRepo = colaboradorRepo;
            _unidadeRepo = unidadeRepo;
        }

        public async Task<bool> UnidadeExisteAsync(int unidadeId) =>
            await _unidadeRepo.ExisteAsync(u => u.Id == unidadeId);

        public async Task<(IEnumerable<Models.Colaborador> Items, int Total)> BuscarAsync(
            int unidadeId, string? nome, TipoColaborador? cargo, int pagina, int tamanhoPagina) =>
            await _colaboradorRepo.BuscarPorUnidadeAsync(unidadeId, nome, cargo, pagina, tamanhoPagina);

        public async Task<Models.Colaborador?> BuscarPorIdAsync(int unidadeId, int id) =>
            await _colaboradorRepo.ObterPorUnidadeEIdAsync(unidadeId, id);

        public async Task<Models.Colaborador> CriarAsync(
            int unidadeId, string nomeCompleto, string email,
            TipoColaborador cargo, string? especialidade, string? registroProfissional)
        {
            var colaborador = new Models.Colaborador
            {
                UnidadeMedicaId = unidadeId,
                NomeCompleto = nomeCompleto,
                Email = email,
                Cargo = cargo,
                Especialidade = especialidade,
                RegistroProfissional = registroProfissional
            };

            await _colaboradorRepo.AdicionarAsync(colaborador);
            await _colaboradorRepo.SalvarAsync();
            return colaborador;
        }

        public async Task<Models.Colaborador?> AtualizarAsync(
            int unidadeId, int id, string nomeCompleto, string email,
            TipoColaborador cargo, string? especialidade, string? registroProfissional)
        {
            var colaborador = await _colaboradorRepo.ObterPorUnidadeEIdAsync(unidadeId, id);
            if (colaborador == null) return null;

            colaborador.NomeCompleto = nomeCompleto;
            colaborador.Email = email;
            colaborador.Cargo = cargo;
            colaborador.Especialidade = especialidade;
            colaborador.RegistroProfissional = registroProfissional;

            _colaboradorRepo.Atualizar(colaborador);
            await _colaboradorRepo.SalvarAsync();
            return colaborador;
        }

        public async Task<bool> ExcluirAsync(int unidadeId, int id)
        {
            var colaborador = await _colaboradorRepo.ObterPorUnidadeEIdAsync(unidadeId, id);
            if (colaborador == null) return false;

            _colaboradorRepo.Remover(colaborador);
            await _colaboradorRepo.SalvarAsync();
            return true;
        }
    }
}
