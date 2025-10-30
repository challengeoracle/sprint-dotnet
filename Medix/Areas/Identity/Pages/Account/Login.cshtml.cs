// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace Medix.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ILogger<LoginModel> _logger;
        private readonly UserManager<IdentityUser> _userManager; // << NOVO: Adicionado o UserManager

        public LoginModel(
            SignInManager<IdentityUser> signInManager,
            ILogger<LoginModel> logger,
            UserManager<IdentityUser> userManager) // << NOVO: Injetado no construtor
        {
            _signInManager = signInManager;
            _logger = logger;
            _userManager = userManager; // << NOVO: Atribuído
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "O campo E-mail é obrigatório.")]
            [EmailAddress(ErrorMessage = "O formato do e-mail não é válido.")]
            public string Email { get; set; }

            [Required(ErrorMessage = "O campo Senha é obrigatório.")]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Display(Name = "Lembrar de mim")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/");

            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    _logger.LogInformation("Usuário logado.");

                    // --- INÍCIO DA LÓGICA DE REDIRECIONAMENTO ---

                    // 1. Encontra o usuário que acabou de fazer login
                    var user = await _userManager.FindByEmailAsync(Input.Email);
                    if (user == null)
                    {
                        // Isso não deve acontecer, mas por segurança
                        return LocalRedirect(returnUrl);
                    }

                    // 2. Pega os papéis (Roles) desse usuário
                    var roles = await _userManager.GetRolesAsync(user);

                    // 3. Verifica os papéis e redireciona
                    if (roles.Contains("EquipeMedix"))
                    {
                        // Se for da equipe, vai para o Dashboard principal
                        _logger.LogInformation("Usuário é EquipeMedix. Redirecionando para /Home/Index.");
                        return RedirectToAction("Index", "Home", new { area = "" });
                    }
                    else if (roles.Contains("UnidadeSaude"))
                    {
                        // Se for da unidade, vai para o Dashboard da Área UnidadeSaude
                        _logger.LogInformation("Usuário é UnidadeSaude. Redirecionando para /UnidadeSaude/Dashboard/Index.");
                        return RedirectToAction("Index", "Dashboard", new { area = "UnidadeSaude" });
                    }
                    else
                    {
                        // Se não tiver papel (ou outro papel), vai para a home padrão
                        _logger.LogWarning($"Usuário {user.UserName} logou mas não tem um papel de redirecionamento conhecido.");
                        return LocalRedirect(returnUrl);
                    }

                    // --- FIM DA LÓGICA DE REDIRECIONAMENTO ---
                }
                if (result.RequiresTwoFactor)
                {
                    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
                }
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("Conta de usuário bloqueada.");
                    return RedirectToPage("./Lockout");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Tentativa de login inválida.");
                    return Page();
                }
            }

            return Page();
        }
    }
}