using Medix.Data;
using Medix.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Medix.Areas.UnidadeSaude.Controllers
{
    [Area("UnidadeSaude")]
    [Authorize(Roles = "UnidadeSaude")] // Só utilizadores da unidade podem aceder
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public DashboardController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Método auxiliar para pegar a Unidade Médica do usuário logado
        private async Task<UnidadeMedica?> GetUserUnidadeMedicaAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return null;

            return await _context.UnidadesMedicas
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.AdministradorUserId == userId);
        }

        // GET: /UnidadeSaude/Dashboard/Index
        public async Task<IActionResult> Index()
        {
            var unidade = await GetUserUnidadeMedicaAsync();
            if (unidade == null)
            {
                return Unauthorized("A sua conta de utilizador não está vinculada a nenhuma unidade de saúde.");
            }

            // Passa o nome da unidade para a View
            ViewData["NomeUnidade"] = unidade.Nome;

            // Lógica futura: Buscar estatísticas de pacientes/colaboradores desta unidade
            ViewData["TotalPacientes"] = await _context.Pacientes.CountAsync(p => p.UnidadeMedicaId == unidade.Id);
            ViewData["TotalColaboradores"] = await _context.Colaboradores.CountAsync(c => c.UnidadeMedicaId == unidade.Id);

            return View();
        }
    }
}