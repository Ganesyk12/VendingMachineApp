using Microsoft.EntityFrameworkCore;
using VendingMachineApp.Models;
using BC = BCrypt.Net.BCrypt;

namespace VendingMachineApp.Services;

public interface IAnonymousUserService
{
    /// <summary>
    /// Mengambil user default "Anonym" untuk pengunjung tanpa login.
    /// Jika belum ada, user beserta saldo awalnya akan dibuat otomatis.
    /// </summary>
    Task<UserLogin> GetOrCreateAsync();

    Task<int> GetOrCreateIdAsync();
}

public class AnonymousUserService : IAnonymousUserService
{
    public const string AnonymousUserName = "anonym@vending.local";
    public const string AnonymousDisplayName = "Anonym";

    private static readonly SemaphoreSlim CreationLock = new(1, 1);

    private readonly VendingMachineContext _context;

    public AnonymousUserService(VendingMachineContext context)
    {
        _context = context;
    }

    public async Task<int> GetOrCreateIdAsync()
    {
        var user = await GetOrCreateAsync();
        return user.IdUser;
    }

    public async Task<UserLogin> GetOrCreateAsync()
    {
        var user = await _context.UserLogins
            .Include(u => u.UserBalance)
            .FirstOrDefaultAsync(u => u.UserName == AnonymousUserName);

        if (user != null)
        {
            return await EnsureBalanceAsync(user);
        }

        // Serialize creation so concurrent guest requests don't create duplicates.
        await CreationLock.WaitAsync();
        try
        {
            user = await _context.UserLogins
                .Include(u => u.UserBalance)
                .FirstOrDefaultAsync(u => u.UserName == AnonymousUserName);

            if (user != null)
            {
                return await EnsureBalanceAsync(user);
            }

            user = new UserLogin
            {
                UserName = AnonymousUserName,
                // Anonym tidak boleh login; password diisi hash acak yang tidak diketahui siapa pun.
                Password = BC.HashPassword(Guid.NewGuid().ToString("N")),
                Status = "A"
            };

            _context.UserLogins.Add(user);
            _context.UserBalances.Add(new UserBalance
            {
                User = user,
                Name = AnonymousDisplayName,
                Balance = 0
            });

            await _context.SaveChangesAsync();
            return user;
        }
        finally
        {
            CreationLock.Release();
        }
    }

    private async Task<UserLogin> EnsureBalanceAsync(UserLogin user)
    {
        var changed = false;

        if (user.Status != "A")
        {
            user.Status = "A";
            changed = true;
        }

        if (user.UserBalance == null)
        {
            user.UserBalance = new UserBalance
            {
                IdUser = user.IdUser,
                Name = AnonymousDisplayName,
                Balance = 0
            };
            _context.UserBalances.Add(user.UserBalance);
            changed = true;
        }

        if (changed)
        {
            await _context.SaveChangesAsync();
        }

        return user;
    }
}
