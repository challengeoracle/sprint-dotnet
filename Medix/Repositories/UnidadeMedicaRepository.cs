using Medix.Data;
using Medix.Models;
using Microsoft.EntityFrameworkCore;

namespace Medix.Repositories
{
    public class UnidadeMedicaRepository : Repository<UnidadeMedica>, IUnidadeMedicaRepository
    {
        public UnidadeMedicaRepository(ApplicationDbContext context) : base(context) { }

        public async Task<(IEnumerable<UnidadeMedica> Items, int Total)> BuscarAsync(
            string? nome,
            StatusUnidade? status,
            string sortBy,
            string sortDirection,
            int pagina,
            int tamanhoPagina)
        {
            var query = _dbSet.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(nome))
                query = query.Where(u => u.Nome.Contains(nome));

            if (status.HasValue)
                query = query.Where(u => u.Status == status.Value);

            query = sortBy.ToLowerInvariant() switch
            {
                "datacadastro" => sortDirection.Equals("DESC", StringComparison.OrdinalIgnoreCase)
                    ? query.OrderByDescending(u => u.DataCadastro)
                    : query.OrderBy(u => u.DataCadastro),
                _ => sortDirection.Equals("DESC", StringComparison.OrdinalIgnoreCase)
                    ? query.OrderByDescending(u => u.Nome)
                    : query.OrderBy(u => u.Nome)
            };

            var total = await query.CountAsync();
            var items = await query
                .Skip((pagina - 1) * tamanhoPagina)
                .Take(tamanhoPagina)
                .ToListAsync();

            return (items, total);
        }
    }
}
