using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VendingMachineApp.Models;
using VendingMachineApp.Models.ViewModels;
using VendingMachineApp.Services;
using VendingMachineApp.Helpers;

namespace VendingMachineApp.Controllers
{
    [Authorize]
    public class SmtpController : Controller
    {
        private readonly VendingMachineContext _context;
        private readonly IEmailService _emailService;

        public SmtpController(VendingMachineContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task<IActionResult> Index()
        {
            var settings = await _context.SmtpEmails
                .Where(s => s.IsActive)
                .Select(s => new SmtpSettingViewModel
                {
                    IdSmtp = s.IdSmtp,
                    Host = s.Host,
                    Port = s.Port,
                    EnableSSL = s.EnableSSL,
                    SenderName = s.SenderName,
                    SenderEmail = s.SenderEmail,
                    Username = s.Username,
                    IsActive = s.IsActive
                })
                .ToListAsync();
            return View(settings);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TestEmail(int idSmtp, string toEmail)
        {
            if (string.IsNullOrEmpty(toEmail))
            {
                TempData["ErrorMessage"] = "Alamat email tujuan tidak boleh kosong.";
                return RedirectToAction(nameof(Index));
            }

            var isSuccess = await _emailService.SendEmailAsync(
                toEmail,
                "Test Email dari Vending App",
                MessageBuilder.BuildTestEmailBody()
            );

            if (isSuccess) TempData["SuccessMessage"] = $"Pesan percobaan berhasil dikirim ke {toEmail}.";
            else TempData["ErrorMessage"] = "Gagal mengirim pesan percobaan. Periksa console log untuk detail error.";

            return RedirectToAction(nameof(Index));
        }

        // --- Create Methods ---
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SmtpSettingViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.IsActive)
                {
                    var activeSettings = await _context.SmtpEmails.Where(s => s.IsActive).ToListAsync();
                    activeSettings.ForEach(s => s.IsActive = false);
                }

                var smtp = new SmptpEmail
                {
                    Host = model.Host,
                    Port = model.Port,
                    EnableSSL = model.EnableSSL,
                    SenderName = model.SenderName,
                    SenderEmail = model.SenderEmail,
                    Username = model.Username,
                    Password = model.Password,
                    IsActive = model.IsActive
                };
                _context.SmtpEmails.Add(smtp);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Konfigurasi SMTP berhasil ditambahkan.";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // --- Edit Methods ---
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var smtp = await _context.SmtpEmails.FindAsync(id);
            if (smtp == null) return NotFound();
            var model = new SmtpSettingViewModel
            {
                IdSmtp = smtp.IdSmtp,
                Host = smtp.Host,
                Port = smtp.Port,
                EnableSSL = smtp.EnableSSL,
                SenderName = smtp.SenderName,
                SenderEmail = smtp.SenderEmail,
                Username = smtp.Username,
                Password = smtp.Password,
                IsActive = smtp.IsActive
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SmtpSettingViewModel model)
        {
            if (id != model.IdSmtp) return NotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    var smtp = await _context.SmtpEmails.FindAsync(id);
                    if (smtp == null) return NotFound();
                    if (model.IsActive && !smtp.IsActive)
                    {
                        var activeSettings = await _context.SmtpEmails.Where(s => s.IsActive).ToListAsync();
                        activeSettings.ForEach(s => s.IsActive = false);
                    }

                    smtp.Host = model.Host;
                    smtp.Port = model.Port;
                    smtp.EnableSSL = model.EnableSSL;
                    smtp.SenderName = model.SenderName;
                    smtp.SenderEmail = model.SenderEmail;
                    smtp.Username = model.Username;
                    if (!string.IsNullOrEmpty(model.Password)) smtp.Password = model.Password;
                    smtp.IsActive = model.IsActive;

                    _context.Update(smtp);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Konfigurasi SMTP berhasil diperbarui.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }
    }
}
