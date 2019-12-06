using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using MangerInstructions.Models;
using MangerInstructions.ViewModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace MangerInstructions.Controllers
{
    public class AccountController : Controller
    {
        private MangerInstructionsDbContext mangerInstructionsDbContext;
        private readonly IAuthenticationSchemeProvider authenticationSchemeProvider;
        private readonly IStringLocalizer<SharedResource> sharedLocalizer;

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var userId = User.Claims.FirstOrDefault(t => t.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userId != null)
            {
                var user = mangerInstructionsDbContext.Users.FirstOrDefault(u => u.Id == userId);
                if (user == null)
                    HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme).GetAwaiter();
                else if (user.IsBlock)
                {
                    HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme).GetAwaiter();
                    context.Result = RedirectToAction("Block");
                }
            }
        }

        public AccountController(MangerInstructionsDbContext context, IStringLocalizer<SharedResource> sharedLocalizer,
            IAuthenticationSchemeProvider authenticationSchemeProvider)
        {
            mangerInstructionsDbContext = context;
            this.sharedLocalizer = sharedLocalizer;
            this.authenticationSchemeProvider = authenticationSchemeProvider;
        }

        [HttpPost]
        public async Task<IActionResult> ChangeUserName(String name)
        {
            var userId = User.Claims.FirstOrDefault(t => t.Type == ClaimTypes.NameIdentifier).Value;
            var user = mangerInstructionsDbContext.Users.FirstOrDefault(u => u.Id == userId);
            user.Name = name;
            await mangerInstructionsDbContext.SaveChangesAsync();
            return Ok();
        }

        [HttpGet]
        public IActionResult CreateAccount(String userName = null, String userEmail = null, String provider = null)
        {
            ViewBag.Provider = provider;
            if (provider == null)
            {
                return View();
            }
            else if (userName == null || userEmail == null)
                return RedirectToAction("Index", "Home");
            else
                return View(new RegisterUser { Name = userName, Email = userEmail });
        }

        [HttpPost]
        public async Task<IActionResult> CreateAccount(RegisterUser registerUser)
        {
            if (ModelState.IsValid)
            {
                User userMail = await mangerInstructionsDbContext.Users.FirstOrDefaultAsync(u => u.Email == registerUser.Email);
                if (userMail != null)
                    ModelState.AddModelError("", sharedLocalizer["ExistMail"]);
                else
                {
                    User user = new User
                    {
                        Name = registerUser.Name,
                        Email = registerUser.Email,
                        Password = registerUser.Password,
                        PersonalPage = new PersonalPage(),
                        Role = Role.User
                    };
                    mangerInstructionsDbContext.Users.Add(user);
                    await mangerInstructionsDbContext.SaveChangesAsync();
                    await AuthenticateAsync(user);
                    return RedirectToAction("Index", "Home");
                }
            }
            return View();
        }

        [HttpGet]
        public IActionResult SignInSocial(String provider)
        {
            return Challenge(new AuthenticationProperties { RedirectUri = Url.Action("FindAccount", "Account", new { provider = provider }) }, provider);
        }

        [HttpGet]
        public async Task<IActionResult> FindAccount(String provider)
        {
            String userName = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
            String userEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            User user = await mangerInstructionsDbContext.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            if (user == null)
                return RedirectToAction("CreateAccount", "Account", new { userName = userName, userEmail = userEmail, provider = provider });
            else
            {
                if (user.IsBlock)
                    return RedirectToAction("Block");
                await AuthenticateAsync(user);
                return RedirectToAction("Index", "Home");
            }
        }

        public IActionResult Block()
        {
            return View();
        }

        private async Task<IEnumerable<String>> GetProvidersAsync()
        {
            return (await authenticationSchemeProvider.GetAllSchemesAsync()).Select(n => n.DisplayName).Where(n => !String.IsNullOrEmpty(n));
        }

        [HttpGet]
        public async Task<IActionResult> SignIn()
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");
            ViewBag.Scheme = await GetProvidersAsync();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SignIn(LoginUser loginUser)
        {
            if (ModelState.IsValid)
            {
                User user = await mangerInstructionsDbContext.Users.FirstOrDefaultAsync(u => u.Email == loginUser.Email && u.Password == loginUser.Password);
                if (user != null)
                {
                    if (user.IsBlock)
                        return RedirectToAction("Block");
                    await AuthenticateAsync(user);
                    return RedirectToAction("Index", "Home");
                }
            }
            ModelState.AddModelError("", sharedLocalizer["WrongLoginAndPassword"]);
            ViewBag.Scheme = await GetProvidersAsync();
            return View();
        }

        [Authorize]
        public async Task<IActionResult> SignOut()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("SignIn", "Account");
        }

        private async Task AuthenticateAsync(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim(ClaimTypes.Email, user.Email)
            };
            ClaimsIdentity id = new ClaimsIdentity(claims, "ApplicationCookie", ClaimTypes.Name, ClaimTypes.Role);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
        }
    }
}