using Medix.Services.Audit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Medix.Controllers
{
    /// <summary>
    /// Endpoints de consulta do log de auditoria (persistido no MongoDB).
    /// Apenas a EquipeMedix tem acesso.
    /// </summary>
    [Route("api/auditoria")]
    [ApiController]
    [Authorize(Roles = "EquipeMedix")]
    [EnableRateLimiting("api")]
    public class AuditoriaApiController : ControllerBase
    {
        private readonly IAuditoriaService _auditoria;

        public AuditoriaApiController(IAuditoriaService auditoria)
        {
            _auditoria = auditoria;
        }

        /// <summary>
        /// Retorna os últimos registros de auditoria do sistema.
        /// </summary>
        /// <param name="limite">Quantidade máxima de registros (padrão 50, máximo 200).</param>
        [HttpGet(Name = "GetLogsAuditoria")]
        [ProducesResponseType(typeof(IEnumerable<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetRecentes([FromQuery] int limite = 50)
        {
            if (limite < 1) limite = 1;
            if (limite > 200) limite = 200;

            var logs = await _auditoria.ObterRecentesAsync(limite);
            return Ok(logs);
        }

        /// <summary>
        /// Retorna todos os logs de uma entidade e registro específico.
        /// </summary>
        /// <param name="entidade">Nome da entidade: UnidadeMedica, Paciente ou Colaborador.</param>
        /// <param name="id">Id do registro na base relacional.</param>
        [HttpGet("{entidade}/{id:int}", Name = "GetLogsPorEntidade")]
        [ProducesResponseType(typeof(IEnumerable<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetPorEntidade(string entidade, int id)
        {
            var entidadesValidas = new[] { "UnidadeMedica", "Paciente", "Colaborador" };
            if (!entidadesValidas.Contains(entidade))
                return BadRequest(new { message = $"Entidade inválida. Use: {string.Join(", ", entidadesValidas)}" });

            var logs = await _auditoria.ObterPorEntidadeAsync(entidade, id);
            return Ok(logs);
        }
    }
}
