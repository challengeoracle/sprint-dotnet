using Medix.Models;
using Medix.Repositories;

namespace Medix.Services
{
    public class UnidadeService : IUnidadeService
    {
        private readonly IUnidadeMedicaRepository _repo;

        public UnidadeService(IUnidadeMedicaRepository repo)
        {
            _repo = repo;
        }

        public async Task<UnidadePagedResult> BuscarAsync(
            string? nome,
            StatusUnidade? status,
            string sortBy,
            string sortDirection,
            int page,
            int pageSize)
        {
            var (items, total) = await _repo.BuscarAsync(nome, status, sortBy, sortDirection, page, pageSize);
            return new UnidadePagedResult { Items = items.ToList(), TotalCount = total };
        }

        public async Task<UnidadeMedica?> BuscarPorIdAsync(int id) =>
            await _repo.ObterPorIdAsync(id);

        public async Task<UnidadeMedica> CriarAsync(
            string nome, string cnpj, string? endereco,
            string? telefone, string emailAdmin, StatusUnidade status)
        {
            var unidade = new UnidadeMedica
            {
                Nome = nome,
                CNPJ = cnpj,
                Endereco = endereco,
                Telefone = telefone,
                EmailAdmin = emailAdmin,
                Status = status,
                DataCadastro = DateTime.UtcNow
            };

            await _repo.AdicionarAsync(unidade);
            await _repo.SalvarAsync();
            return unidade;
        }

        public async Task<UnidadeMedica?> AtualizarAsync(
            int id, string nome, string cnpj, string? endereco,
            string? telefone, string emailAdmin, StatusUnidade status)
        {
            var unidade = await _repo.ObterPorIdAsync(id);
            if (unidade == null) return null;

            unidade.Nome = nome;
            unidade.CNPJ = cnpj;
            unidade.Endereco = endereco;
            unidade.Telefone = telefone;
            unidade.EmailAdmin = emailAdmin;
            unidade.Status = status;

            _repo.Atualizar(unidade);
            await _repo.SalvarAsync();
            return unidade;
        }

        public async Task<bool> ExcluirAsync(int id)
        {
            var unidade = await _repo.ObterPorIdAsync(id);
            if (unidade == null) return false;

            _repo.Remover(unidade);
            await _repo.SalvarAsync();
            return true;
        }
    }
}
