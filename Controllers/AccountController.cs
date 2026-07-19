using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
      private readonly IRedisService _redisService;

      public AccountController(
         VendingMachineContext context,
         IRedisService redisService)
      {
         _context = context;
         _redisService = redisService;
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

         if (!_redisService.IsAvailable)
         {
            return Json(new { success = false, message = "Fitur verifikasi email sedang tidak tersedia. Silakan coba lagi nanti." });
         }

          // Generate 6 digit random code
          var random = new Random();
          var verificationCode = random.Next(100000, 999999).ToString();

          // Simpan ke Redis Cache, expiry 1 menit
          await _redisService.SetCacheAsync($"VerificationCode_{model.Email}", verificationCode, TimeSpan.FromMinutes(1));

          // Kirim Email via Redis Message Queue
          var htmlMessage = MessageBuilder.BuildVerificationEmailBody(model.Name, verificationCode);

          await _redisService.PublishEmailAsync(model.Email, "Kode Verifikasi Vending App", htmlMessage);

          return Json(new { success = true, message = "Kode verifikasi sedang dikirim." });
      }

       [HttpPost]
       [ValidateAntiForgeryToken]
       public async Task<IActionResult> Register(RegisterViewModel model)
       {
          if (ModelState.IsValid)
          {
             // Validasi Kode Verifikasi dari Redis Cache
             var storedCode = await _redisService.GetCacheAsync($"VerificationCode_{model.Email}");

             if (storedCode != null)
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

             // Hapus kode dari Redis cache setelah sukses
             await _redisService.RemoveCacheAsync($"VerificationCode_{model.Email}");

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
      public async Task<IActionResult> SendLoginOtp([FromBody] Dictionary<string, string> data)
      {
         var email = data.GetValueOrDefault("email", "");
         if (string.IsNullOrEmpty(email))
            return Json(new { success = false, message = "Email tidak boleh kosong." });

         var user = await _context.UserLogins
            .Include(u => u.UserBalance)
            .FirstOrDefaultAsync(u => u.UserName == email && u.Status == "A");

         if (user == null)
            return Json(new { success = false, message = "Email tidak terdaftar." });

         if (!_redisService.IsAvailable)
         {
            return Json(new { success = false, message = "Fitur OTP login sedang tidak tersedia. Silakan gunakan password untuk login." });
         }

          var random = new Random();
          var otpCode = random.Next(100000, 999999).ToString();

          await _redisService.SetCacheAsync($"LoginOtp_{email}", otpCode, TimeSpan.FromMinutes(1));

          var name = user.UserBalance?.Name ?? user.UserName;
          var htmlMessage = MessageBuilder.BuildLoginOtpEmailBody(name, otpCode);

          await _redisService.PublishEmailAsync(email, "Kode OTP Login Vending App", htmlMessage);

          return Json(new { success = true, message = "Kode OTP sedang dikirim ke email Anda." });
      }

      [HttpPost]
      public async Task<IActionResult> Login(LoginViewModel model)
      {
         if (ModelState.IsValid)
         {
            var user = await _context.UserLogins
               .Include(u => u.UserBalance)
               .Include(u => u.UserRoles)
               .FirstOrDefaultAsync(u => u.UserName == model.Email && u.Status == "A");

            if (user == null)
            {
               ModelState.AddModelError("", "Email tidak terdaftar.");
               return View(model);
            }

            bool isValid = false;

            if (model.LoginMode == "otp")
            {
               var storedOtp = await _redisService.GetCacheAsync($"LoginOtp_{model.Email}");

               if (storedOtp != null)
               {
                  if (storedOtp == model.VerificationCode)
                  {
                     isValid = true;
                     await _redisService.RemoveCacheAsync($"LoginOtp_{model.Email}");
                  }
                  else
                  {
                     ModelState.AddModelError("VerificationCode", "Kode OTP yang dimasukkan salah.");
                  }
               }
               else
               {
                  ModelState.AddModelError("VerificationCode", "Kode OTP telah kadaluarsa. Silakan minta kode baru.");
               }
            }
            else
            {
               if (!string.IsNullOrEmpty(model.Password) && BC.Verify(model.Password, user.Password))
               {
                  isValid = true;
               }
               else
               {
                  ModelState.AddModelError("", "Email atau Password salah.");
               }
            }

            if (isValid)
            {
               var claims = new List<Claim>
               {
                  new Claim(ClaimTypes.Name, user.UserBalance?.Name ?? user.UserName),
                  new Claim(ClaimTypes.Email, user.UserName),
                  new Claim("UserId", user.IdUser.ToString())
               };

               if (user.UserRoles != null)
               {
                  foreach (var role in user.UserRoles.Where(r => r.Status == "A"))
                  {
                     claims.Add(new Claim(ClaimTypes.Role, role.RoleName));
                  }
               }

               var identity = new ClaimsIdentity(claims, "CookieAuth");
               var principal = new ClaimsPrincipal(identity);

               await HttpContext.SignInAsync("CookieAuth", principal);

               TempData["WelcomeMessage"] =
                  $"Selamat Datang {user.UserBalance?.Name ?? user.UserName}, Selamat berbelanja";

               return RedirectToAction("Index", "Home");
            }
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
