using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VendingMachineApp.Models;
using VendingMachineApp.Models.ViewModels;
using BC = BCrypt.Net.BCrypt;

namespace VendingMachineApp.Controllers
{
    [Authorize]
    public class UserManagementController : Controller
    {
        private readonly VendingMachineContext _context;

        public UserManagementController(VendingMachineContext context)
        {
            _context = context;
        }

        // GET: UserManagement
        public async Task<IActionResult> Index()
        {
            var users = await _context.UserLogins
                .Include(u => u.UserBalance)
                .Select(u => new UserListViewModel
                {
                    IdUser = u.IdUser,
                    UserName = u.UserName,
                    Name = u.UserBalance != null ? u.UserBalance.Name : "N/A",
                    Balance = u.UserBalance != null ? u.UserBalance.Balance : 0
                })
                .ToListAsync();

            return View(users);
        }

        // GET: UserManagement/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: UserManagement/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
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

                var balance = new UserBalance
                {
                    User = user,
                    Name = model.Name,
                    Balance = 0
                };
                _context.UserBalances.Add(balance);

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "User berhasil ditambahkan.";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // GET: UserManagement/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.UserLogins
                .Include(u => u.UserBalance)
                .FirstOrDefaultAsync(m => m.IdUser == id);

            if (user == null)
            {
                return NotFound();
            }

            var model = new UserEditViewModel
            {
                IdUser = user.IdUser,
                Email = user.UserName,
                Name = user.UserBalance?.Name ?? string.Empty
            };

            return View(model);
        }

        // POST: UserManagement/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UserEditViewModel model)
        {
            if (id != model.IdUser)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var userToUpdate = await _context.UserLogins
                        .Include(u => u.UserBalance)
                        .FirstOrDefaultAsync(u => u.IdUser == id);

                    if (userToUpdate == null)
                    {
                        return NotFound();
                    }

                    // Check if email is unqiue
                    if (userToUpdate.UserName != model.Email)
                    {
                        var existingEmail =
                            await _context.UserLogins.FirstOrDefaultAsync(u => u.UserName == model.Email);
                        if (existingEmail != null)
                        {
                            ModelState.AddModelError("Email", "Email sudah terdaftar.");
                            return View(model);
                        }

                        userToUpdate.UserName = model.Email;
                    }

                    if (!string.IsNullOrEmpty(model.Password))
                    {
                        userToUpdate.Password = BC.HashPassword(model.Password);
                    }

                    if (userToUpdate.UserBalance != null)
                    {
                        userToUpdate.UserBalance.Name = model.Name;
                    }
                    else
                    {
                        _context.UserBalances.Add(new UserBalance
                        {
                            IdUser = userToUpdate.IdUser,
                            Name = model.Name,
                            Balance = 0
                        });
                    }

                    _context.Update(userToUpdate);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "User berhasil diperbarui.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(model.IdUser))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // POST: UserManagement/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.UserLogins
                .Include(u => u.UserBalance)
                .Include(u => u.BalanceHistories)
                .Include(u => u.UserTransactions)
                .FirstOrDefaultAsync(u => u.IdUser == id);

            if (user != null)
            {
                // Due to cascading restrictions, explicitly remove related records
                _context.BalanceHistories.RemoveRange(user.BalanceHistories);
                _context.UserTransactions.RemoveRange(user.UserTransactions);
                if (user.UserBalance != null)
                {
                    _context.UserBalances.Remove(user.UserBalance);
                }

                _context.UserLogins.Remove(user);

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "User berhasil dihapus.";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool UserExists(int id)
        {
            return _context.UserLogins.Any(e => e.IdUser == id);
        }
    }
}
