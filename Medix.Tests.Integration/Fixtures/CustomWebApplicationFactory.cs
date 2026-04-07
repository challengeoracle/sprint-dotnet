using Medix.Data;
using Medix.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Medix.Tests.Integration.Fixtures;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remover todos os descritores relacionados ao ApplicationDbContext
            var dbDescriptors = services
                .Where(d =>
                    d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>) ||
                    d.ServiceType == typeof(ApplicationDbContext))
                .ToList();
            foreach (var d in dbDescriptors)
                services.Remove(d);

            // Remover configurações internas do EF Core (IDbContextOptionsConfiguration<T>)
            var internalConfigs = services
                .Where(d => d.ServiceType.FullName?.Contains("DbContextOptionsConfiguration") == true)
                .ToList();
            foreach (var d in internalConfigs)
                services.Remove(d);

            // Criar IServiceProvider interno isolado para o InMemory
            var inMemorySp = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            services.AddDbContext<ApplicationDbContext>(options =>
                options
                    .UseInMemoryDatabase("MedixTestDb")
                    .UseInternalServiceProvider(inMemorySp));

            // Autenticação fake para testes — substituir o esquema padrão do Identity pelo TestScheme
            // para que [Authorize] aceite o handler de teste sem precisar de login real
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "TestScheme";
                options.DefaultChallengeScheme = "TestScheme";
                options.DefaultScheme = "TestScheme";
            })
            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("TestScheme", _ => { });

            // Seed de dados de teste
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Database.EnsureCreated();
            SeedTestData(db);
        });

        builder.UseEnvironment("Test");
    }

    private static void SeedTestData(ApplicationDbContext db)
    {
        if (db.UnidadesMedicas.Any()) return;

        db.UnidadesMedicas.AddRange(
            new UnidadeMedica
            {
                Nome = "Hospital Teste Alpha",
                CNPJ = "11.111.111/0001-11",
                EmailAdmin = "alpha@test.com",
                Status = StatusUnidade.Ativa,
                DataCadastro = new DateTime(2024, 1, 1)
            },
            new UnidadeMedica
            {
                Nome = "Clínica Beta",
                CNPJ = "22.222.222/0001-22",
                EmailAdmin = "beta@test.com",
                Status = StatusUnidade.Inativa,
                DataCadastro = new DateTime(2024, 2, 1)
            },
            new UnidadeMedica
            {
                Nome = "UPA Gama",
                CNPJ = "33.333.333/0001-33",
                EmailAdmin = "gama@test.com",
                Status = StatusUnidade.Ativa,
                DataCadastro = new DateTime(2024, 3, 1)
            }
        );
        db.SaveChanges();
    }
}
