using Medix.Data;
using Medix.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Medix.Areas.UnidadeSaude.Controllers
{
    [Area("UnidadeSaude")]
    [Authorize(Roles = "UnidadeSaude")]
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
            int unidadeId = unidade.Id; // ID da unidade

            // Busca estatísticas SÓ DESTA UNIDADE
            ViewData["TotalPacientes"] = await _context.Pacientes.CountAsync(p => p.UnidadeMedicaId == unidadeId);
            ViewData["TotalColaboradores"] = await _context.Colaboradores.CountAsync(c => c.UnidadeMedicaId == unidadeId);

            // 1. Busca a atividade recente (últimos 5 pacientes cadastrados)
            ViewData["PacientesRecentes"] = await _context.Pacientes
                                                        .Where(p => p.UnidadeMedicaId == unidadeId)
                                                        .OrderByDescending(p => p.Id)
                                                        .Take(5)
                                                        .ToListAsync();

            // 2. Busca a atividade recente de colaboradores (últimos 5)
            ViewData["ColaboradoresRecentes"] = await _context.Colaboradores
                                                            .Where(c => c.UnidadeMedicaId == unidadeId)
                                                            .OrderByDescending(c => c.Id)
                                                            .Take(5)
                                                            .ToListAsync();

            // 3. Simulação dos dados do gráfico:
            int totalPacientes = (int)ViewData["TotalPacientes"];
            if (totalPacientes == 0) totalPacientes = 1;

            ViewData["PacientesCompletos"] = (int)Math.Round(totalPacientes * 0.7);
            ViewData["PacientesIncompletos"] = totalPacientes - (int)ViewData["PacientesCompletos"];

            return View();
        }
    }
}