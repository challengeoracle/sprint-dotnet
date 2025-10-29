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
using Medix.ViewModels;

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
        // parâmetros pra busca, filtro, ordenação e paginação
        public async Task<IActionResult> Index(
            string? searchString,
            StatusUnidade? status,
            string sortOrder,
            int? pageNumber)
        {
            // Guarda os filtros e ordenação pra usar nos links da view
            ViewData["CurrentSort"] = sortOrder;
            ViewData["NomeSortParam"] = String.IsNullOrEmpty(sortOrder) ? "nome_desc" : "";
            ViewData["DataSortParam"] = sortOrder == "Data" ? "data_desc" : "Data";
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentStatus"] = status;

            // Começa a query no banco
            var query = _context.UnidadesMedicas.AsQueryable();

            // Filtro de busca por nome
            if (!String.IsNullOrEmpty(searchString))
            {
                query = query.Where(u => u.Nome.Contains(searchString));
            }

            // Filtro por status
            if (status.HasValue)
            {
                query = query.Where(u => u.Status == status.Value);
            }

            // Ordenação
            switch (sortOrder)
            {
                case "nome_desc":
                    query = query.OrderByDescending(u => u.Nome);
                    break;
                case "Data":
                    query = query.OrderBy(u => u.DataCadastro);
                    break;
                case "data_desc":
                    query = query.OrderByDescending(u => u.DataCadastro);
                    break;
                default:
                    query = query.OrderBy(u => u.Nome);
                    break;
            }

            // Paginação
            int pageSize = 10;
            var count = await query.CountAsync();
            var items = await query.Skip(((pageNumber ?? 1) - 1) * pageSize).Take(pageSize).ToListAsync();

            // Passa os dados da paginação pra View
            ViewData["PageNumber"] = pageNumber ?? 1;
            ViewData["TotalPages"] = (int)Math.Ceiling(count / (double)pageSize);
            ViewData["HasPreviousPage"] = (pageNumber ?? 1) > 1;
            ViewData["HasNextPage"] = (pageNumber ?? 1) < (int)Math.Ceiling(count / (double)pageSize);

            return View(items); // Envia só os itens da página atual pra View
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
            // Agora retorna o ViewModel vazio
            return View(new CreateUnidadeViewModel());
        }

        // POST: UnidadesMedicas/Create
        [HttpPost("nova")] // Rota para receber os dados do formulário de criação
        [ValidateAntiForgeryToken]
        // Agora recebe o ViewModel, ajustei o Bind
        public async Task<IActionResult> Create([Bind("Nome,CNPJ,Endereco,Telefone,EmailAdmin,Status")] CreateUnidadeViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                // Mapear ViewModel para Model
                var unidadeMedica = new UnidadeMedica
                {
                    Nome = viewModel.Nome,
                    CNPJ = viewModel.CNPJ,
                    Endereco = viewModel.Endereco,
                    Telefone = viewModel.Telefone,
                    EmailAdmin = viewModel.EmailAdmin,
                    Status = viewModel.Status,
                    // A data de cadastro é definida automaticamente pelo servidor
                    DataCadastro = DateTime.Now
                };

                _context.Add(unidadeMedica);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            // Retorna a view com o viewModel preenchido se a validação falhar
            return View(viewModel);
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

            // Mapear Model para ViewModel
            var viewModel = new EditUnidadeViewModel
            {
                Id = unidadeMedica.Id,
                Nome = unidadeMedica.Nome,
                CNPJ = unidadeMedica.CNPJ,
                Endereco = unidadeMedica.Endereco,
                Telefone = unidadeMedica.Telefone,
                EmailAdmin = unidadeMedica.EmailAdmin,
                Status = unidadeMedica.Status,
                DataCadastro = unidadeMedica.DataCadastro // Mantido para exibição
            };

            return View(viewModel);
        }

        // POST: UnidadesMedicas/Edit/5
        [HttpPost("editar/{id:int}")] // Rota para receber os dados do formulário de edição
        [ValidateAntiForgeryToken]
        // Agora recebe o ViewModel, ajustei o Bind
        public async Task<IActionResult> Edit(int id, EditUnidadeViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Buscar a entidade original
                    var unidadeMedica = await _context.UnidadesMedicas.FindAsync(id);
                    if (unidadeMedica == null)
                    {
                        return NotFound();
                    }

                    // Atualizar a entidade com dados do ViewModel
                    unidadeMedica.Nome = viewModel.Nome;
                    unidadeMedica.CNPJ = viewModel.CNPJ;
                    unidadeMedica.Endereco = viewModel.Endereco;
                    unidadeMedica.Telefone = viewModel.Telefone;
                    unidadeMedica.EmailAdmin = viewModel.EmailAdmin;
                    unidadeMedica.Status = viewModel.Status;
                    // DataCadastro não muda na edição

                    _context.Update(unidadeMedica);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UnidadeMedicaExists(viewModel.Id))
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
            // Retorna a view com o viewModel preenchido se a validação falhar
            return View(viewModel);
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