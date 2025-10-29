using Medix.Data;
using Medix.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Medix.Controllers
{
    [Route("api/unidades")] // Define a rota base da API
    [ApiController]       // Marca como um controller de API
    [Authorize]           // Garante que só usuários logados possam acessar
    public class UnidadesMedicasApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UnidadesMedicasApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/unidades
        [HttpGet]
        public async Task<ActionResult<PagedResult<UnidadeMedica>>> GetUnidadesMedicas(
            [FromQuery] string? nome = null,        // Filtro por nome
            [FromQuery] StatusUnidade? status = null, // Filtro por status
            [FromQuery] string sortBy = "Nome",      // Campo para ordenar (padrão: Nome)
            [FromQuery] string sortDirection = "ASC", // Direção da ordenação (padrão: ASC)
            [FromQuery] int page = 1,                 // Página atual (padrão: 1)
            [FromQuery] int pageSize = 10)            // Itens por página (padrão: 10)
        {
            // Validação básica dos parâmetros de paginação
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100; // Limite máximo para evitar sobrecarga

            // Inicia a consulta
            var query = _context.UnidadesMedicas.AsQueryable();

            // Aplica Filtros
            if (!string.IsNullOrWhiteSpace(nome))
            {
                query = query.Where(u => u.Nome.Contains(nome));
            }
            if (status.HasValue)
            {
                query = query.Where(u => u.Status == status.Value);
            }

            // Aplica Ordenação
            Expression<Func<UnidadeMedica, object>> orderByExpression = sortBy.ToLowerInvariant() switch
            {
                "datacadastro" => u => u.DataCadastro,
                _ => u => u.Nome // Padrão é ordenar por Nome
            };

            if (sortDirection.Equals("DESC", StringComparison.OrdinalIgnoreCase))
            {
                query = query.OrderByDescending(orderByExpression);
            }
            else
            {
                query = query.OrderBy(orderByExpression);
            }

            // Conta o total de itens ANTES da paginação
            var totalItems = await query.CountAsync();

            // Aplica Paginação
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Cria o objeto de resultado paginado
            var result = new PagedResult<UnidadeMedica>
            {
                Items = items,
                TotalCount = totalItems,
                PageNumber = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
            };

            return Ok(result);
        }

        // GET: api/unidades/5 (Endpoint simples para buscar por ID, útil para HATEOAS depois)
        [HttpGet("{id}")]
        public async Task<ActionResult<UnidadeMedica>> GetUnidadeMedica(int id)
        {
            var unidadeMedica = await _context.UnidadesMedicas.FindAsync(id);

            if (unidadeMedica == null)
            {
                return NotFound();
            }

            return Ok(unidadeMedica);
        }
    }

    // Classe auxiliar para formatar a resposta paginada
    public class PagedResult<T>
    {
        public List<T> Items { get; set; }
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }
}