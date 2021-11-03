using System.Globalization;
using System.Linq;
using System.Security;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AspNetCoreQueryBuilderApp.Data;
using AspNetCoreQueryBuilderApp.Models;
using System.Collections.Generic;

namespace AspNetCoreQueryBuilderApp.Controllers {
    public class AccountController : Controller {

        [HttpGet]
        public async Task<IActionResult> Login([FromServices]ApplicationDbContext dbContext) {
            return View(await GetLoginScreenModelAsync(dbContext));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login([FromServices]ApplicationDbContext dbContext, int userId, string returnUrl) {
            var user = await dbContext.Users.FindAsync(userId);
            if(user != null) {
                await SignIn(user);

                if(Url.IsLocalUrl(returnUrl)) {
                    return Redirect(returnUrl);
                }
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }

            throw new SecurityException($"User not found by the ID: {userId}");
        }

        [HttpPost]
        public async Task<IActionResult> Logout() {
            await HttpContext.SignOutAsync();
            return RedirectToAction(nameof(Login));
        }

        async Task SignIn(ApplicationUser user) {
            string userName = $"{user.FirstMidName} {user.LastName}";

            var claims = new List<Claim> {
                new Claim(ClaimTypes.Name, userName),
                new Claim(ClaimTypes.NameIdentifier, userName),
                new Claim(ClaimTypes.Role, "User"),
                new Claim(ClaimTypes.Sid, user.ID.ToString(CultureInfo.InvariantCulture))
            };

            var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
            identity.AddClaims(claims);

            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(principal, new AuthenticationProperties { IsPersistent = true });
        }

        async Task<LoginScreenModel> GetLoginScreenModelAsync(ApplicationDbContext dBContext) {
            var model = new LoginScreenModel();
            model.Users = await dBContext.Users
                .Select(x => new SelectListItem {
                    Value = x.ID.ToString(CultureInfo.InvariantCulture),
                    Text = $"{x.FirstMidName} {x.LastName}"
                })
                .ToListAsync();
            return model;
        }

    }
}
