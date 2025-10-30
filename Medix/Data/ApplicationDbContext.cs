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
    }
}