using Medix.Data;
using Medix.Models;
using Medix.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Routing;

namespace Medix.Controllers
{
    [Route("api/unidades")] // Define a rota base da API
    [ApiController]       // Marca como um controller de API
    [Authorize]           // Garante que só usuários logados possam acessar
    public class UnidadesMedicasApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly LinkGenerator _linkGenerator; // Injetado para gerar URLs

        // Injeta o LinkGenerator no construtor
        public UnidadesMedicasApiController(ApplicationDbContext context, LinkGenerator linkGenerator)
        {
            _context = context;
            _linkGenerator = linkGenerator;
        }

        // GET: api/unidades
        [HttpGet(Name = "GetUnidades")]
        // Agora retorna o DTO com links
        public async Task<ActionResult<PagedResultWithLinks<UnidadeMedicaDto>>> GetUnidadesMedicas(
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

            // Mapear Model pra DTO e gerar links pra cada item
            var itemDtos = items.Select(item =>
            {
                var dto = new UnidadeMedicaDto
                {
                    Id = item.Id,
                    Nome = item.Nome,
                    CNPJ = item.CNPJ,
                    Endereco = item.Endereco,
                    Telefone = item.Telefone,
                    EmailAdmin = item.EmailAdmin,
                    Status = item.Status,
                    DataCadastro = item.DataCadastro
                };
                GenerateItemLinks(dto); // Chama a função pra criar os links HATEOAS do item
                return dto;
            }).ToList();

            // Cria o objeto de resultado paginado com links
            var result = new PagedResultWithLinks<UnidadeMedicaDto>
            {
                Items = itemDtos, // Agora usa a lista de DTOs
                TotalCount = totalItems,
                PageNumber = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
            };

            GeneratePagingLinks(result, nome, status, sortBy, sortDirection); // Chama a função pra criar os links HATEOAS de paginação

            return Ok(result);
        }

        // GET: api/unidades/5
        [HttpGet("{id}", Name = "GetUnidadeById")] // Nomeei a rota pra gerar link pra ela
        // Agora retorna o DTO com links
        public async Task<ActionResult<UnidadeMedicaDto>> GetUnidadeMedica(int id)
        {
            var unidadeMedica = await _context.UnidadesMedicas.FindAsync(id);

            if (unidadeMedica == null)
            {
                return NotFound();
            }

            // Mapear Model pra DTO
            var dto = new UnidadeMedicaDto
            {
                Id = unidadeMedica.Id,
                Nome = unidadeMedica.Nome,
                CNPJ = unidadeMedica.CNPJ,
                Endereco = unidadeMedica.Endereco,
                Telefone = unidadeMedica.Telefone,
                EmailAdmin = unidadeMedica.EmailAdmin,
                Status = unidadeMedica.Status,
                DataCadastro = unidadeMedica.DataCadastro
            };
            GenerateItemLinks(dto); // Gerar links HATEOAS pro item

            return Ok(dto);
        }

        // --- Métodos Auxiliares para Gerar Links HATEOAS ---

        private void GenerateItemLinks(UnidadeMedicaDto dto)
        {
            // Link pro próprio recurso ("self")
            dto.Links.Add(new LinkDto(
                _linkGenerator.GetUriByName(HttpContext, "GetUnidadeById", new { id = dto.Id }),
                "self",
                "GET"));

            // Link pra editar ("update") - Aponta pra mesma rota GET por enquanto, método PUT
            dto.Links.Add(new LinkDto(
                _linkGenerator.GetUriByName(HttpContext, "GetUnidadeById", new { id = dto.Id }),
                "update",
                "PUT"));

            // Link pra excluir ("delete") - Aponta pra mesma rota GET por enquanto, método DELETE
            dto.Links.Add(new LinkDto(
                _linkGenerator.GetUriByName(HttpContext, "GetUnidadeById", new { id = dto.Id }),
                "delete",
                "DELETE"));
        }

        private void GeneratePagingLinks(PagedResultWithLinks<UnidadeMedicaDto> result, string? nome, StatusUnidade? status, string sortBy, string sortDirection)
        {
            // Link pra página atual ("self")
            result.Links.Add(new LinkDto(
                _linkGenerator.GetUriByName(HttpContext, "GetUnidades", new { nome, status, sortBy, sortDirection, page = result.PageNumber, pageSize = result.PageSize }),
                "self",
                "GET"));

            // Link pra primeira página ("first")
            if (result.PageNumber > 1)
            {
                result.Links.Add(new LinkDto(
                   _linkGenerator.GetUriByName(HttpContext, "GetUnidades", new { nome, status, sortBy, sortDirection, page = 1, pageSize = result.PageSize }),
                   "first",
                   "GET"));
            }

            // Link pra página anterior ("previous")
            if (result.PageNumber > 1)
            {
                result.Links.Add(new LinkDto(
                   _linkGenerator.GetUriByName(HttpContext, "GetUnidades", new { nome, status, sortBy, sortDirection, page = result.PageNumber - 1, pageSize = result.PageSize }),
                   "previous",
                   "GET"));
            }

            // Link pra próxima página ("next")
            if (result.PageNumber < result.TotalPages)
            {
                result.Links.Add(new LinkDto(
                   _linkGenerator.GetUriByName(HttpContext, "GetUnidades", new { nome, status, sortBy, sortDirection, page = result.PageNumber + 1, pageSize = result.PageSize }),
                   "next",
                   "GET"));
            }

            // Link pra última página ("last")
            if (result.PageNumber < result.TotalPages)
            {
                result.Links.Add(new LinkDto(
                   _linkGenerator.GetUriByName(HttpContext, "GetUnidades", new { nome, status, sortBy, sortDirection, page = result.TotalPages, pageSize = result.PageSize }),
                   "last",
                   "GET"));
            }
        }
    }

    // Classe auxiliar para formatar a resposta paginada com links HATEOAS
    public class PagedResultWithLinks<T> where T : class
    {
        public List<T> Items { get; set; }
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public List<LinkDto> Links { get; set; } = new List<LinkDto>();
    }
}