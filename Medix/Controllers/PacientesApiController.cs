using Medix.Data;
using Medix.Models;
using Medix.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace Medix.Controllers
{
    [Route("api/unidades/{unidadeId}/pacientes")]
    [ApiController]
    [Authorize]
    [EnableRateLimiting("api")]
    public class PacientesApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly LinkGenerator _linkGenerator;

        public PacientesApiController(ApplicationDbContext context, LinkGenerator linkGenerator)
        {
            _context = context;
            _linkGenerator = linkGenerator;
        }

        // GET: api/unidades/{unidadeId}/pacientes
        [HttpGet(Name = "GetPacientes")]
        public async Task<ActionResult<PagedResultWithLinks<PacienteDto>>> GetPacientes(
            int unidadeId,
            [FromQuery] string? nome = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            if (!await UnidadeExisteAsync(unidadeId))
                return NotFound(new { message = "Unidade médica não encontrada." });

            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            var query = _context.Pacientes
                .Where(p => p.UnidadeMedicaId == unidadeId);

            if (!string.IsNullOrWhiteSpace(nome))
                query = query.Where(p => p.NomeCompleto.Contains(nome));

            query = query.OrderBy(p => p.NomeCompleto);

            var totalCount = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            var itemDtos = items.Select(p =>
            {
                var dto = MapToDto(p);
                GenerateItemLinks(dto, unidadeId);
                return dto;
            }).ToList();

            var result = new PagedResultWithLinks<PacienteDto>
            {
                Items = itemDtos,
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };

            result.Links.Add(new LinkDto(
                _linkGenerator.GetUriByName(HttpContext, "GetPacientes", new { unidadeId, nome, page, pageSize }) ?? string.Empty,
                "self", "GET"));

            if (page > 1)
            {
                result.Links.Add(new LinkDto(
                    _linkGenerator.GetUriByName(HttpContext, "GetPacientes", new { unidadeId, nome, page = 1, pageSize }) ?? string.Empty,
                    "first", "GET"));
                result.Links.Add(new LinkDto(
                    _linkGenerator.GetUriByName(HttpContext, "GetPacientes", new { unidadeId, nome, page = page - 1, pageSize }) ?? string.Empty,
                    "previous", "GET"));
            }

            if (page < result.TotalPages)
            {
                result.Links.Add(new LinkDto(
                    _linkGenerator.GetUriByName(HttpContext, "GetPacientes", new { unidadeId, nome, page = page + 1, pageSize }) ?? string.Empty,
                    "next", "GET"));
                result.Links.Add(new LinkDto(
                    _linkGenerator.GetUriByName(HttpContext, "GetPacientes", new { unidadeId, nome, page = result.TotalPages, pageSize }) ?? string.Empty,
                    "last", "GET"));
            }

            return Ok(result);
        }

        // GET: api/unidades/{unidadeId}/pacientes/{id}
        [HttpGet("{id}", Name = "GetPacienteById")]
        public async Task<ActionResult<PacienteDto>> GetPaciente(int unidadeId, int id)
        {
            if (!await UnidadeExisteAsync(unidadeId))
                return NotFound(new { message = "Unidade médica não encontrada." });

            var paciente = await _context.Pacientes
                .FirstOrDefaultAsync(p => p.Id == id && p.UnidadeMedicaId == unidadeId);

            if (paciente == null)
                return NotFound();

            var dto = MapToDto(paciente);
            GenerateItemLinks(dto, unidadeId);

            return Ok(dto);
        }

        // POST: api/unidades/{unidadeId}/pacientes
        [HttpPost(Name = "CreatePaciente")]
        public async Task<ActionResult<PacienteDto>> PostPaciente(int unidadeId, [FromBody] PacienteCreateDto input)
        {
            if (!await UnidadeExisteAsync(unidadeId))
                return NotFound(new { message = "Unidade médica não encontrada." });

            var paciente = new Paciente
            {
                NomeCompleto = input.NomeCompleto,
                CPF = input.CPF,
                DataNascimento = input.DataNascimento,
                Email = input.Email,
                Telefone = input.Telefone,
                Endereco = input.Endereco,
                UnidadeMedicaId = unidadeId
            };

            _context.Pacientes.Add(paciente);
            await _context.SaveChangesAsync();

            var dto = MapToDto(paciente);
            GenerateItemLinks(dto, unidadeId);

            return CreatedAtRoute("GetPacienteById", new { unidadeId, id = paciente.Id }, dto);
        }

        // PUT: api/unidades/{unidadeId}/pacientes/{id}
        [HttpPut("{id}", Name = "UpdatePaciente")]
        public async Task<ActionResult<PacienteDto>> PutPaciente(int unidadeId, int id, [FromBody] PacienteUpdateDto input)
        {
            if (!await UnidadeExisteAsync(unidadeId))
                return NotFound(new { message = "Unidade médica não encontrada." });

            var paciente = await _context.Pacientes
                .FirstOrDefaultAsync(p => p.Id == id && p.UnidadeMedicaId == unidadeId);

            if (paciente == null)
                return NotFound();

            paciente.NomeCompleto = input.NomeCompleto;
            paciente.CPF = input.CPF;
            paciente.DataNascimento = input.DataNascimento;
            paciente.Email = input.Email;
            paciente.Telefone = input.Telefone;
            paciente.Endereco = input.Endereco;

            await _context.SaveChangesAsync();

            var dto = MapToDto(paciente);
            GenerateItemLinks(dto, unidadeId);

            return Ok(dto);
        }

        // DELETE: api/unidades/{unidadeId}/pacientes/{id}
        [HttpDelete("{id}", Name = "DeletePaciente")]
        public async Task<IActionResult> DeletePaciente(int unidadeId, int id)
        {
            if (!await UnidadeExisteAsync(unidadeId))
                return NotFound(new { message = "Unidade médica não encontrada." });

            var paciente = await _context.Pacientes
                .FirstOrDefaultAsync(p => p.Id == id && p.UnidadeMedicaId == unidadeId);

            if (paciente == null)
                return NotFound();

            _context.Pacientes.Remove(paciente);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // --- Auxiliares ---

        private async Task<bool> UnidadeExisteAsync(int unidadeId) =>
            await _context.UnidadesMedicas.AnyAsync(u => u.Id == unidadeId);

        private static PacienteDto MapToDto(Paciente p) => new()
        {
            Id = p.Id,
            NomeCompleto = p.NomeCompleto,
            CPF = p.CPF,
            DataNascimento = p.DataNascimento,
            Email = p.Email,
            Telefone = p.Telefone,
            Endereco = p.Endereco,
            UnidadeMedicaId = p.UnidadeMedicaId
        };

        private void GenerateItemLinks(PacienteDto dto, int unidadeId)
        {
            dto.Links.Add(new LinkDto(
                _linkGenerator.GetUriByName(HttpContext, "GetPacienteById", new { unidadeId, id = dto.Id }) ?? string.Empty,
                "self", "GET"));
            dto.Links.Add(new LinkDto(
                _linkGenerator.GetUriByName(HttpContext, "UpdatePaciente", new { unidadeId, id = dto.Id }) ?? string.Empty,
                "update", "PUT"));
            dto.Links.Add(new LinkDto(
                _linkGenerator.GetUriByName(HttpContext, "DeletePaciente", new { unidadeId, id = dto.Id }) ?? string.Empty,
                "delete", "DELETE"));
            dto.Links.Add(new LinkDto(
                _linkGenerator.GetUriByName(HttpContext, "GetUnidadeById", new { id = unidadeId }) ?? string.Empty,
                "unidade", "GET"));
        }
    }
}
