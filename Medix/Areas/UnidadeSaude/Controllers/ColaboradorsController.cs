using Medix.Data;
using Medix.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims; // Para pegar o ID do usuário logado

namespace Medix.Areas.UnidadeSaude.Controllers
{
    // GARANTE QUE SÓ UTILIZADORES COM O PAPEL "UnidadeSaude" PODEM ACESSAR
    [Authorize(Roles = "UnidadeSaude")]
    public class ColaboradoresController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ColaboradoresController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Método auxiliar para pegar o ID da Unidade Médica do usuário logado
        private async Task<int?> GetUserUnidadeMedicaIdAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return null;
            }

            var unidade = await _context.UnidadesMedicas
                                        .AsNoTracking() // Não precisamos rastrear a unidade, só do ID
                                        .FirstOrDefaultAsync(u => u.AdministradorUserId == userId);

            return unidade?.Id;
        }

        // GET: Colaboradores
        public async Task<IActionResult> Index()
        {
            var unidadeId = await GetUserUnidadeMedicaIdAsync();
            if (unidadeId == null) return Unauthorized("Usuário não está vinculado a uma unidade médica.");

            // FILTRA OS COLABORADORES PARA MOSTRAR APENAS OS DAQUELA UNIDADE
            var colaboradores = await _context.Colaboradores
                                          .Where(c => c.UnidadeMedicaId == unidadeId.Value)
                                          .ToListAsync();

            return View(colaboradores);
        }

        // GET: Colaboradores/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            var unidadeId = await GetUserUnidadeMedicaIdAsync();
            if (id == null || unidadeId == null) return NotFound();

            // FILTRA PELA UNIDADE DO USUÁRIO
            var colaborador = await _context.Colaboradores
                .FirstOrDefaultAsync(m => m.Id == id && m.UnidadeMedicaId == unidadeId.Value);

            if (colaborador == null) return NotFound();

            return View(colaborador);
        }

        // GET: Colaboradores/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Colaboradores/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("NomeCompleto,Email,Cargo,Especialidade,RegistroProfissional")] Colaborador colaborador)
        {
            var unidadeId = await GetUserUnidadeMedicaIdAsync();
            if (unidadeId == null) return Unauthorized();

            // VINCULA O NOVO COLABORADOR À UNIDADE DO USUÁRIO LOGADO
            colaborador.UnidadeMedicaId = unidadeId.Value;

            ModelState.Remove("UnidadeMedicaId");
            ModelState.Remove("UnidadeMedica");

            if (ModelState.IsValid)
            {
                _context.Add(colaborador);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(colaborador);
        }

        // GET: Colaboradores/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            var unidadeId = await GetUserUnidadeMedicaIdAsync();
            if (id == null || unidadeId == null) return NotFound();

            // FILTRA PELA UNIDADE DO USUÁRIO
            var colaborador = await _context.Colaboradores
                .FirstOrDefaultAsync(c => c.Id == id && c.UnidadeMedicaId == unidadeId.Value);

            if (colaborador == null) return NotFound();

            return View(colaborador);
        }

        // POST: Colaboradores/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,NomeCompleto,Email,Cargo,Especialidade,RegistroProfissional")] Colaborador colaborador)
        {
            var unidadeId = await GetUserUnidadeMedicaIdAsync();
            if (id != colaborador.Id || unidadeId == null) return NotFound();

            // VINCULA O COLABORADOR À UNIDADE DO USUÁRIO
            colaborador.UnidadeMedicaId = unidadeId.Value;
            ModelState.Remove("UnidadeMedica");

            if (ModelState.IsValid)
            {
                try
                {
                    // VERIFICA NOVAMENTE SE O USUÁRIO TEM PERMISSÃO para editar
                    var hasPermission = await _context.Colaboradores
                        .AnyAsync(c => c.Id == id && c.UnidadeMedicaId == unidadeId.Value);

                    if (!hasPermission) return NotFound();

                    _context.Update(colaborador);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Colaboradores.Any(e => e.Id == colaborador.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(colaborador);
        }

        // GET: Colaboradores/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            var unidadeId = await GetUserUnidadeMedicaIdAsync();
            if (id == null || unidadeId == null) return NotFound();

            // FILTRA PELA UNIDADE DO USUÁRIO
            var colaborador = await _context.Colaboradores
                .FirstOrDefaultAsync(m => m.Id == id && m.UnidadeMedicaId == unidadeId.Value);

            if (colaborador == null) return NotFound();

            return View(colaborador);
        }

        // POST: Colaboradores/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var unidadeId = await GetUserUnidadeMedicaIdAsync();
            if (unidadeId == null) return Unauthorized();

            // FILTRA PELA UNIDADE DO USUÁRIO
            var colaborador = await _context.Colaboradores
                .FirstOrDefaultAsync(c => c.Id == id && c.UnidadeMedicaId == unidadeId.Value);

            if (colaborador != null)
            {
                _context.Colaboradores.Remove(colaborador);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}