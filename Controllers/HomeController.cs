using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using VendingMachineApp.Models;
using VendingMachineApp.Models.ViewModels;
using VendingMachineApp.Services;
using System.Security.Claims;

namespace VendingMachineApp.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly VendingMachineContext _context;
    private readonly IAnonymousUserService _anonymousUserService;

    public HomeController(
        ILogger<HomeController> logger,
        VendingMachineContext context,
        IAnonymousUserService anonymousUserService)
    {
        _logger = logger;
        _context = context;
        _anonymousUserService = anonymousUserService;
    }

    public async Task<IActionResult> Index()
    {
        var products = await _context.Products.OrderBy(p => p.Name).ToListAsync();

        var loggedInUserId = GetLoggedInUserId();
        var isAnonymous = loggedInUserId == null;
        var userId = loggedInUserId ?? await _anonymousUserService.GetOrCreateIdAsync();

        var userBalance = await _context.UserBalances.FirstOrDefaultAsync(ub => ub.IdUser == userId);

        ViewBag.CurrentBalance = userBalance?.Balance ?? 0;
        ViewBag.IdUser = userId;
        ViewBag.IsAnonymous = isAnonymous;

        return View(products);
    }

    [HttpPost]
    public async Task<IActionResult> AddBalance(decimal amount)
    {
        if (amount <= 0) return BadRequest("Nominal harus lebih dari 0.");

        var userId = await GetEffectiveUserIdAsync();

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

    [HttpPost]
    public async Task<IActionResult> ProcessCheckout([FromBody] CheckoutRequest request)
    {
        if (request == null || request.Items.Count == 0) return BadRequest("Keranjang kosong.");

        var userId = await GetEffectiveUserIdAsync();

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
        var countToday = await _context.UserTransactions
            .CountAsync(t => t.Date >= today && t.Date < today.AddDays(1));
        int seq = countToday + 1;

        var trxCode = $"{prefix}{seq:D3}";

        var userTransaction = new UserTransaction
        {
            IdUser = userId,
            TrxCode = trxCode,
            TotalAmount = totalCost,
            Date = DateTime.Now,
            BalanceAfterTransaction = userBalance.Balance - totalCost,
            TransactionType = "Pembelian"
        };

        _context.UserTransactions.Add(userTransaction);

        var details = new List<TransactionDetail>();
        var descriptionItems = new List<string>();

        foreach (var item in request.Items)
        {
            var product = await _context.Products.FindAsync(item.ProductId);
            if (product == null) continue;

            product.Quantity -= item.Quantity;

            var detail = new TransactionDetail
            {
                UserTransaction = userTransaction,
                IdProduct = product.IdProduct,
                Price = product.Price,
                Quantity = item.Quantity,
                SubTotal = product.Price * item.Quantity
            };
            details.Add(detail);
            descriptionItems.Add($"{product.Name} (x{item.Quantity})");

            _context.TransactionDetails.Add(detail);
        }

        userBalance.Balance -= totalCost;

        // Record to BalanceHistory once for the entire checkout
        // To avoid making string too long, truncate if needed, or simply list items
        var desc = $"Pembelian: {string.Join(", ", descriptionItems)}";
        if (desc.Length > 255) desc = desc.Substring(0, 252) + "...";

        var history = new BalanceHistory
        {
            IdUser = userId,
            CreditBalance = totalCost,
            TransactionType = "Purchase",
            Description = desc
        };
        _context.BalanceHistories.Add(history);

        await _context.SaveChangesAsync();
        return Json(new { success = true, newBalance = userBalance.Balance });
    }

    public async Task<IActionResult> PersonalBalanceHistory()
    {
        var userId = await GetEffectiveUserIdAsync();
        ViewBag.IsAnonymous = GetLoggedInUserId() == null;

        var history = await _context.BalanceHistories
            .Where(bh => bh.IdUser == userId)
            .OrderByDescending(bh => bh.DateCreated)
            .ToListAsync();

        return View(history);
    }

    private int? GetLoggedInUserId()
    {
        var userIdClaim = User.FindFirst("UserId")?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : null;
    }

    /// <summary>
    /// Mengembalikan user yang sedang aktif: user login jika ada, atau user default "Anonym".
    /// </summary>
    private async Task<int> GetEffectiveUserIdAsync()
    {
        var loggedInUserId = GetLoggedInUserId();
        if (loggedInUserId.HasValue) return loggedInUserId.Value;

        return await _anonymousUserService.GetOrCreateIdAsync();
    }
}
