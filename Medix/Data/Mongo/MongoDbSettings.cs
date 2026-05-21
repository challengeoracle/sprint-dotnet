namespace Medix.Data.Mongo
{
    /// <summary>
    /// Configurações da conexão com o MongoDB, mapeadas da seção
    /// "MongoDb" no appsettings.json.
    /// </summary>
    public class MongoDbSettings
    {
        public string ConnectionString { get; set; } = string.Empty;
        public string DatabaseName { get; set; } = "MedixAudit";
        public string LogsCollectionName { get; set; } = "LogsAuditoria";
    }
}
