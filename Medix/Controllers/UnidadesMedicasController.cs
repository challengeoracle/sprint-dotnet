using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Medix.Data;
using Medix.Models;

namespace Medix.Controllers
{
    [Authorize]
    [Route("unidades")] // Prefixo para todas as rotas nesse controller
    public class UnidadesMedicasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UnidadesMedicasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: UnidadesMedicas
        [HttpGet("")] // Rota para a listagem (Index)
        public async Task<IActionResult> Index()
        {
            return View(await _context.UnidadesMedicas.ToListAsync());
        }

        // GET: UnidadesMedicas/Details/5
        [HttpGet("detalhes/{id:int}")] // Rota para Detalhes
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var unidadeMedica = await _context.UnidadesMedicas
                .FirstOrDefaultAsync(m => m.Id == id);
            if (unidadeMedica == null)
            {
                return NotFound();
            }

            return View(unidadeMedica);
        }

        // GET: UnidadesMedicas/Create
        [HttpGet("nova")] // Rota para exibir o formulário de criação
        public IActionResult Create()
        {
            return View();
        }

        // POST: UnidadesMedicas/Create
        [HttpPost("nova")] // Rota para receber os dados do formulário de criação
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Nome,CNPJ,Endereco,Telefone,EmailAdmin,Status")] UnidadeMedica unidadeMedica)
        {
            if (ModelState.IsValid)
            {
                // A data de cadastro é definida automaticamente pelo servidor
                unidadeMedica.DataCadastro = DateTime.Now;

                _context.Add(unidadeMedica);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(unidadeMedica);
        }

        // GET: UnidadesMedicas/Edit/5
        [HttpGet("editar/{id:int}")] // Rota para exibir o formulário de edição
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var unidadeMedica = await _context.UnidadesMedicas.FindAsync(id);
            if (unidadeMedica == null)
            {
                return NotFound();
            }

            return View(unidadeMedica);
        }

        // POST: UnidadesMedicas/Edit/5
        [HttpPost("editar/{id:int}")] // Rota para receber os dados do formulário de edição
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nome,CNPJ,Endereco,Telefone,EmailAdmin,Status,DataCadastro")] UnidadeMedica unidadeMedica)
        {
            if (id != unidadeMedica.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(unidadeMedica);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UnidadeMedicaExists(unidadeMedica.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(unidadeMedica);
        }

        // GET: UnidadesMedicas/Delete/5
        [HttpGet("excluir/{id:int}")] // Rota para exibir a confirmação de exclusão
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var unidadeMedica = await _context.UnidadesMedicas
                .FirstOrDefaultAsync(m => m.Id == id);
            if (unidadeMedica == null)
            {
                return NotFound();
            }

            return View(unidadeMedica);
        }

        // POST: UnidadesMedicas/Delete/5
        [HttpPost("excluir/{id:int}")] // Rota para confirmar a exclusão
        [ValidateAntiForgeryToken]
        // Mantido o ActionName("Delete") caso haja algum uso específico, mas a rota define a URL
        [ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var unidadeMedica = await _context.UnidadesMedicas.FindAsync(id);
            if (unidadeMedica != null)
            {
                _context.UnidadesMedicas.Remove(unidadeMedica);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UnidadeMedicaExists(int id)
        {
            return _context.UnidadesMedicas.Any(e => e.Id == id);
        }
    }
}