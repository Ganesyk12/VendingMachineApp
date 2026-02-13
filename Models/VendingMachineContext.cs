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

        var currentUser =
            _httpContextAccessor.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ??
            "SYSTEM";

        foreach (var entry in entries)
        {
            var entity = (BaseEntity)entry.Entity;
            if (entry.State == EntityState.Added)
            {
                if (entity.UserCreated == "SYSTEM") entity.UserCreated = currentUser;
                entity.DateCreated = DateTime.Now;
                entity.UserModified = currentUser;
                entity.DateModified = DateTime.Now;
            }
            else if (entry.State == EntityState.Modified)
            {
                entity.UserModified = currentUser;
                entity.DateModified = DateTime.Now;
            }
        }
    }
}
