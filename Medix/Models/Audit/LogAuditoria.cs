using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Medix.Models.Audit
{
    /// <summary>
    /// Documento de auditoria persistido no MongoDB.
    /// Registra cada operação de criação, atualização ou exclusão
    /// realizada sobre as entidades do sistema.
    /// </summary>
    public class LogAuditoria
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        /// <summary>
        /// Entidade afetada: UnidadeMedica, Paciente ou Colaborador.
        /// </summary>
        public string Entidade { get; set; } = string.Empty;

        /// <summary>
        /// Id numérico do registro afetado na base relacional.
        /// </summary>
        public int EntidadeId { get; set; }

        /// <summary>
        /// Operação realizada: CREATE, UPDATE ou DELETE.
        /// </summary>
        public string Operacao { get; set; } = string.Empty;

        /// <summary>
        /// Email ou identificador do usuário que realizou a operação.
        /// </summary>
        public string RealizadoPor { get; set; } = "sistema";

        /// <summary>
        /// Momento exato da operação (UTC).
        /// </summary>
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime RealizadoEm { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Snapshot do estado da entidade após a operação (JSON livre).
        /// Permite reconstruir o histórico de mudanças.
        /// </summary>
        public Dictionary<string, object?> Detalhe { get; set; } = new();
    }
}
