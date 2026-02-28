using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;
using VendingMachineApp.Helpers;
using VendingMachineApp.Models;
using VendingMachineApp.Models.ViewModels;
using VendingMachineApp.Services;
using BC = BCrypt.Net.BCrypt;

namespace VendingMachineApp.Controllers
{
   public class AccountController : Controller
   {
      private readonly VendingMachineContext _context;
      private readonly IMemoryCache _cache;
      private readonly IEmailService _emailService;

      public AccountController(
         VendingMachineContext context,
         IMemoryCache cache,
         IEmailService emailService)
      {
         _context = context;
         _cache = cache;
         _emailService = emailService;
      }

      [HttpGet]
      public IActionResult Register()
      {
         return View();
      }

      [HttpPost]
      public async Task<IActionResult> SendVerificationCode([FromBody] RegisterViewModel model)
      {
         if (string.IsNullOrEmpty(model.Email))
         {
            return Json(new { success = false, message = "Email tidak boleh kosong." });
         }

         // Cek apakah email sudah terdaftar
         var existingUser = await _context.UserLogins.FirstOrDefaultAsync(u => u.UserName == model.Email);
         if (existingUser != null)
         {
            return Json(new { success = false, message = "Email sudah terdaftar." });
         }

         // Generate 6 digit random code
         var random = new Random();
         var verificationCode = random.Next(100000, 999999).ToString();

         // Simpan ke MemoryCache, expiry 1 menit (60 detik)
         var cacheOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(1));

         _cache.Set($"VerificationCode_{model.Email}", verificationCode, cacheOptions);

         // Kirim Email
         var htmlMessage = MessageBuilder.BuildVerificationEmailBody(model.Name, verificationCode);

         var isSent = await _emailService.SendEmailAsync(model.Email, "Kode Verifikasi Vending App", htmlMessage);

         if (isSent)
         {
            return Json(new { success = true, message = "Kode verifikasi berhasil dikirim." });
         }

         return Json(new
         {
            success = false, message = "Gagal mengirim email verifikasi. Pastikan SMTP Anda dikonfigurasi dengan benar."
         });
      }

      [HttpPost]
      [ValidateAntiForgeryToken]
      public async Task<IActionResult> Register(RegisterViewModel model)
      {
         if (ModelState.IsValid)
         {
            // Validasi Kode Verifikasi dari Cache
            if (_cache.TryGetValue($"VerificationCode_{model.Email}", out string? storedCode))
            {
               if (storedCode != model.VerificationCode)
               {
                  ModelState.AddModelError("VerificationCode", "Kode verifikasi yang dimasukkan salah.");
                  return View(model);
               }
            }
            else
            {
               ModelState.AddModelError("VerificationCode",
                  "Kode verifikasi telah kadaluarsa atau tidak berikan. Silakan klik tombol 'Register' untuk meminta kode.");
               return View(model);
            }

            // Validasi Email Ulang (untuk keamanan)
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

            // Hapus kode dari cache setelah sukses
            _cache.Remove($"VerificationCode_{model.Email}");

            TempData["SuccessMessage"] = "Pendaftaran berhasil! Silakan login.";
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

               TempData["WelcomeMessage"] =
                  $"Selamat Datang {user.UserBalance?.Name ?? user.UserName}, Selamat berbelanja";

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
