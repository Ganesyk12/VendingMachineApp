using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VendingMachineApp.Data;
using VendingMachineApp.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace VendingMachineApp.Controllers
{
    public class TransactionController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TransactionController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Menampilkan halaman Index yang berisi daftar transaksi
        public async Task<IActionResult> Index()
        {
            var transactions = await _context.Transactions
                .Include(t => t.User)
                .Include(t => t.Product)
                .OrderByDescending(t => t.Date)
                .ToListAsync();
            return View(transactions);
        }

        // Menampilkan form untuk membuat transaksi baru (misalnya pembelian produk)
        public async Task<IActionResult> Create()
        {
            ViewData["Users"] = await _context.Users.ToListAsync();
            ViewData["Products"] = await _context.Products.Where(p => p.Quantity > 0).ToListAsync();
            return View();
        }

        // Menangani POST untuk membuat transaksi
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int userId, int? IdProduct, decimal amount, string transactionType)
        {
            var user = await _context.Users.FindAsync(userId);
            var product = IdProduct != null ? await _context.Products.FindAsync(IdProduct) : null;

            if (user == null) {
                ModelState.AddModelError("User", "User tidak ditemukan");
                return View();
            }

            decimal balanceAfterTransaction = user.Balance ?? 0;
            if (transactionType == "Pembelian" && product != null)
            {
                if (user.Balance < product.Price)
                {
                    ModelState.AddModelError("Balance", "Saldo tidak mencukupi untuk melakukan pembelian.");
                    return View();
                }
                
                balanceAfterTransaction -= product.Price;
                product.Quantity--;
                amount = product.Price;
            }
            else if (transactionType == "Penambahan Saldo") {
                balanceAfterTransaction += amount;
            }

            var transaction = new Transaction
            {
                UserId = userId,
                IdProduct = IdProduct,
                Amount = amount,
                Date = DateTime.Now,
                BalanceAfterTransaction = balanceAfterTransaction,
                TransactionType = transactionType
            };

            user.Balance = balanceAfterTransaction;
            _context.Transactions.Add(transaction);

            if (product != null) _context.Products.Update(product);
            _context.Users.Update(user);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
