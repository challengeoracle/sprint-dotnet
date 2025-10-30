using Medix.Data;
using Medix.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims; // Para pegar o ID do usuário logado

namespace Medix.Areas.UnidadeSaude.Controllers
{
    // GARANTE QUE SÓ UTILIZADORES COM O PAPEL "UnidadeSaude" PODEM ACESSAR
    [Authorize(Roles = "UnidadeSaude")]
    [Area("UnidadeSaude")]
    public class PacientesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public PacientesController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
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
                return null; // Não deve acontecer se [Authorize] estiver ativo
            }

            var unidade = await _context.UnidadesMedicas
                          .FirstOrDefaultAsync(u => u.AdministradorUserId == userId);

            return unidade?.Id;
        }

        // GET: Pacientes
        public async Task<IActionResult> Index()
        {
            var unidadeId = await GetUserUnidadeMedicaIdAsync();
            if (unidadeId == null)
            {
                return Unauthorized("Usuário não está vinculado a uma unidade médica.");
            }

            // FILTRA OS PACIENTES PARA MOSTRAR APENAS OS DAQUELA UNIDADE
            var pacientes = await _context.Pacientes
                     .Where(p => p.UnidadeMedicaId == unidadeId.Value)
                     .ToListAsync();

            return View(pacientes);
        }

        // GET: Pacientes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            var unidadeId = await GetUserUnidadeMedicaIdAsync();
            if (id == null || unidadeId == null) return NotFound();

            // FILTRA O PACIENTE PELA UNIDADE DO USUÁRIO
            var paciente = await _context.Pacientes
        .FirstOrDefaultAsync(m => m.Id == id && m.UnidadeMedicaId == unidadeId.Value);

            if (paciente == null) return NotFound();

            return View(paciente);
        }

        // GET: Pacientes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Pacientes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("NomeCompleto,CPF,DataNascimento,Email,Telefone,Endereco")] Paciente paciente)
        {
            var unidadeId = await GetUserUnidadeMedicaIdAsync();
            if (unidadeId == null) return Unauthorized();

            // VINCULA O NOVO PACIENTE À UNIDADE DO USUÁRIO LOGADO
            paciente.UnidadeMedicaId = unidadeId.Value;

            // Remove o Id e UnidadeMedicaId do ModelState para evitar validação desnecessária
            ModelState.Remove("UnidadeMedicaId");
            ModelState.Remove("UnidadeMedica");

            if (ModelState.IsValid)
            {
                _context.Add(paciente);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(paciente);
        }

        // GET: Pacientes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            var unidadeId = await GetUserUnidadeMedicaIdAsync();
            if (id == null || unidadeId == null) return NotFound();

            // FILTRA O PACIENTE PELA UNIDADE DO USUÁRIO
            var paciente = await _context.Pacientes
        .FirstOrDefaultAsync(p => p.Id == id && p.UnidadeMedicaId == unidadeId.Value);

            if (paciente == null) return NotFound();

            return View(paciente);
        }

        // POST: Pacientes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,NomeCompleto,CPF,DataNascimento,Email,Telefone,Endereco")] Paciente paciente)
        {
            var unidadeId = await GetUserUnidadeMedicaIdAsync();
            if (id != paciente.Id || unidadeId == null) return NotFound();

            // VINCULA O PACIENTE À UNIDADE DO USUÁRIO
            paciente.UnidadeMedicaId = unidadeId.Value;
            ModelState.Remove("UnidadeMedica");

            if (ModelState.IsValid)
            {
                try
                {
                    // VERIFICA NOVAMENTE SE O USUÁRIO TEM PERMISSÃO para editar este paciente!!!!
                    var hasPermission = await _context.Pacientes
            .AnyAsync(p => p.Id == id && p.UnidadeMedicaId == unidadeId.Value);

                    if (!hasPermission) return NotFound();

                    _context.Update(paciente);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Pacientes.Any(e => e.Id == paciente.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(paciente);
        }

        // GET: Pacientes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            var unidadeId = await GetUserUnidadeMedicaIdAsync();
            if (id == null || unidadeId == null) return NotFound();

            // FILTRA O PACIENTE PELA UNIDADE DO USUÁRIO
            var paciente = await _context.Pacientes
        .FirstOrDefaultAsync(m => m.Id == id && m.UnidadeMedicaId == unidadeId.Value);

            if (paciente == null) return NotFound();

            return View(paciente);
        }

        // POST: Pacientes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var unidadeId = await GetUserUnidadeMedicaIdAsync();
            if (unidadeId == null) return Unauthorized();

            // FILTRA O PACIENTE PELA UNIDADE DO USUÁRIO
            var paciente = await _context.Pacientes
        .FirstOrDefaultAsync(p => p.Id == id && p.UnidadeMedicaId == unidadeId.Value);

            if (paciente != null)
            {
                _context.Pacientes.Remove(paciente);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}