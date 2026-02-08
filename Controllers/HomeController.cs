using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using VendingMachineApp.Models;

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

    public IActionResult Index()
    {
        var products = _context.Products.OrderBy(p => p.Name).ToList();
        return View(products);
    }

    [HttpPost]
    public IActionResult RegisterUser(string name, decimal balance)
    {
        if (string.IsNullOrEmpty(name)) return BadRequest("Nama harus diisi.");

        var user = _context.Users.FirstOrDefault(u => u.Name == name);
        if (user == null)
        {
            user = new User { Name = name, Balance = balance };
            _context.Users.Add(user);
        }
        else
        {
            user.Balance = balance; // Reset balance for this simulation if user already exists
            _context.Users.Update(user);
        }

        _context.SaveChanges();
        return Json(new { success = true, userId = user.UserId, balance = user.Balance });
    }

    [HttpPost]
    public async Task<IActionResult> ProcessCheckout([FromBody] CheckoutRequest request)
    {
        if (request == null || request.Items.Count == 0) return BadRequest("Keranjang kosong.");

        var user = await _context.Users.FindAsync(request.IdUser);
        if (user == null) return NotFound("User tidak ditemukan.");

        decimal totalCost = 0;
        foreach (var item in request.Items)
        {
            var product = await _context.Products.FindAsync(item.ProductId);
            if (product == null || product.Quantity < item.Quantity)
            {
                return BadRequest($"Produk {(product?.Name ?? "Tidak Dikenal")} tidak mencukupi stok.");
            }
            totalCost += product.Price * item.Quantity;
        }

        if (user.Balance < totalCost) return BadRequest("Saldo tidak mencukupi.");

        // Process Transaction
        foreach (var item in request.Items)
        {
            var product = await _context.Products.FindAsync(item.ProductId);
            if (product == null) continue; // Should not happen due to previous check

            product.Quantity -= item.Quantity;

            var transaction = new Transaction
            {
                IdUser = user.UserId,
                IdProduct = product.IdProduct,
                Amount = product.Price * item.Quantity,
                TransactionType = "Pembelian",
                Date = DateTime.Now
            };
            
            user.Balance -= (product.Price * item.Quantity);
            transaction.BalanceAfterTransaction = user.Balance;
            
            _context.Transactions.Add(transaction);
        }

        await _context.SaveChangesAsync();
        return Json(new { success = true, newBalance = user.Balance });
    }
}
