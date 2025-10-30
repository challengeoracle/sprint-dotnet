using Medix.Data;
using Medix.Models;
using Medix.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity; // Precisamos disto
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Medix.Controllers
{
    [Authorize] // Continua a exigir login para a página principal
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager; // Adicionado

        // Injeta o UserManager para podermos verificar os papéis
        public HomeController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager; // Adicionado
        }

        // Ação Index agora é assíncrona para verificar o utilizador
        public async Task<IActionResult> Index()
        {
            // Pega o utilizador que está logado
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                // Se não encontrar o utilizador, desloga-o por segurança
                return Challenge();
            }

            // --- LÓGICA DE REDIRECIONAMENTO POR PAPEL ---

            if (await _userManager.IsInRoleAsync(user, "EquipeMedix"))
            {
                // Se for EquipeMedix, carrega o Dashboard da Equipe (lógica antiga)
                var viewModel = new DashboardViewModel
                {
                    UserName = user.UserName,
                    TotalUnidades = await _context.UnidadesMedicas.CountAsync(),
                    UnidadesAtivas = await _context.UnidadesMedicas.CountAsync(u => u.Status == StatusUnidade.Ativa),
                    UnidadesInativas = await _context.UnidadesMedicas.CountAsync(u => u.Status == StatusUnidade.Inativa),
                    UnidadesSuspensas = await _context.UnidadesMedicas.CountAsync(u => u.Status == StatusUnidade.Suspensa),
                    AtividadeRecente = await _context.UnidadesMedicas
                                            .OrderByDescending(u => u.DataCadastro)
                                            .Take(5)
                                            .ToListAsync(),
                    StatusDistribution = new Dictionary<string, int>
                    {
                        { "Ativas", await _context.UnidadesMedicas.CountAsync(u => u.Status == StatusUnidade.Ativa) },
                        { "Inativas", await _context.UnidadesMedicas.CountAsync(u => u.Status == StatusUnidade.Inativa) },
                        { "Suspensas", await _context.UnidadesMedicas.CountAsync(u => u.Status == StatusUnidade.Suspensa) },
                        { "Em Teste", await _context.UnidadesMedicas.CountAsync(u => u.Status == StatusUnidade.EmTeste) }
                    }
                };
                return View(viewModel);
            }
            else if (await _userManager.IsInRoleAsync(user, "UnidadeSaude"))
            {
                // Se for UnidadeSaude, redireciona para o Dashboard da Área
                return RedirectToAction("Index", "Dashboard", new { area = "UnidadeSaude" });
            }

            // Se for um utilizador logado sem papel, apenas mostra uma página de acesso negado
            return Challenge();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}