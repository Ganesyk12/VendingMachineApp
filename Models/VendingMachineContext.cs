using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

namespace VendingMachineApp.Models;

public class VendingMachineContext : DbContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public VendingMachineContext(DbContextOptions<VendingMachineContext> options,
        IHttpContextAccessor httpContextAccessor) : base(options)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public DbSet<UserLogin> UserLogins { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<BalanceHistory> BalanceHistories { get; set; }
    public DbSet<UserBalance> UserBalances { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("vendingmachine");
    }

    public override int SaveChanges()
    {
        AddAuditInfo();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        AddAuditInfo();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void AddAuditInfo()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is BaseEntity && (e.State == EntityState.Added || e.State == EntityState.Modified));

        var user = _httpContextAccessor.HttpContext?.User;
        var currentUser = user?.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value
                          ?? user?.Identity?.Name
                          ?? "SYSTEM";

        foreach (var entry in entries)
        {
            var entity = (BaseEntity)entry.Entity;
            var now = DateTime.Now;

            if (entry.State == EntityState.Added)
            {
                // Rule: Registration entities default to SYSTEM, others use the active user
                var creator = (entity is UserLogin || entity is UserBalance) ? "SYSTEM" : currentUser;

                entity.UserCreated = creator;
                entity.DateCreated = now;
                entity.UserModified = creator;
                entity.DateModified = now;
            }
            else if (entry.State == EntityState.Modified)
            {
                entity.UserModified = currentUser;
                entity.DateModified = now;
            }
        }
    }
}
