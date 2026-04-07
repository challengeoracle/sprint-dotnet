using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Medix.Models
{
    public enum StatusUnidade
    {
        Ativa,
        Inativa,
        Suspensa,
        EmTeste
    }
    public class UnidadeMedica
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string CNPJ { get; set; } = string.Empty;
        public string? Endereco { get; set; }
        public string? Telefone { get; set; }
        public string EmailAdmin { get; set; } = string.Empty; // Este é o email de *contato*, não o de login
        public StatusUnidade Status { get; set; }
        public DateTime DataCadastro { get; set; }

        // Chave estrangeira para o IdentityUser que administra essa unidade
        public string? AdministradorUserId { get; set; }

        [ForeignKey("AdministradorUserId")]
        public virtual IdentityUser? AdministradorUser { get; set; }
    }
}