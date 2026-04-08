using Medix.Data;
using Medix.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Medix.Services
{
    public class UnidadeService : IUnidadeService
    {
        private readonly ApplicationDbContext _context;

        public UnidadeService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<UnidadePagedResult> BuscarAsync(
            string? nome,
            StatusUnidade? status,
            string sortBy,
            string sortDirection,
            int page,
            int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            var query = _context.UnidadesMedicas.AsQueryable();

            if (!string.IsNullOrWhiteSpace(nome))
                query = query.Where(u => u.Nome.Contains(nome));

            if (status.HasValue)
                query = query.Where(u => u.Status == status.Value);

            Expression<Func<UnidadeMedica, object>> orderExpr = sortBy.ToLowerInvariant() switch
            {
                "datacadastro" => u => u.DataCadastro,
                _ => u => u.Nome
            };

            query = sortDirection.Equals("DESC", StringComparison.OrdinalIgnoreCase)
                ? query.OrderByDescending(orderExpr)
                : query.OrderBy(orderExpr);

            var totalCount = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return new UnidadePagedResult { Items = items, TotalCount = totalCount };
        }

        public async Task<UnidadeMedica?> BuscarPorIdAsync(int id)
        {
            return await _context.UnidadesMedicas.FindAsync(id);
        }

        public async Task<UnidadeMedica> CriarAsync(string nome, string cnpj, string? endereco, string? telefone, string emailAdmin, StatusUnidade status)
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

            _context.UnidadesMedicas.Add(unidade);
            await _context.SaveChangesAsync();
            return unidade;
        }

        public async Task<UnidadeMedica?> AtualizarAsync(int id, string nome, string cnpj, string? endereco, string? telefone, string emailAdmin, StatusUnidade status)
        {
            var unidade = await _context.UnidadesMedicas.FindAsync(id);
            if (unidade == null) return null;

            unidade.Nome = nome;
            unidade.CNPJ = cnpj;
            unidade.Endereco = endereco;
            unidade.Telefone = telefone;
            unidade.EmailAdmin = emailAdmin;
            unidade.Status = status;

            await _context.SaveChangesAsync();
            return unidade;
        }

        public async Task<bool> ExcluirAsync(int id)
        {
            var unidade = await _context.UnidadesMedicas.FindAsync(id);
            if (unidade == null) return false;

            _context.UnidadesMedicas.Remove(unidade);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
