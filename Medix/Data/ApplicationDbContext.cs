using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Medix.Models;

namespace Medix.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        public DbSet<Paciente> Pacientes { get; set; }
        public DbSet<Colaborador> Colaboradores { get; set; }
        public DbSet<UnidadeMedica> UnidadesMedicas { get; set; }

        // Oracle < 23c não suporta BOOLEAN em DDL — mapeia para NUMBER(1)
        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.Properties<bool>().HaveColumnType("NUMBER(1)");
            configurationBuilder.Properties<bool?>().HaveColumnType("NUMBER(1)");
        }
    }
}