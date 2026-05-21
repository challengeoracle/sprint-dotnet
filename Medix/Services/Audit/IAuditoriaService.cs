using Medix.Models.Audit;

namespace Medix.Services.Audit
{
    /// <summary>
    /// Contrato para registro e consulta de logs de auditoria no MongoDB.
    /// </summary>
    public interface IAuditoriaService
    {
        /// <summary>
        /// Registra uma operação realizada sobre uma entidade.
        /// </summary>
        Task RegistrarAsync(
            string entidade,
            int entidadeId,
            string operacao,
            string realizadoPor,
            Dictionary<string, object?> detalhe);

        /// <summary>
        /// Retorna todos os logs de uma entidade específica, ordenados do mais recente para o mais antigo.
        /// </summary>
        Task<List<LogAuditoria>> ObterPorEntidadeAsync(string entidade, int entidadeId);

        /// <summary>
        /// Retorna os últimos N logs do sistema, para o endpoint de auditoria geral.
        /// </summary>
        Task<List<LogAuditoria>> ObterRecentesAsync(int limite = 50);
    }
}
