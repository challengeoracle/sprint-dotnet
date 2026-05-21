using Medix.Models.Audit;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Medix.Data.Mongo
{
    /// <summary>
    /// Contexto de acesso ao MongoDB, análogo ao ApplicationDbContext do EF Core.
    /// Registrado como Singleton — o MongoClient é thread-safe por design.
    /// </summary>
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(IOptions<MongoDbSettings> settings)
        {
            var client = new MongoClient(settings.Value.ConnectionString);
            _database = client.GetDatabase(settings.Value.DatabaseName);
        }

        /// <summary>
        /// Coleção de logs de auditoria.
        /// </summary>
        public virtual IMongoCollection<LogAuditoria> LogsAuditoria =>
            _database.GetCollection<LogAuditoria>("LogsAuditoria");
    }
}