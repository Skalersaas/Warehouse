using Domain.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Data.Configurations;
public class UnitConfiguration : IEntityTypeConfiguration<Unit>
{
    public void Configure(EntityTypeBuilder<Unit> builder)
    {
        builder.ToTable("Units");
        
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();
        
        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(50);
            
        builder.Property(e => e.IsArchived)
            .IsRequired()
            .HasDefaultValue(false);
            
        builder.Property(e => e.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
            
        // Indexes
        builder.HasIndex(e => e.Name)
            .HasDatabaseName("IX_Units_Name");
        builder.HasIndex(e => e.IsArchived)
            .HasDatabaseName("IX_Units_IsArchived");
            
        // Relationships
        builder.HasMany(e => e.ReceiptItems)
            .WithOne(e => e.Unit)
            .HasForeignKey(e => e.UnitId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasMany(e => e.ShipmentItems)
            .WithOne(e => e.Unit)
            .HasForeignKey(e => e.UnitId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasMany(e => e.Balances)
            .WithOne(e => e.Unit)
            .HasForeignKey(e => e.UnitId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
