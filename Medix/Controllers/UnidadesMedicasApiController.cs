using Medix.Data;
using Medix.Models;
using Medix.Models.Dtos;
using Medix.Services;
using Medix.Services.Audit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Routing;

namespace Medix.Controllers
{
    [Route("api/unidades")]
    [ApiController]
    [Authorize]
    [EnableRateLimiting("api")]
    public class UnidadesMedicasApiController : ControllerBase
    {
        private readonly IUnidadeService _unidadeService;
        private readonly LinkGenerator _linkGenerator;
        private readonly IAuditoriaService _auditoria;

        public UnidadesMedicasApiController(
            IUnidadeService unidadeService,
            LinkGenerator linkGenerator,
            IAuditoriaService auditoria)
        {
            _unidadeService = unidadeService;
            _linkGenerator = linkGenerator;
            _auditoria = auditoria;
        }

        // GET: api/unidades
        [HttpGet(Name = "GetUnidades")]
        public async Task<ActionResult<PagedResultWithLinks<UnidadeMedicaDto>>> GetUnidadesMedicas(
            [FromQuery] string? nome = null,
            [FromQuery] StatusUnidade? status = null,
            [FromQuery] string sortBy = "Nome",
            [FromQuery] string sortDirection = "ASC",
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var pagedResult = await _unidadeService.BuscarAsync(nome, status, sortBy, sortDirection, page, pageSize);

            var totalItems = pagedResult.TotalCount;
            var items = pagedResult.Items;

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
                GenerateItemLinks(dto);
                return dto;
            }).ToList();

            var result = new PagedResultWithLinks<UnidadeMedicaDto>
            {
                Items = itemDtos,
                TotalCount = totalItems,
                PageNumber = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
            };

            GeneratePagingLinks(result, nome, status, sortBy, sortDirection);

