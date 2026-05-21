using Medix.Data;
using Medix.Models;
using Microsoft.EntityFrameworkCore;

namespace Medix.Repositories
{
    public class PacienteRepository : Repository<Paciente>, IPacienteRepository
    {
        public PacienteRepository(ApplicationDbContext context) : base(context) { }

        public async Task<(IEnumerable<Paciente> Items, int Total)> BuscarPorUnidadeAsync(
            int unidadeId,
            string? nome,
            int pagina,
            int tamanhoPagina)
        {
            if (pagina < 1) pagina = 1;
            if (tamanhoPagina < 1) tamanhoPagina = 10;
            if (tamanhoPagina > 100) tamanhoPagina = 100;

            var query = _dbSet
                .AsNoTracking()
                .Where(p => p.UnidadeMedicaId == unidadeId);

            if (!string.IsNullOrWhiteSpace(nome))
                query = query.Where(p => p.NomeCompleto.Contains(nome));

            query = query.OrderBy(p => p.NomeCompleto);

            var total = await query.CountAsync();
            var items = await query
                .Skip((pagina - 1) * tamanhoPagina)
                .Take(tamanhoPagina)
                .ToListAsync();

            return (items, total);
        }

        public async Task<Paciente?> ObterPorUnidadeEIdAsync(int unidadeId, int id) =>
            await _dbSet.FirstOrDefaultAsync(p => p.Id == id && p.UnidadeMedicaId == unidadeId);
    }
}
