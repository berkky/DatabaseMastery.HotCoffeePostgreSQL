using System.Security.Claims;
using DatabaseMastery.HotCoffeePostgreSQL.Authentication;
using DatabaseMastery.HotCoffeePostgreSQL.Models.ViewModels;
using DatabaseMastery.HotCoffeePostgreSQL.Services.AdminAuth;
using DatabaseMastery.HotCoffeePostgreSQL.Infrastructure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace DatabaseMastery.HotCoffeePostgreSQL.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAdminCredentialValidator _credentialValidator;

        public AccountController(IAdminCredentialValidator credentialValidator)
        {
            _credentialValidator = credentialValidator;
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login(string? returnUrl)
        {
            if (User.Identity?.IsAuthenticated == true && User.IsInRole(AdminRoles.Admin))
            {
                return RedirectToAction("Index", "Dashboard");
            }

            return View(new AdminLoginViewModel { ReturnUrl = returnUrl });
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [EnableRateLimiting(RateLimitPolicies.AdminLoginPost)]
        public async Task<IActionResult> Login(AdminLoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (!_credentialValidator.Validate(model.Username, model.Password))
            {
                ModelState.AddModelError(string.Empty, "Geçersiz kullanıcı adı veya parola.");
                model.Password = string.Empty;
                return View(model);
            }

            var displayName = model.Username.Trim();
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, displayName),
                new(ClaimTypes.Name, displayName),
                new(ClaimTypes.Role, AdminRoles.Admin)
            };

            var identity = new ClaimsIdentity(claims, AuthSchemes.HotCoffeeAdmin);
            var principal = new ClaimsPrincipal(identity);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                AllowRefresh = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
            };

            await HttpContext.SignInAsync(AuthSchemes.HotCoffeeAdmin, principal, authProperties);

            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }

            return RedirectToAction("Index", "Dashboard");
        }

        [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(AuthSchemes.HotCoffeeAdmin);
            return RedirectToAction(nameof(Login));
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