            return Ok(result);
        }

        // GET: api/unidades/5
        [HttpGet("{id}", Name = "GetUnidadeById")]
        public async Task<ActionResult<UnidadeMedicaDto>> GetUnidadeMedica(int id)
        {
            var unidadeMedica = await _unidadeService.BuscarPorIdAsync(id);

            if (unidadeMedica == null)
                return NotFound();

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
            GenerateItemLinks(dto);

            return Ok(dto);
        }

        // POST: api/unidades
        [HttpPost(Name = "CreateUnidade")]
        public async Task<ActionResult<UnidadeMedicaDto>> PostUnidadeMedica([FromBody] UnidadeMedicaCreateDto dto)
        {
            var unidade = await _unidadeService.CriarAsync(dto.Nome, dto.CNPJ, dto.Endereco, dto.Telefone, dto.EmailAdmin, dto.Status);

            var result = new UnidadeMedicaDto
            {
                Id = unidade.Id,
                Nome = unidade.Nome,
                CNPJ = unidade.CNPJ,
                Endereco = unidade.Endereco,
                Telefone = unidade.Telefone,
                EmailAdmin = unidade.EmailAdmin,
                Status = unidade.Status,
                DataCadastro = unidade.DataCadastro
            };
            GenerateItemLinks(result);

            // --- Auditoria ---
            await _auditoria.RegistrarAsync(
                entidade: "UnidadeMedica",
                entidadeId: unidade.Id,
                operacao: "CREATE",
                realizadoPor: User.Identity?.Name ?? "anonimo",
                detalhe: new Dictionary<string, object?>
                {
                    ["nome"] = unidade.Nome,
                    ["cnpj"] = unidade.CNPJ,
                    ["emailAdmin"] = unidade.EmailAdmin,
                    ["status"] = unidade.Status.ToString()
                });

            return CreatedAtRoute("GetUnidadeById", new { id = result.Id }, result);
        }

        // PUT: api/unidades/5
        [HttpPut("{id}", Name = "UpdateUnidade")]
        public async Task<ActionResult<UnidadeMedicaDto>> PutUnidadeMedica(int id, [FromBody] UnidadeMedicaUpdateDto dto)
        {
            var unidade = await _unidadeService.AtualizarAsync(id, dto.Nome, dto.CNPJ, dto.Endereco, dto.Telefone, dto.EmailAdmin, dto.Status);

            if (unidade == null)
                return NotFound();

            var result = new UnidadeMedicaDto
            {
                Id = unidade.Id,
                Nome = unidade.Nome,
                CNPJ = unidade.CNPJ,
                Endereco = unidade.Endereco,
                Telefone = unidade.Telefone,
                EmailAdmin = unidade.EmailAdmin,
                Status = unidade.Status,
                DataCadastro = unidade.DataCadastro
            };
            GenerateItemLinks(result);

            // --- Auditoria ---
            await _auditoria.RegistrarAsync(
                entidade: "UnidadeMedica",
                entidadeId: id,
                operacao: "UPDATE",
                realizadoPor: User.Identity?.Name ?? "anonimo",
                detalhe: new Dictionary<string, object?>
                {
                    ["nome"] = unidade.Nome,
                    ["cnpj"] = unidade.CNPJ,
                    ["emailAdmin"] = unidade.EmailAdmin,
                    ["status"] = unidade.Status.ToString()
                });

            return Ok(result);
        }

        // DELETE: api/unidades/5
        [HttpDelete("{id}", Name = "DeleteUnidade")]
        public async Task<IActionResult> DeleteUnidadeMedica(int id)
        {
            var excluido = await _unidadeService.ExcluirAsync(id);

            if (!excluido)
                return NotFound();

            // --- Auditoria ---
            await _auditoria.RegistrarAsync(
                entidade: "UnidadeMedica",
                entidadeId: id,
                operacao: "DELETE",
                realizadoPor: User.Identity?.Name ?? "anonimo",
                detalhe: new Dictionary<string, object?> { ["id"] = id });

            return NoContent();
        }

        // --- Métodos Auxiliares para Gerar Links HATEOAS ---

        private void GenerateItemLinks(UnidadeMedicaDto dto)
        {
            dto.Links.Add(new LinkDto(
                _linkGenerator.GetUriByName(HttpContext, "GetUnidadeById", new { id = dto.Id }) ?? string.Empty,
                "self", "GET"));

            dto.Links.Add(new LinkDto(
                _linkGenerator.GetUriByName(HttpContext, "UpdateUnidade", new { id = dto.Id }) ?? string.Empty,
                "update", "PUT"));

            dto.Links.Add(new LinkDto(
                _linkGenerator.GetUriByName(HttpContext, "DeleteUnidade", new { id = dto.Id }) ?? string.Empty,
                "delete", "DELETE"));
        }

        private void GeneratePagingLinks(PagedResultWithLinks<UnidadeMedicaDto> result, string? nome, StatusUnidade? status, string sortBy, string sortDirection)
        {
            result.Links.Add(new LinkDto(
                _linkGenerator.GetUriByName(HttpContext, "GetUnidades", new { nome, status, sortBy, sortDirection, page = result.PageNumber, pageSize = result.PageSize }) ?? string.Empty,
                "self", "GET"));

            if (result.PageNumber > 1)
            {
                result.Links.Add(new LinkDto(
                   _linkGenerator.GetUriByName(HttpContext, "GetUnidades", new { nome, status, sortBy, sortDirection, page = 1, pageSize = result.PageSize }) ?? string.Empty,
                   "first", "GET"));

                result.Links.Add(new LinkDto(
                   _linkGenerator.GetUriByName(HttpContext, "GetUnidades", new { nome, status, sortBy, sortDirection, page = result.PageNumber - 1, pageSize = result.PageSize }) ?? string.Empty,
                   "previous", "GET"));
            }

            if (result.PageNumber < result.TotalPages)
            {
                result.Links.Add(new LinkDto(
                   _linkGenerator.GetUriByName(HttpContext, "GetUnidades", new { nome, status, sortBy, sortDirection, page = result.PageNumber + 1, pageSize = result.PageSize }) ?? string.Empty,
                   "next", "GET"));

                result.Links.Add(new LinkDto(
                   _linkGenerator.GetUriByName(HttpContext, "GetUnidades", new { nome, status, sortBy, sortDirection, page = result.TotalPages, pageSize = result.PageSize }) ?? string.Empty,
                   "last", "GET"));
            }
        }
    }

    // Classe auxiliar para formatar a resposta paginada com links HATEOAS
    public class PagedResultWithLinks<T> where T : class
    {
        public List<T> Items { get; set; } = new List<T>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public List<LinkDto> Links { get; set; } = new List<LinkDto>();
    }
}
