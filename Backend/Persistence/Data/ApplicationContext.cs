using Domain.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Persistence.Data;

public class ApplicationContext(DbContextOptions<ApplicationContext> options)
    : DbContext(options)
{
    public DbSet<Resource> Resources { get; set; }
    public DbSet<Unit> Units { get; set; }
    public DbSet<Client> Clients { get; set; }
    public DbSet<ReceiptDocument> ReceiptDocuments { get; set; }
    public DbSet<ReceiptItem> ReceiptItems { get; set; }

    public DbSet<ShipmentDocument> ShipmentDocuments { get; set; }
    public DbSet<ShipmentItem> ShipmentItems { get; set; }
    public DbSet<Balance> Balances { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Resource>()
            .HasIndex(r => r.Name)
            .IsUnique();
        modelBuilder.Entity<Resource>()
            .Property(r => r.Name)
            .IsRequired();

        modelBuilder.Entity<Unit>()
            .HasIndex(u => u.Name)
            .IsUnique();
        modelBuilder.Entity<Unit>()
            .Property(u => u.Name)
            .IsRequired();

        modelBuilder.Entity<Client>()
            .HasIndex(c => c.Name)
            .IsUnique();
        modelBuilder.Entity<Client>()
            .Property(c => c.Name)
            .IsRequired();

        modelBuilder.Entity<Balance>()
            .HasIndex(b => new { b.ResourceId, b.UnitId })
            .IsUnique();

        modelBuilder.Entity<Balance>()
            .HasOne(b => b.Resource)
            .WithMany()
            .HasForeignKey(b => b.ResourceId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Balance>()
            .HasOne(b => b.Unit)
            .WithMany()
            .HasForeignKey(b => b.UnitId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ReceiptDocument>()
            .HasIndex(rd => rd.Number)
            .IsUnique();

        modelBuilder.Entity<ReceiptItem>()
            .HasOne(ri => ri.Document)
            .WithMany(rd => rd.Items)
            .HasForeignKey(ri => ri.DocumentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ReceiptItem>()
            .HasOne(ri => ri.Resource)
            .WithMany()
            .HasForeignKey(ri => ri.ResourceId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ReceiptItem>()
            .HasOne(ri => ri.Unit)
            .WithMany()
            .HasForeignKey(ri => ri.UnitId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ShipmentDocument>()
            .HasIndex(sd => sd.Number)
            .IsUnique();

        modelBuilder.Entity<ShipmentDocument>()
            .HasOne(sd => sd.Client)
            .WithMany()
            .HasForeignKey(sd => sd.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ShipmentItem>()
            .HasOne(si => si.Document)
            .WithMany(sd => sd.Items)
            .HasForeignKey(si => si.DocumentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ShipmentItem>()
            .HasOne(si => si.Resource)
            .WithMany()
            .HasForeignKey(si => si.ResourceId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ShipmentItem>()
            .HasOne(si => si.Unit)
            .WithMany()
            .HasForeignKey(si => si.UnitId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
