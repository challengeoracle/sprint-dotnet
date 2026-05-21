using Medix.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Medix.Repositories
{
    /// <summary>
    /// Implementação genérica do padrão Repository usando EF Core.
    /// Encapsula todas as operações de banco de dados, mantendo os Services
    /// independentes do mecanismo de persistência.
    /// </summary>
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly ApplicationDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public Repository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public async Task<T?> ObterPorIdAsync(int id) =>
            await _dbSet.FindAsync(id);

        public async Task<IEnumerable<T>> ObterTodosAsync() =>
            await _dbSet.AsNoTracking().ToListAsync();

        public async Task<(IEnumerable<T> Items, int Total)> BuscarAsync(
            Expression<Func<T, bool>>? filtro,
            Func<IQueryable<T>, IOrderedQueryable<T>>? ordenacao,
            int pagina,
            int tamanhoPagina)
        {
            if (pagina < 1) pagina = 1;
            if (tamanhoPagina < 1) tamanhoPagina = 10;
            if (tamanhoPagina > 100) tamanhoPagina = 100;

            IQueryable<T> query = _dbSet.AsNoTracking();

            if (filtro != null)
                query = query.Where(filtro);

            if (ordenacao != null)
                query = ordenacao(query);

            var total = await query.CountAsync();
            var items = await query
                .Skip((pagina - 1) * tamanhoPagina)
                .Take(tamanhoPagina)
                .ToListAsync();

            return (items, total);
        }

        public async Task<bool> ExisteAsync(Expression<Func<T, bool>> predicado) =>
            await _dbSet.AnyAsync(predicado);

        public async Task AdicionarAsync(T entidade) =>
            await _dbSet.AddAsync(entidade);

        public void Atualizar(T entidade) =>
            _dbSet.Update(entidade);

        public void Remover(T entidade) =>
            _dbSet.Remove(entidade);

        public async Task SalvarAsync() =>
            await _context.SaveChangesAsync();
    }
}
