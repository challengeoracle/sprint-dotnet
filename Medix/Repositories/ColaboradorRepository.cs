using Medix.Data;
using Medix.Models;
using Microsoft.EntityFrameworkCore;

namespace Medix.Repositories
{
    public class ColaboradorRepository : Repository<Colaborador>, IColaboradorRepository
    {
        public ColaboradorRepository(ApplicationDbContext context) : base(context) { }

        public async Task<(IEnumerable<Colaborador> Items, int Total)> BuscarPorUnidadeAsync(
            int unidadeId,
            string? nome,
            TipoColaborador? cargo,
            int pagina,
            int tamanhoPagina)
        {
            if (pagina < 1) pagina = 1;
            if (tamanhoPagina < 1) tamanhoPagina = 10;
            if (tamanhoPagina > 100) tamanhoPagina = 100;

            var query = _dbSet
                .AsNoTracking()
                .Where(c => c.UnidadeMedicaId == unidadeId);

            if (!string.IsNullOrWhiteSpace(nome))
                query = query.Where(c => c.NomeCompleto.Contains(nome));

            if (cargo.HasValue)
                query = query.Where(c => c.Cargo == cargo.Value);

            query = query.OrderBy(c => c.NomeCompleto);

            var total = await query.CountAsync();
            var items = await query
                .Skip((pagina - 1) * tamanhoPagina)
                .Take(tamanhoPagina)
                .ToListAsync();

            return (items, total);
        }

        public async Task<Colaborador?> ObterPorUnidadeEIdAsync(int unidadeId, int id) =>
            await _dbSet.FirstOrDefaultAsync(c => c.Id == id && c.UnidadeMedicaId == unidadeId);
    }
}
