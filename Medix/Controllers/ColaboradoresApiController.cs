using Medix.Data;
using Medix.Models;
using Medix.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace Medix.Controllers
{
    [Route("api/unidades/{unidadeId}/colaboradores")]
    [ApiController]
    [Authorize]
    [EnableRateLimiting("api")]
    public class ColaboradoresApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly LinkGenerator _linkGenerator;

        public ColaboradoresApiController(ApplicationDbContext context, LinkGenerator linkGenerator)
        {
            _context = context;
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
            if (!await UnidadeExisteAsync(unidadeId))
                return NotFound(new { message = "Unidade médica não encontrada." });

            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            var query = _context.Colaboradores
                .Where(c => c.UnidadeMedicaId == unidadeId);

            if (!string.IsNullOrWhiteSpace(nome))
                query = query.Where(c => c.NomeCompleto.Contains(nome));

            if (cargo.HasValue)
                query = query.Where(c => c.Cargo == cargo.Value);

            query = query.OrderBy(c => c.NomeCompleto);

            var totalCount = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            var itemDtos = items.Select(c =>
            {
                var dto = MapToDto(c);
                GenerateItemLinks(dto, unidadeId);
                return dto;
            }).ToList();

            var result = new PagedResultWithLinks<ColaboradorDto>
            {
                Items = itemDtos,
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
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
            if (!await UnidadeExisteAsync(unidadeId))
                return NotFound(new { message = "Unidade médica não encontrada." });

            var colaborador = await _context.Colaboradores
                .FirstOrDefaultAsync(c => c.Id == id && c.UnidadeMedicaId == unidadeId);

            if (colaborador == null)
                return NotFound();

            var dto = MapToDto(colaborador);
            GenerateItemLinks(dto, unidadeId);

            return Ok(dto);
        }

        // POST: api/unidades/{unidadeId}/colaboradores
        [HttpPost(Name = "CreateColaborador")]
        public async Task<ActionResult<ColaboradorDto>> PostColaborador(int unidadeId, [FromBody] ColaboradorCreateDto input)
        {
            if (!await UnidadeExisteAsync(unidadeId))
                return NotFound(new { message = "Unidade médica não encontrada." });

            var colaborador = new Colaborador
            {
                NomeCompleto = input.NomeCompleto,
                Email = input.Email,
                Cargo = input.Cargo,
                Especialidade = input.Especialidade,
                RegistroProfissional = input.RegistroProfissional,
                UnidadeMedicaId = unidadeId
            };

            _context.Colaboradores.Add(colaborador);
            await _context.SaveChangesAsync();

            var dto = MapToDto(colaborador);
            GenerateItemLinks(dto, unidadeId);

            return CreatedAtRoute("GetColaboradorById", new { unidadeId, id = colaborador.Id }, dto);
        }

        // PUT: api/unidades/{unidadeId}/colaboradores/{id}
        [HttpPut("{id}", Name = "UpdateColaborador")]
        public async Task<ActionResult<ColaboradorDto>> PutColaborador(int unidadeId, int id, [FromBody] ColaboradorUpdateDto input)
        {
            if (!await UnidadeExisteAsync(unidadeId))
                return NotFound(new { message = "Unidade médica não encontrada." });

            var colaborador = await _context.Colaboradores
                .FirstOrDefaultAsync(c => c.Id == id && c.UnidadeMedicaId == unidadeId);

            if (colaborador == null)
                return NotFound();

            colaborador.NomeCompleto = input.NomeCompleto;
            colaborador.Email = input.Email;
            colaborador.Cargo = input.Cargo;
            colaborador.Especialidade = input.Especialidade;
            colaborador.RegistroProfissional = input.RegistroProfissional;

            await _context.SaveChangesAsync();

            var dto = MapToDto(colaborador);
            GenerateItemLinks(dto, unidadeId);

            return Ok(dto);
        }

        // DELETE: api/unidades/{unidadeId}/colaboradores/{id}
        [HttpDelete("{id}", Name = "DeleteColaborador")]
        public async Task<IActionResult> DeleteColaborador(int unidadeId, int id)
        {
            if (!await UnidadeExisteAsync(unidadeId))
                return NotFound(new { message = "Unidade médica não encontrada." });

            var colaborador = await _context.Colaboradores
                .FirstOrDefaultAsync(c => c.Id == id && c.UnidadeMedicaId == unidadeId);

            if (colaborador == null)
                return NotFound();

            _context.Colaboradores.Remove(colaborador);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // --- Auxiliares ---

        private async Task<bool> UnidadeExisteAsync(int unidadeId) =>
            await _context.UnidadesMedicas.AnyAsync(u => u.Id == unidadeId);

        private static ColaboradorDto MapToDto(Colaborador c) => new()
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
