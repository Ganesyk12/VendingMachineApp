using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VendingMachineApp.Models;
using VendingMachineApp.Models.ViewModels;

namespace VendingMachineApp.Controllers
{
    [Authorize(Roles = "VendingApps-SA")]
    public class UserRoleController : Controller
    {
        private readonly VendingMachineContext _context;

        public UserRoleController(VendingMachineContext context)
        {
            _context = context;
        }

        // GET: UserRole
        public async Task<IActionResult> Index(int? userId)
        {
            var query = _context.UserRoles
                .Include(u => u.UserLogin)
                .AsQueryable();

            if (userId != null)
            {
                query = query.Where(r => r.IdUser == userId);
                var user = await _context.UserLogins.FindAsync(userId);
                ViewBag.UserId = userId;
                ViewBag.UserName = user?.UserName ?? "User";
            }

            var roles = await query.Select(r => new UserRoleListViewModel
                {
                    IdUserRole = r.IdUserRole,
                    IdUser = r.IdUser,
                    UserName = r.UserLogin != null ? r.UserLogin.UserName : "Unknown",
                    RoleName = r.RoleName,
                    Status = r.Status
                })
                .ToListAsync();

            return View(roles);
        }

        // GET: UserRole/Create
        public async Task<IActionResult> Create(int? userId)
        {
            var model = new UserRoleCreateViewModel
            {
                IdUser = userId ?? 0,
                UserList = await GetUserSelectList(),
                Status = "A"
            };
            return View(model);
        }

        // POST: UserRole/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserRoleCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userRole = new UserRole
                {
                    IdUser = model.IdUser,
                    RoleName = model.RoleName
                };

                _context.UserRoles.Add(userRole);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Role berhasil ditambahkan.";
                return RedirectToAction(nameof(Index), new { userId = userRole.IdUser });
            }

            model.UserList = await GetUserSelectList();
            return View(model);
        }

        // GET: UserRole/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userRole = await _context.UserRoles.FindAsync(id);
            if (userRole == null)
            {
                return NotFound();
            }

            var model = new UserRoleEditViewModel
            {
                IdUserRole = userRole.IdUserRole,
                IdUser = userRole.IdUser,
                RoleName = userRole.RoleName,
                UserList = await GetUserSelectList()
            };

            return View(model);
        }

        // POST: UserRole/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UserRoleEditViewModel model)
        {
            if (id != model.IdUserRole)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var userRole = await _context.UserRoles.FindAsync(id);
                    if (userRole == null)
                    {
                        return NotFound();
                    }

                    userRole.IdUser = model.IdUser;
                    userRole.RoleName = model.RoleName;

                    _context.Update(userRole);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Role berhasil diperbarui.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserRoleExists(model.IdUserRole))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index), new { userId = model.IdUser });
            }

            model.UserList = await GetUserSelectList();
            return View(model);
        }

        // POST: UserRole/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userRole = await _context.UserRoles.FindAsync(id);
            if (userRole != null)
            {
                int userId = userRole.IdUser;
                _context.UserRoles.Remove(userRole);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Role berhasil dihapus.";
                return RedirectToAction(nameof(Index), new { userId = userId });
            }

            return RedirectToAction(nameof(Index));
        }

        private bool UserRoleExists(int id)
        {
            return _context.UserRoles.Any(e => e.IdUserRole == id);
        }

        private async Task<List<SelectListItem>> GetUserSelectList()
        {
            return await _context.UserLogins
                .Select(u => new SelectListItem
                {
                    Value = u.IdUser.ToString(),
                    Text = u.UserName
                })
                .ToListAsync();
        }
    }
}
