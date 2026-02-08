using Microsoft.EntityFrameworkCore;
namespace VendingMachineApp.Models;

    public class VendingMachineContext : DbContext
    {
        public VendingMachineContext(DbContextOptions<VendingMachineContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Transaction> Transactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.HasDefaultSchema("vendingmachine");
        }
    }
