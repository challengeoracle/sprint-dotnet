using System.Linq.Expressions;

namespace Medix.Repositories
{
    /// <summary>
    /// Contrato genérico do padrão Repository.
    /// Define as operações CRUD básicas que todas as entidades compartilham.
    /// </summary>
    public interface IRepository<T> where T : class
    {
        Task<T?> ObterPorIdAsync(int id);
        Task<IEnumerable<T>> ObterTodosAsync();
        Task<(IEnumerable<T> Items, int Total)> BuscarAsync(
            Expression<Func<T, bool>>? filtro,
            Func<IQueryable<T>, IOrderedQueryable<T>>? ordenacao,
            int pagina,
            int tamanhoPagina);
        Task<bool> ExisteAsync(Expression<Func<T, bool>> predicado);
        Task AdicionarAsync(T entidade);
        void Atualizar(T entidade);
        void Remover(T entidade);
        Task SalvarAsync();
    }
}
