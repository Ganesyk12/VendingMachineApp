using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VendingMachineApp.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace VendingMachineApp.Controllers
{
    [Authorize]
    public class TransactionController : Controller
    {
        private readonly VendingMachineContext _context;

        public TransactionController(VendingMachineContext context)
        {
            _context = context;
        }

        // Menampilkan halaman Index yang berisi daftar transaksi
        public async Task<IActionResult> Index()
        {
            var transactions = await _context.Transactions
                .Include(t => t.User).ThenInclude(u => u.UserBalance)
                .Include(t => t.Product)
                .OrderByDescending(t => t.Date)
                .ToListAsync();
            return View(transactions);
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
            var countToday = await _context.Transactions
                .CountAsync(t => t.Date >= today && t.Date < today.AddDays(1));
            string trxCode = $"{prefix}{(countToday + 1):D3}";

            var transaction = new Transaction
            {
                IdUser = idUser,
                IdProduct = idProduct,
                Amount = amount,
                Date = DateTime.Now,
                TrxCode = trxCode,
                BalanceAfterTransaction = balanceAfterTransaction,
                TransactionType = transactionType
            };

            user.UserBalance.Balance = balanceAfterTransaction;
            _context.Transactions.Add(transaction);

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
    }
}
