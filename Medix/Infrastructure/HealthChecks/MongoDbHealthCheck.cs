using Medix.Data.Mongo;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MongoDB.Driver;

namespace Medix.Infrastructure.HealthChecks
{
    /// <summary>
    /// Health check que verifica a conectividade com o MongoDB
    /// executando um ping no servidor configurado.
    /// </summary>
    public class MongoDbHealthCheck : IHealthCheck
    {
        private readonly MongoDbContext _mongo;

        public MongoDbHealthCheck(MongoDbContext mongo)
        {
            _mongo = mongo;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Tenta listar os nomes de banco — operação leve que valida a conexão
                var cursor = await _mongo.LogsAuditoria.Database
                    .Client
                    .ListDatabaseNamesAsync(cancellationToken);

                await cursor.MoveNextAsync(cancellationToken);

                return HealthCheckResult.Healthy("MongoDB conectado.");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy(
                    "MongoDB indisponível.",
                    exception: ex);
            }
        }
    }
}
