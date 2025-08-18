using Domain.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using Utilities;

namespace Persistence.Data;

public class ApplicationContext : DbContext
{
    public ApplicationContext()
    {
    }

    public ApplicationContext(DbContextOptions<ApplicationContext> options)
        : base(options)
    {
    }

    public DbSet<Resource> Resources { get; set; }
    public DbSet<Unit> Units { get; set; }
    public DbSet<Client> Clients { get; set; }
    public DbSet<ReceiptDocument> ReceiptDocuments { get; set; }
    public DbSet<ReceiptItem> ReceiptItems { get; set; }

    public DbSet<ShipmentDocument> ShipmentDocuments { get; set; }
    public DbSet<ShipmentItem> ShipmentItems { get; set; }
    public DbSet<Balance> Balances { get; set; }
    public DbSet<WorkerExecution> WorkerExecutions { get; set; }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
#if DEBUG
        EnvLoader.LoadEnvFile("../.env");
#endif
        optionsBuilder.UseNpgsql(Environment.GetEnvironmentVariable("ConnectionString"));
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Resource entity configuration
        modelBuilder.Entity<Resource>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.HasIndex(r => r.Name).IsUnique();
            entity.Property(r => r.Name).IsRequired();
        });

        // Unit entity configuration
        modelBuilder.Entity<Unit>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.HasIndex(u => u.Name).IsUnique();
            entity.Property(u => u.Name).IsRequired();
        });

        // Client entity configuration
        modelBuilder.Entity<Client>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.HasIndex(c => c.Name).IsUnique();
            entity.Property(c => c.Name).IsRequired();
        });

        // Balance entity configuration
        modelBuilder.Entity<Balance>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.HasIndex(b => new { b.ResourceId, b.UnitId }).IsUnique();

            entity.HasOne(b => b.Resource)
                .WithMany(r => r.Balances)
                .HasForeignKey(b => b.ResourceId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(b => b.Unit)
                .WithMany(u => u.Balances)
                .HasForeignKey(b => b.UnitId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ReceiptDocument entity configuration
        modelBuilder.Entity<ReceiptDocument>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.HasIndex(rd => rd.Number).IsUnique();
            entity.Property(rd => rd.Number).IsRequired();
        });

        // ReceiptItem entity configuration
        modelBuilder.Entity<ReceiptItem>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedOnAdd();

            entity.HasOne(ri => ri.Document)
                .WithMany(rd => rd.Items)
                .HasForeignKey(ri => ri.DocumentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ri => ri.Resource)
                .WithMany(r => r.ReceiptItems)
                .HasForeignKey(ri => ri.ResourceId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(ri => ri.Unit)
                .WithMany(u => u.ReceiptItems)
                .HasForeignKey(ri => ri.UnitId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ShipmentDocument entity configuration
        modelBuilder.Entity<ShipmentDocument>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.HasIndex(sd => sd.Number).IsUnique();
            entity.Property(sd => sd.Number).IsRequired();

            entity.HasOne(sd => sd.Client)
                .WithMany()
                .HasForeignKey(sd => sd.ClientId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ShipmentItem entity configuration
        modelBuilder.Entity<ShipmentItem>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedOnAdd();

            entity.HasOne(si => si.Document)
                .WithMany(sd => sd.Items)
                .HasForeignKey(si => si.DocumentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(si => si.Resource)
                .WithMany(r => r.ShipmentItems)
                .HasForeignKey(si => si.ResourceId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(si => si.Unit)
                .WithMany(u => u.ShipmentItems)
                .HasForeignKey(si => si.UnitId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<WorkerExecution>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
        });
    }
}
