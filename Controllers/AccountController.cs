using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using VendingMachineApp.Models;
using VendingMachineApp.Models.ViewModels;
using BC = BCrypt.Net.BCrypt;

namespace VendingMachineApp.Controllers
{
   public class AccountController : Controller
   {
      private readonly VendingMachineContext _context;

      public AccountController(VendingMachineContext context)
      {
         _context = context;
      }

      [HttpGet]
      public IActionResult Register()
      {
         return View();
      }

      [HttpPost]
      public async Task<IActionResult> Register(RegisterViewModel model)
      {
         if (ModelState.IsValid)
         {
            var existingUser = await _context.UserLogins.FirstOrDefaultAsync(u => u.UserName == model.Email);
            if (existingUser != null)
            {
               ModelState.AddModelError("Email", "Email sudah terdaftar.");
               return View(model);
            }

            var user = new UserLogin
            {
               UserName = model.Email,
               Password = BC.HashPassword(model.Password)
            };

            _context.UserLogins.Add(user);

            // Create associated balance record
            var balance = new UserBalance
            {
               User = user,
               Name = model.Name,
               Balance = 0
            };
            _context.UserBalances.Add(balance);

            await _context.SaveChangesAsync();

            return RedirectToAction("Login");
         }

         return View(model);
      }

      [HttpGet]
      public IActionResult Login()
      {
         return View();
      }

      [HttpPost]
      public async Task<IActionResult> Login(LoginViewModel model)
      {
         if (ModelState.IsValid)
         {
            var user = await _context.UserLogins.Include(u => u.UserBalance)
               .FirstOrDefaultAsync(u => u.UserName == model.Email);
            if (user != null && BC.Verify(model.Password, user.Password))
            {
               var claims = new List<Claim>
               {
                  new Claim(ClaimTypes.Name, user.UserBalance?.Name ?? user.UserName),
                  new Claim(ClaimTypes.Email, user.UserName),
                  new Claim("UserId", user.IdUser.ToString())
               };

               var identity = new ClaimsIdentity(claims, "CookieAuth");
               var principal = new ClaimsPrincipal(identity);

               await HttpContext.SignInAsync("CookieAuth", principal);

               return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Email atau Password salah.");
         }

         return View(model);
      }

      public async Task<IActionResult> Logout()
      {
         await HttpContext.SignOutAsync("CookieAuth");
         return RedirectToAction("Index", "Home");
      }
   }
}
