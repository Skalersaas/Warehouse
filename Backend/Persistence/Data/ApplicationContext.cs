using Domain.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Npgsql.PostgresTypes;
using Persistence.Data.Configurations;
using Utilities;

namespace Persistence.Data;

public class ApplicationContext : DbContext
{
    public ApplicationContext() { }
    public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options) { }

    public DbSet<Client> Clients { get; set; }
    public DbSet<Resource> Resources { get; set; }
    public DbSet<Unit> Units { get; set; }
    public DbSet<Balance> Balances { get; set; }
    public DbSet<ReceiptDocument> ReceiptDocuments { get; set; }
    public DbSet<ReceiptItem> ReceiptItems { get; set; }
    public DbSet<ShipmentDocument> ShipmentDocuments { get; set; }
    public DbSet<ShipmentItem> ShipmentItems { get; set; }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
#if DEBUG
            foreach (var path in new[] { ".env", "../.env" })
            {
                if (File.Exists(path))
                {
                    EnvLoader.LoadEnvFile(path);
                    break;
                }
            }
#endif
            optionsBuilder.UseNpgsql(Environment.GetEnvironmentVariable("ConnectionString"));
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations
        modelBuilder.ApplyConfiguration(new ClientConfiguration());
        modelBuilder.ApplyConfiguration(new ResourceConfiguration());
        modelBuilder.ApplyConfiguration(new UnitConfiguration());
        modelBuilder.ApplyConfiguration(new BalanceConfiguration());
        modelBuilder.ApplyConfiguration(new ReceiptDocumentConfiguration());
        modelBuilder.ApplyConfiguration(new ReceiptItemConfiguration());
        modelBuilder.ApplyConfiguration(new ShipmentDocumentConfiguration());
        modelBuilder.ApplyConfiguration(new ShipmentItemConfiguration());

        // Global query filters for archived entities (soft delete)
        modelBuilder.Entity<Client>().HasQueryFilter(e => !e.IsArchived);
        modelBuilder.Entity<Resource>().HasQueryFilter(e => !e.IsArchived);
        modelBuilder.Entity<Unit>().HasQueryFilter(e => !e.IsArchived);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return await base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.Entity.GetType().GetProperty("UpdatedAt") != null)
            {
                entry.Property("UpdatedAt").CurrentValue = DateTime.UtcNow;
            }
        }
    }
}
