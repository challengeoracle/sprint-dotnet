using Medix.Models;
using Medix.Models.Dtos;
using Medix.Services.Audit;
using Medix.Services.Colaborador;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Medix.Controllers
{
    [Route("api/unidades/{unidadeId}/colaboradores")]
    [ApiController]
    [Authorize]
    [EnableRateLimiting("api")]
    public class ColaboradoresApiController : ControllerBase
    {
        private readonly IColaboradorService _colaboradorService;
        private readonly IAuditoriaService _auditoria;
        private readonly LinkGenerator _linkGenerator;

        public ColaboradoresApiController(
            IColaboradorService colaboradorService,
            IAuditoriaService auditoria,
            LinkGenerator linkGenerator)
        {
            _colaboradorService = colaboradorService;
            _auditoria = auditoria;
            _linkGenerator = linkGenerator;
        }

        // GET: api/unidades/{unidadeId}/colaboradores
        [HttpGet(Name = "GetColaboradores")]
        public async Task<ActionResult<PagedResultWithLinks<ColaboradorDto>>> GetColaboradores(
            int unidadeId,
            [FromQuery] string? nome = null,
            [FromQuery] TipoColaborador? cargo = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            if (!await _colaboradorService.UnidadeExisteAsync(unidadeId))
                return NotFound(new { message = "Unidade médica não encontrada." });

            var (items, total) = await _colaboradorService.BuscarAsync(unidadeId, nome, cargo, page, pageSize);

            var itemDtos = items.Select(c =>
            {
                var dto = MapToDto(c);
                GenerateItemLinks(dto, unidadeId);
                return dto;
            }).ToList();

            var result = new PagedResultWithLinks<ColaboradorDto>
            {
                Items = itemDtos,
                TotalCount = total,
                PageNumber = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(total / (double)pageSize)
            };

            result.Links.Add(new LinkDto(
                _linkGenerator.GetUriByName(HttpContext, "GetColaboradores", new { unidadeId, nome, cargo, page, pageSize }) ?? string.Empty,
                "self", "GET"));

            if (page > 1)
            {
                result.Links.Add(new LinkDto(
                    _linkGenerator.GetUriByName(HttpContext, "GetColaboradores", new { unidadeId, nome, cargo, page = 1, pageSize }) ?? string.Empty,
                    "first", "GET"));
                result.Links.Add(new LinkDto(
                    _linkGenerator.GetUriByName(HttpContext, "GetColaboradores", new { unidadeId, nome, cargo, page = page - 1, pageSize }) ?? string.Empty,
                    "previous", "GET"));
            }

            if (page < result.TotalPages)
            {
                result.Links.Add(new LinkDto(
                    _linkGenerator.GetUriByName(HttpContext, "GetColaboradores", new { unidadeId, nome, cargo, page = page + 1, pageSize }) ?? string.Empty,
                    "next", "GET"));
                result.Links.Add(new LinkDto(
                    _linkGenerator.GetUriByName(HttpContext, "GetColaboradores", new { unidadeId, nome, cargo, page = result.TotalPages, pageSize }) ?? string.Empty,
                    "last", "GET"));
            }

            return Ok(result);
        }

        // GET: api/unidades/{unidadeId}/colaboradores/{id}
        [HttpGet("{id}", Name = "GetColaboradorById")]
        public async Task<ActionResult<ColaboradorDto>> GetColaborador(int unidadeId, int id)
        {
            if (!await _colaboradorService.UnidadeExisteAsync(unidadeId))
                return NotFound(new { message = "Unidade médica não encontrada." });

            var colaborador = await _colaboradorService.BuscarPorIdAsync(unidadeId, id);
            if (colaborador == null) return NotFound();

            var dto = MapToDto(colaborador);
            GenerateItemLinks(dto, unidadeId);
            return Ok(dto);
        }

        // POST: api/unidades/{unidadeId}/colaboradores
        [HttpPost(Name = "CreateColaborador")]
        public async Task<ActionResult<ColaboradorDto>> PostColaborador(int unidadeId, [FromBody] ColaboradorCreateDto input)
        {
            if (!await _colaboradorService.UnidadeExisteAsync(unidadeId))
                return NotFound(new { message = "Unidade médica não encontrada." });

            var colaborador = await _colaboradorService.CriarAsync(
                unidadeId, input.NomeCompleto, input.Email,
                input.Cargo, input.Especialidade, input.RegistroProfissional);

            await _auditoria.RegistrarAsync(
                "Colaborador", colaborador.Id, "CREATE",
                User.Identity?.Name ?? "anonimo",
                new Dictionary<string, object?>
                {
                    ["nomeCompleto"] = colaborador.NomeCompleto,
                    ["cargo"] = colaborador.Cargo.ToString(),
                    ["unidadeId"] = unidadeId
                });

            var dto = MapToDto(colaborador);
            GenerateItemLinks(dto, unidadeId);
            return CreatedAtRoute("GetColaboradorById", new { unidadeId, id = colaborador.Id }, dto);
        }

        // PUT: api/unidades/{unidadeId}/colaboradores/{id}
        [HttpPut("{id}", Name = "UpdateColaborador")]
        public async Task<ActionResult<ColaboradorDto>> PutColaborador(int unidadeId, int id, [FromBody] ColaboradorUpdateDto input)
        {
            if (!await _colaboradorService.UnidadeExisteAsync(unidadeId))
                return NotFound(new { message = "Unidade médica não encontrada." });

            var colaborador = await _colaboradorService.AtualizarAsync(
                unidadeId, id, input.NomeCompleto, input.Email,
                input.Cargo, input.Especialidade, input.RegistroProfissional);

            if (colaborador == null) return NotFound();

            await _auditoria.RegistrarAsync(
                "Colaborador", id, "UPDATE",
                User.Identity?.Name ?? "anonimo",
                new Dictionary<string, object?> { ["nomeCompleto"] = colaborador.NomeCompleto });

            var dto = MapToDto(colaborador);
            GenerateItemLinks(dto, unidadeId);
            return Ok(dto);
        }

        // DELETE: api/unidades/{unidadeId}/colaboradores/{id}
        [HttpDelete("{id}", Name = "DeleteColaborador")]
        public async Task<IActionResult> DeleteColaborador(int unidadeId, int id)
        {
            if (!await _colaboradorService.UnidadeExisteAsync(unidadeId))
                return NotFound(new { message = "Unidade médica não encontrada." });

            var excluido = await _colaboradorService.ExcluirAsync(unidadeId, id);
            if (!excluido) return NotFound();

            await _auditoria.RegistrarAsync(
                "Colaborador", id, "DELETE",
                User.Identity?.Name ?? "anonimo",
                new Dictionary<string, object?> { ["id"] = id });

            return NoContent();
        }

        // --- Auxiliares ---

        private static ColaboradorDto MapToDto(Models.Colaborador c) => new()
        {
            Id = c.Id,
            NomeCompleto = c.NomeCompleto,
            Email = c.Email,
            Cargo = c.Cargo,
            Especialidade = c.Especialidade,
            RegistroProfissional = c.RegistroProfissional,
            UnidadeMedicaId = c.UnidadeMedicaId
        };

        private void GenerateItemLinks(ColaboradorDto dto, int unidadeId)
        {
            dto.Links.Add(new LinkDto(
                _linkGenerator.GetUriByName(HttpContext, "GetColaboradorById", new { unidadeId, id = dto.Id }) ?? string.Empty,
                "self", "GET"));
            dto.Links.Add(new LinkDto(
                _linkGenerator.GetUriByName(HttpContext, "UpdateColaborador", new { unidadeId, id = dto.Id }) ?? string.Empty,
                "update", "PUT"));
            dto.Links.Add(new LinkDto(
                _linkGenerator.GetUriByName(HttpContext, "DeleteColaborador", new { unidadeId, id = dto.Id }) ?? string.Empty,
                "delete", "DELETE"));
            dto.Links.Add(new LinkDto(
                _linkGenerator.GetUriByName(HttpContext, "GetUnidadeById", new { id = unidadeId }) ?? string.Empty,
                "unidade", "GET"));
        }
    }
}
