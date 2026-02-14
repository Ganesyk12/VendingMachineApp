using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VendingMachineApp.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace VendingMachineApp.Controllers
{
    [Authorize]
    public class ProductController : Controller
    {
        private readonly VendingMachineContext _context;

        public ProductController(VendingMachineContext context)
        {
            _context = context;
        }

        // Menampilkan daftar produk
        public async Task<IActionResult> Index()
        {
            var products = await _context.Products.OrderBy(p => p.Name).ToListAsync();
            return View(products);
        }

        // Menampilkan form untuk membuat produk baru
        public async Task<IActionResult> Create()
        {
            ViewBag.ExistingProducts = await _context.Products.OrderBy(p => p.Name).ToListAsync();
            return View();
        }

        // Menangani POST untuk membuat produk baru atau update stok
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, bool isNewProduct, int? selectedProductId)
        {
            if (isNewProduct)
            {
                if (ModelState.IsValid)
                {
                    _context.Products.Add(product);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            else
            {
                if (selectedProductId.HasValue && product.Quantity > 0)
                {
                    var existingProduct = await _context.Products.FindAsync(selectedProductId.Value);
                    if (existingProduct != null)
                    {
                        existingProduct.Quantity += product.Quantity;
                        _context.Update(existingProduct);
                        await _context.SaveChangesAsync();
                        return RedirectToAction(nameof(Index));
                    }

                    ModelState.AddModelError("", "Produk tidak ditemukan.");
                }
                else
                {
                    ModelState.AddModelError("", "Harap pilih produk dan masukkan jumlah yang valid.");
                }
            }

            ViewBag.ExistingProducts = await _context.Products.OrderBy(p => p.Name).ToListAsync();
            return View(product);
        }

        // Menampilkan form untuk mengedit produk
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();
            return View(product);
        }

        // Menangani POST untuk mengedit produk
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product)
        {
            if (id != product.IdProduct) return NotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.IdProduct)) return NotFound();
                    else throw;
                }

                return RedirectToAction(nameof(Index));
            }

            return View(product);
        }

        // GET: Product/Delete/{id}
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var product = await _context.Products
                .FirstOrDefaultAsync(m => m.IdProduct == id);
            if (product == null) return NotFound();

            return View(product); // Pastikan produk dikirim ke view Delete
        }

        // POST: Product/DeleteConfirmed
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            Console.WriteLine($"ID Produk yang akan dihapus: {id}");
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.IdProduct == id);
        }
    }
}
