using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using VendingMachineApp.Models;
using VendingMachineApp.Models.ViewModels;
using System.Security.Claims;

namespace VendingMachineApp.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly VendingMachineContext _context;

    public HomeController(ILogger<HomeController> logger, VendingMachineContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var products = await _context.Products.OrderBy(p => p.Name).ToListAsync();

        if (User.Identity != null && User.Identity.IsAuthenticated)
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (userIdClaim != null && int.TryParse(userIdClaim, out int userId))
            {
                var userBalance = await _context.UserBalances.FirstOrDefaultAsync(ub => ub.IdUser == userId);
                ViewBag.CurrentBalance = userBalance?.Balance ?? 0;
                ViewBag.IdUser = userId;
            }
        }
        else
        {
            ViewBag.CurrentBalance = 0;
        }

        return View(products);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> AddBalance(decimal amount)
    {
        if (amount <= 0) return BadRequest("Nominal harus lebih dari 0.");

        var userIdClaim = User.FindFirst("UserId")?.Value;
        if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId)) return Unauthorized();

        var userBalance = await _context.UserBalances.FirstOrDefaultAsync(ub => ub.IdUser == userId);
        if (userBalance == null) return NotFound("Data saldo tidak ditemukan.");

        userBalance.Balance += amount;

        var history = new BalanceHistory
        {
            IdUser = userId,
            DebitBalance = amount,
            TransactionType = "Topup",
            Description = "Penambahan saldo mandiri"
        };

        _context.BalanceHistories.Add(history);
        await _context.SaveChangesAsync();

        return Json(new { success = true, newBalance = userBalance.Balance });
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> ProcessCheckout([FromBody] CheckoutRequest request)
    {
        if (request == null || request.Items.Count == 0) return BadRequest("Keranjang kosong.");

        var userIdClaim = User.FindFirst("UserId")?.Value;
        if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId)) return Unauthorized();

        var userBalance = await _context.UserBalances.FirstOrDefaultAsync(ub => ub.IdUser == userId);
        if (userBalance == null) return NotFound("User tidak ditemukan.");

        decimal totalCost = 0;
        foreach (var item in request.Items)
        {
            var product = await _context.Products.FindAsync(item.ProductId);
            if (product == null || product.Quantity < item.Quantity)
                return BadRequest($"Produk {(product?.Name ?? "Tidak Dikenal")} tidak mencukupi stok.");
            totalCost += product.Price * item.Quantity;
        }

        if (userBalance.Balance < totalCost) return BadRequest("Saldo tidak mencukupi.");

        // Process Transaction
        var today = DateTime.Today;
        var prefix = $"VM-{today:yyyyMMdd}-";
        var countToday = await _context.Transactions
            .CountAsync(t => t.Date >= today && t.Date < today.AddDays(1));
        int seq = countToday + 1;

        foreach (var item in request.Items)
        {
            var product = await _context.Products.FindAsync(item.ProductId);
            if (product == null) continue;

            product.Quantity -= item.Quantity;

            var transaction = new Transaction
            {
                IdUser = userId,
                IdProduct = product.IdProduct,
                Amount = product.Price * item.Quantity,
                TransactionType = "Pembelian",
                Date = DateTime.Now,
                TrxCode = $"{prefix}{seq:D3}",
                BalanceAfterTransaction = userBalance.Balance - (product.Price * item.Quantity)
            };
            userBalance.Balance -= (product.Price * item.Quantity);
            _context.Transactions.Add(transaction);

            // Record to BalanceHistory
            var history = new BalanceHistory
            {
                IdUser = userId,
                CreditBalance = (product.Price * item.Quantity),
                TransactionType = "Purchase",
                Description = $"Pembelian {product.Name} x {item.Quantity}"
            };
            _context.BalanceHistories.Add(history);
        }

        await _context.SaveChangesAsync();
        return Json(new { success = true, newBalance = userBalance.Balance });
    }

    [Authorize]
    public async Task<IActionResult> PersonalBalanceHistory()
    {
        var userIdClaim = User.FindFirst("UserId")?.Value;
        if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId)) return Unauthorized();

        var history = await _context.BalanceHistories
            .Where(bh => bh.IdUser == userId)
            .OrderByDescending(bh => bh.DateCreated)
            .ToListAsync();

        return View(history);
    }
}
