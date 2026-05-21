using Medix.Models.Dtos;
using Medix.Services.Audit;
using Medix.Services.Paciente;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Medix.Controllers
{
    [Route("api/unidades/{unidadeId}/pacientes")]
    [ApiController]
    [Authorize]
    [EnableRateLimiting("api")]
    public class PacientesApiController : ControllerBase
    {
        private readonly IPacienteService _pacienteService;
        private readonly IAuditoriaService _auditoria;
        private readonly LinkGenerator _linkGenerator;

        public PacientesApiController(
            IPacienteService pacienteService,
            IAuditoriaService auditoria,
            LinkGenerator linkGenerator)
        {
            _pacienteService = pacienteService;
            _auditoria = auditoria;
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
            if (!await _pacienteService.UnidadeExisteAsync(unidadeId))
                return NotFound(new { message = "Unidade médica não encontrada." });

            var (items, total) = await _pacienteService.BuscarAsync(unidadeId, nome, page, pageSize);

            var itemDtos = items.Select(p =>
            {
                var dto = MapToDto(p);
                GenerateItemLinks(dto, unidadeId);
                return dto;
            }).ToList();

            var result = new PagedResultWithLinks<PacienteDto>
            {
                Items = itemDtos,
                TotalCount = total,
                PageNumber = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(total / (double)pageSize)
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
            if (!await _pacienteService.UnidadeExisteAsync(unidadeId))
                return NotFound(new { message = "Unidade médica não encontrada." });

            var paciente = await _pacienteService.BuscarPorIdAsync(unidadeId, id);
            if (paciente == null) return NotFound();

            var dto = MapToDto(paciente);
            GenerateItemLinks(dto, unidadeId);
            return Ok(dto);
        }

        // POST: api/unidades/{unidadeId}/pacientes
        [HttpPost(Name = "CreatePaciente")]
        public async Task<ActionResult<PacienteDto>> PostPaciente(int unidadeId, [FromBody] PacienteCreateDto input)
        {
            if (!await _pacienteService.UnidadeExisteAsync(unidadeId))
                return NotFound(new { message = "Unidade médica não encontrada." });

            var paciente = await _pacienteService.CriarAsync(
                unidadeId, input.NomeCompleto, input.CPF,
                input.DataNascimento, input.Email, input.Telefone, input.Endereco);

            await _auditoria.RegistrarAsync(
                "Paciente", paciente.Id, "CREATE",
                User.Identity?.Name ?? "anonimo",
                new Dictionary<string, object?>
                {
                    ["nomeCompleto"] = paciente.NomeCompleto,
                    ["cpf"] = paciente.CPF,
                    ["unidadeId"] = unidadeId
                });

            var dto = MapToDto(paciente);
            GenerateItemLinks(dto, unidadeId);
            return CreatedAtRoute("GetPacienteById", new { unidadeId, id = paciente.Id }, dto);
        }

        // PUT: api/unidades/{unidadeId}/pacientes/{id}
        [HttpPut("{id}", Name = "UpdatePaciente")]
        public async Task<ActionResult<PacienteDto>> PutPaciente(int unidadeId, int id, [FromBody] PacienteUpdateDto input)
        {
            if (!await _pacienteService.UnidadeExisteAsync(unidadeId))
                return NotFound(new { message = "Unidade médica não encontrada." });

            var paciente = await _pacienteService.AtualizarAsync(
                unidadeId, id, input.NomeCompleto, input.CPF,
                input.DataNascimento, input.Email, input.Telefone, input.Endereco);

            if (paciente == null) return NotFound();

            await _auditoria.RegistrarAsync(
                "Paciente", id, "UPDATE",
                User.Identity?.Name ?? "anonimo",
                new Dictionary<string, object?> { ["nomeCompleto"] = paciente.NomeCompleto });

            var dto = MapToDto(paciente);
            GenerateItemLinks(dto, unidadeId);
            return Ok(dto);
        }

        // DELETE: api/unidades/{unidadeId}/pacientes/{id}
        [HttpDelete("{id}", Name = "DeletePaciente")]
        public async Task<IActionResult> DeletePaciente(int unidadeId, int id)
        {
            if (!await _pacienteService.UnidadeExisteAsync(unidadeId))
                return NotFound(new { message = "Unidade médica não encontrada." });

            var excluido = await _pacienteService.ExcluirAsync(unidadeId, id);
            if (!excluido) return NotFound();

            await _auditoria.RegistrarAsync(
                "Paciente", id, "DELETE",
                User.Identity?.Name ?? "anonimo",
                new Dictionary<string, object?> { ["id"] = id });

            return NoContent();
        }

        // --- Auxiliares ---

        private static PacienteDto MapToDto(Models.Paciente p) => new()
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
