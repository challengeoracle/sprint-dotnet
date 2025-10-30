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
using Microsoft.AspNetCore.Identity;

namespace Medix.Controllers
{
    [Authorize] // Só a equipe logada pode mexer aqui
    [Route("unidades")] // Prefixo para todas as rotas nesse controller
    public class UnidadesMedicasController : Controller
    {
        private readonly ApplicationDbContext _context;
        // Adicionei o UserManager e o RoleManager
        private readonly UserManager<IdentityUser> _userManager;

        public UnidadesMedicasController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager; // Injeção de dependência
        }

        // GET: UnidadesMedicas
        [HttpGet("")] // Rota para a listagem (Index)
        // Ação do Index com busca, filtro, ordenação e paginação
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
                default: // Padrão é ordenar por nome ASC
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
        public async Task<IActionResult> Create(CreateUnidadeViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                // 1. Criar o usuário de acesso para a unidade
                var user = new IdentityUser
                {
                    UserName = viewModel.EmailAcesso,
                    Email = viewModel.EmailAcesso,
                    EmailConfirmed = true // Já crio como confirmado
                };

                var result = await _userManager.CreateAsync(user, viewModel.SenhaAcesso);

                if (result.Succeeded)
                {
                    // 2. Se o usuário foi criado, adicionar ele ao Papel "UnidadeSaude"
                    await _userManager.AddToRoleAsync(user, "UnidadeSaude");

                    // 3. Mapear ViewModel para Model
                    var unidadeMedica = new UnidadeMedica
                    {
                        Nome = viewModel.Nome,
                        CNPJ = viewModel.CNPJ,
                        Endereco = viewModel.Endereco,
                        Telefone = viewModel.Telefone,
                        EmailAdmin = viewModel.EmailAdmin,
                        Status = viewModel.Status,
                        DataCadastro = DateTime.Now,
                        AdministradorUserId = user.Id // IMPORTANTE: Vincula o ID do usuário criado
                    };

                    _context.Add(unidadeMedica);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    // Se a criação do usuário falhou (ex: email já existe),
                    // adiciona os erros ao ModelState e retorna pra view.
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("EmailAcesso", error.Description);
                    }
                }
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
                    // DataCadastro e AdministradorUserId não mudam na edição simples

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
        [ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var unidadeMedica = await _context.UnidadesMedicas.FindAsync(id);
            if (unidadeMedica != null)
            {
                _context.UnidadesMedicas.Remove(unidadeMedica);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool UnidadeMedicaExists(int id)
        {
            return _context.UnidadesMedicas.Any(e => e.Id == id);
        }
    }
}