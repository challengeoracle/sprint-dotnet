using Medix.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Alterado de AddDefaultIdentity para AddIdentity para incluir suporte a Roles
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    // Define o caminho para a página de login
    options.LoginPath = $"/Identity/Account/Login";
    // Define o caminho para a página de logout
    options.LogoutPath = $"/Identity/Account/Logout";
    // Define o caminho para a página de acesso negado
    options.AccessDeniedPath = $"/Identity/Account/AccessDenied";
});

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages(); // Garante que as páginas do Identity (login, etc) funcionem

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.MapControllerRoute(
    name: "UnidadeSaudeArea",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Bloco para criar os papéis (Roles) e o usuário Admin na inicialização
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        // 1. Pega o contexto do banco de dados
        var context = services.GetRequiredService<Medix.Data.ApplicationDbContext>();

        // 2. Aplica as migrations (CRIA O BANCO DE DADOS SE NÃO EXISTIR)
        // Isso vai recriar o 'HospitalManagerDb' e todas as tabelas.
        logger.LogInformation("Aplicando migrações do banco de dados...");
        await context.Database.MigrateAsync();
        logger.LogInformation("Migrações aplicadas com sucesso.");

        // 3. Chama o nosso seeder para criar os papéis e o admin
        logger.LogInformation("Criando papéis e usuário admin...");
        await Medix.Data.IdentitySeedData.CreateRolesAndAdminUserAsync(services);
        logger.LogInformation("Papéis e usuário admin criados com sucesso.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Um erro ocorreu ao inicializar o banco de dados.");
    }
}

app.Run();