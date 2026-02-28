using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VendingMachineApp.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using VendingMachineApp.Services;
using VendingMachineApp.Helpers;

namespace VendingMachineApp.Controllers
{
    [Authorize]
    public class TransactionController : Controller
    {
        private readonly VendingMachineContext _context;
        private readonly IEmailService _emailService;

        public TransactionController(VendingMachineContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task<IActionResult> Index()
        {
            var transactions = await _context.UserTransactions
                .Include(t => t.User).ThenInclude(u => u.UserBalance)
                .OrderByDescending(t => t.Date)
                .ToListAsync();
            return View(transactions);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var transaction = await _context.UserTransactions
                .Include(t => t.User).ThenInclude(u => u.UserBalance)
                .Include(t => t.TransactionDetails)
                .ThenInclude(d => d.Product)
                .FirstOrDefaultAsync(t => t.IdTransaction == id);

            if (transaction == null) return NotFound();

            return View(transaction);
        }

        // Menampilkan form untuk membuat transaksi baru (misalnya pembelian produk)
        public async Task<IActionResult> Create()
        {
            ViewData["Users"] = await _context.UserLogins.ToListAsync();
            ViewData["Products"] = await _context.Products.Where(p => p.Quantity > 0).ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int idUser, int? idProduct, decimal amount, string transactionType)
        {
            var user = await _context.UserLogins.Include(u => u.UserBalance)
                .FirstOrDefaultAsync(u => u.IdUser == idUser);
            var product = idProduct != null ? await _context.Products.FindAsync(idProduct) : null;

            if (user == null || user.UserBalance == null)
            {
                ModelState.AddModelError("User", "User atau data saldo tidak ditemukan");
                ViewData["Users"] = await _context.UserLogins.ToListAsync();
                ViewData["Products"] = await _context.Products.Where(p => p.Quantity > 0).ToListAsync();
                return View();
            }

            decimal balanceAfterTransaction = user.UserBalance.Balance;
            decimal debit = 0;
            decimal credit = 0;

            if (transactionType == "Pembelian" && product != null)
            {
                if (user.UserBalance.Balance < product.Price)
                {
                    ModelState.AddModelError("Balance", "Saldo tidak mencukupi untuk melakukan pembelian.");
                    ViewData["Users"] = await _context.UserLogins.ToListAsync();
                    ViewData["Products"] = await _context.Products.Where(p => p.Quantity > 0).ToListAsync();
                    return View();
                }

                balanceAfterTransaction -= product.Price;
                product.Quantity--;
                amount = product.Price;
                credit = amount;
            }
            else if (transactionType == "Penambahan Saldo")
            {
                balanceAfterTransaction += amount;
                debit = amount;
            }

            var today = DateTime.Today;
            var prefix = $"VM-{today:yyyyMMdd}-";
            var countToday = await _context.UserTransactions
                .CountAsync(t => t.Date >= today && t.Date < today.AddDays(1));
            string trxCode = $"{prefix}{(countToday + 1):D3}";

            var userTransaction = new UserTransaction
            {
                IdUser = idUser,
                TrxCode = trxCode,
                TotalAmount = amount,
                Date = DateTime.Now,
                BalanceAfterTransaction = balanceAfterTransaction,
                TransactionType = transactionType
            };

            _context.UserTransactions.Add(userTransaction);

            if (product != null)
            {
                var detail = new TransactionDetail
                {
                    UserTransaction = userTransaction,
                    IdProduct = idProduct,
                    Price = product.Price,
                    Quantity = 1,
                    SubTotal = amount
                };
                _context.TransactionDetails.Add(detail);
                _context.Products.Update(product);
            }

            user.UserBalance.Balance = balanceAfterTransaction;

            // Record Ledger
            var history = new BalanceHistory
            {
                IdUser = idUser,
                DebitBalance = debit,
                CreditBalance = credit,
                TransactionType = transactionType == "Pembelian" ? "Purchase" : "Adjustment",
                Description = transactionType == "Pembelian" ? $"Beli {product?.Name}" : "Penyesuaian saldo manual"
            };
            _context.BalanceHistories.Add(history);

            if (product != null) _context.Products.Update(product);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> SendReceiptEmail(int id)
        {
            var transaction = await _context.UserTransactions
                .Include(t => t.User).ThenInclude(u => u.UserBalance)
                .Include(t => t.TransactionDetails)
                .ThenInclude(d => d.Product)
                .FirstOrDefaultAsync(t => t.IdTransaction == id);

            if (transaction == null || transaction.User == null)
            {
                return Json(new { success = false, message = "Transaksi tidak ditemukan." });
            }

            try
            {
                var pdfBytes = ReceiptPdfGenerator.Generate(transaction);
                string fileName = $"Receipt_{transaction.TrxCode}.pdf";
                string subject = $"Struk Transaksi - {transaction.TrxCode}";
                string htmlMessage = MessageBuilder.BuildReceiptEmailBody(transaction);

                bool isSent = await _emailService.SendEmailWithAttachmentAsync(
                    transaction.User.UserName, subject, htmlMessage, pdfBytes, fileName);

                if (isSent)
                {
                    return Json(new { success = true, message = "Struk PDF berhasil dikirim ke email Anda!" });
                }

                return Json(new { success = false, message = "Gagal mengirim email. Pastikan koneksi SMTP aktif." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Terjadi kesalahan sistem.", error = ex.Message });
            }
        }
    }
}
