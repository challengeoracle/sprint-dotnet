using Medix.Data.Mongo;
using Medix.Models.Audit;
using MongoDB.Driver;

namespace Medix.Services.Audit
{
    /// <summary>
    /// Implementação do serviço de auditoria usando MongoDB.
    /// Todas as operações são async e não bloqueiam a thread da requisição.
    /// </summary>
    public class AuditoriaService : IAuditoriaService
    {
        private readonly MongoDbContext _mongo;
        private readonly ILogger<AuditoriaService> _logger;

        public AuditoriaService(MongoDbContext mongo, ILogger<AuditoriaService> logger)
        {
            _mongo = mongo;
            _logger = logger;
        }

        public async Task RegistrarAsync(
            string entidade,
            int entidadeId,
            string operacao,
            string realizadoPor,
            Dictionary<string, object?> detalhe)
        {
            var log = new LogAuditoria
            {
                Entidade = entidade,
                EntidadeId = entidadeId,
                Operacao = operacao,
                RealizadoPor = realizadoPor,
                RealizadoEm = DateTime.UtcNow,
                Detalhe = detalhe
            };

            try
            {
                await _mongo.LogsAuditoria.InsertOneAsync(log);
                _logger.LogInformation(
                    "Auditoria registrada: {Operacao} em {Entidade} (Id={EntidadeId}) por {Usuario}",
                    operacao, entidade, entidadeId, realizadoPor);
            }
            catch (Exception ex)
            {
                // Auditoria não deve quebrar o fluxo principal da aplicação
                _logger.LogError(ex,
                    "Falha ao registrar auditoria: {Operacao} em {Entidade} (Id={EntidadeId})",
                    operacao, entidade, entidadeId);
            }
        }

        public async Task<List<LogAuditoria>> ObterPorEntidadeAsync(string entidade, int entidadeId)
        {
            var filtro = Builders<LogAuditoria>.Filter.And(
                Builders<LogAuditoria>.Filter.Eq(l => l.Entidade, entidade),
                Builders<LogAuditoria>.Filter.Eq(l => l.EntidadeId, entidadeId));

            var ordenacao = Builders<LogAuditoria>.Sort.Descending(l => l.RealizadoEm);

            return await _mongo.LogsAuditoria
                .Find(filtro)
                .Sort(ordenacao)
                .ToListAsync();
        }

        public async Task<List<LogAuditoria>> ObterRecentesAsync(int limite = 50)
        {
            var ordenacao = Builders<LogAuditoria>.Sort.Descending(l => l.RealizadoEm);

            return await _mongo.LogsAuditoria
                .Find(Builders<LogAuditoria>.Filter.Empty)
                .Sort(ordenacao)
                .Limit(limite)
                .ToListAsync();
        }
    }
}
