using Microsoft.AspNetCore.Identity;

namespace Medix.Data
{
    public static class IdentitySeedData
    {
        private const string roleEquipeMedix = "EquipeMedix";
        private const string roleUnidadeSaude = "UnidadeSaude";

        // Método principal que será chamado no Program.cs
        public static async Task CreateRolesAndAdminUserAsync(IServiceProvider serviceProvider)
        {
            // Pega os serviços necessários (RoleManager e UserManager)
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

            // --- Criar Papéis (Roles) ---

            // Verifica se o papel "EquipeMedix" já existe
            if (!await roleManager.RoleExistsAsync(roleEquipeMedix))
            {
                // Se não existir, cria o papel
                await roleManager.CreateAsync(new IdentityRole(roleEquipeMedix));
            }

            // Verifica se o papel "UnidadeSaude" já existe
            if (!await roleManager.RoleExistsAsync(roleUnidadeSaude))
            {
                // Se não existir, cria o papel
                await roleManager.CreateAsync(new IdentityRole(roleUnidadeSaude));
            }

            // --- Criar a Conta Fixa da Equipe (Admin) ---
            // Define o email do usuário admin da equipe
            var adminEmail = "admin@medix.com";

            // Verifica se o usuário admin já existe
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                // Se não existir, cria o usuário admin
                adminUser = new IdentityUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true // Já confirma o email
                };

                // Define a senha do admin
                var result = await userManager.CreateAsync(adminUser, "Medix123@");

                if (result.Succeeded)
                {
                    // Se o usuário foi criado com sucesso, associa ele ao papel "EquipeMedix"
                    await userManager.AddToRoleAsync(adminUser, roleEquipeMedix);
                }
            }
        }
    }
}